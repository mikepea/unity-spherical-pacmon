using UnityEngine;
using System.Collections;

public class PlayerSphericalMovement : MonoBehaviour
{

    public SphericalCoordinates sc;
    private Vector2 playerDirection = new Vector2 (0, 0);
    private Vector2 playerIntendedDirection = new Vector2 (0, 0);

    public string startMarkerTag;
    public string inputHorizontalTag;
    public string inputVerticalTag;

    public float speed = 0.5F;
    public bool humanControl;

    private float currentAngleX = 0F;
    private float currentAngleY = 0F;
    private float maxAngleY = GlobalGameDetails.maxAngleY;
    private float minAngleY = GlobalGameDetails.minAngleY;
    private int playerGridX = 0;
    private int playerGridY = 0;
    private int lastAutoDirectionChangeTime = 0;
    public  int maxAutoDirectionChangeTime;

    public Map map = new Map (GlobalGameDetails.mapName);

    void Start ()
    {
        sc = new SphericalCoordinates (transform.localPosition, 0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f));
        transform.localPosition = sc.toCartesian;
        PutPlayerAtStartPosition ();
    }

    void PutPlayerAtStartPosition ()
    {
        int[] playerStartGridRef = map.FindEntityGridCell (startMarkerTag);
        playerGridX = playerStartGridRef [0];
        playerGridY = playerStartGridRef [1];
        float[] mapRef = map.LatitudeLongitudeAtGridReference (playerGridX, playerGridY);
        currentAngleX = mapRef [1];
        currentAngleY = mapRef [0];
    }

    Vector2 ProcessInputsIntoDirection (Vector2 direction)
    {
        float h = Input.GetAxisRaw (inputHorizontalTag);
        float v = Input.GetAxisRaw (inputVerticalTag);

        if (h == 1) {
            direction = Vector2.right;
        } else if (h == -1) {
            direction = (- Vector2.right);
        } else if (v == 1) {
            direction = Vector2.up;
        } else if (v == -1) {
            direction = (- Vector2.up);
        }
        return direction;
    }

    Vector2 NextComputerDirection (Vector2 direction)
    {
        if (lastAutoDirectionChangeTime % maxAutoDirectionChangeTime == 0) {
            float r = Random.Range (0, 4);
            if (r < 1) {
                direction = Vector2.right;
            } else if (r < 2) {
                direction = (- Vector2.right);
            } else if (r < 3) {
                direction = Vector2.up;
            } else {
                direction = (- Vector2.up);
            }
        }
        return direction;
    }

    void Stop ()
    {
        playerIntendedDirection = Vector2.zero;
        playerDirection = Vector2.zero;
    }

    void FixedUpdate ()
    {

        if (humanControl == true) {
            playerIntendedDirection = ProcessInputsIntoDirection (playerIntendedDirection);
        } else {
            playerIntendedDirection = NextComputerDirection (playerIntendedDirection);
            lastAutoDirectionChangeTime++;
        }

        UpdateNextPlayerPosition ();

        UpdatePlayerObjectLocationAndRotation ();
    }

    void UpdatePlayerObjectLocationAndRotation ()
    {
        transform.localPosition = sc.SetRotation (
            map.degreesToRadians (currentAngleX),
            map.degreesToRadians (currentAngleY)
        ).toCartesian;
        transform.LookAt (Vector3.zero);
        TransformPlayerForSpriteAnimation ();
    }

    void TransformPlayerForSpriteAnimation ()
    {
        Vector2 directionToFace = playerDirection;
        if (playerDirection == Vector2.zero) {
            directionToFace = playerIntendedDirection;
        }
        if (this.gameObject.tag == "Player") {
            if (directionToFace.x < 0) {
                transform.Rotate (Vector3.forward, 180);
            } else if (directionToFace.y > 0) {
                transform.Rotate (Vector3.forward, 90);
            } else if (directionToFace.y < 0) {
                transform.Rotate (Vector3.forward, 270);
            }
        } else if (this.gameObject.tag == "Baddy") {
        }

    }

    void ChangeDirectionIfAble (Vector2 nextMoveSpeed)
    {
        if (playerIntendedDirection == playerDirection) {
            // not changing direction
            return;
        }

        if ((playerIntendedDirection.x != 0 && playerDirection.x != 0) ||
            (playerIntendedDirection.y != 0 && playerDirection.y != 0)
           ) {
            // reversing, no need to check for grid lines or walls
            playerDirection = playerIntendedDirection;
            return;
        }

        // turning left or right - need to confirm we're about to cross
        // a grid line, and normalise the player onto the grid line.
        // NB: They should *always* be on the grid line in the direction
        //     they are travelling.
        // Also need to check if they would hit a wall
        float dist = map.angularDistanceToNextGridLine (currentAngleY, currentAngleX, playerDirection);
        if (playerIntendedDirection.y != 0) {
            // player is going east/west, wants to go north/south
            if (map.WallAtGridReference (playerGridX, playerGridY - (int)playerIntendedDirection.y)) {
                Debug.Log ("MIKEDEBUG: Wall at"
                    + " X: " + playerGridX
                    + " Y: " + (playerGridY - (int)playerIntendedDirection.y)
                );
                lastAutoDirectionChangeTime = 0;
            } else if (dist < nextMoveSpeed.x) {
                // can turn -- we're on/about to be on a grid line
                Debug.Log ("MIKEDEBUG: Turning North/South!");
                currentAngleX = Mathf.Round (currentAngleX + (playerDirection.x * dist)); // normalise angle to grid
                playerDirection = playerIntendedDirection;
            }
        } else if (playerIntendedDirection.x != 0) {
            // player is going north/south, wants to go east/west
            if (map.WallAtGridReference (map.NormalizeGridX (playerGridX + (int)playerIntendedDirection.x), playerGridY)) {
                Debug.Log ("MIKEDEBUG: Wall at"
                    + " X: " + (playerGridX + (int)playerIntendedDirection.x)
                    + " Y: " + playerGridY
                );
                lastAutoDirectionChangeTime = 0;
            } else if (dist < nextMoveSpeed.y) {
                // can turn -- we're on/about to be on a grid line
                Debug.Log ("MIKEDEBUG: Turning East/West!");
                currentAngleY = Mathf.Round (currentAngleY + (playerDirection.y * dist)); // normalise angle to grid
                playerDirection = playerIntendedDirection;
            }
        }
    }

    void MoveUnlessBlockedByWall (Vector2 nextMoveSpeed)
    {
        float dist = map.angularDistanceToNextGridLine (currentAngleY, currentAngleX, playerDirection);
        if (playerDirection.y != 0
            && dist < nextMoveSpeed.y
            && map.WallAtGridReference (playerGridX, playerGridY - (int)playerDirection.y)
            ) {
            // going north/south, blocked by wall.
            currentAngleY = Mathf.Round (currentAngleY + (playerDirection.y * dist)); // normalise angle to grid
            playerDirection = Vector2.zero;
            lastAutoDirectionChangeTime = 0;
        } else if (playerDirection.x != 0
            && dist < nextMoveSpeed.x
            && map.WallAtGridReference (map.NormalizeGridX (playerGridX + (int)playerDirection.x), playerGridY)
                   ) {
            // going east/west, blocked by wall.
            currentAngleX = Mathf.Round (currentAngleX + (playerDirection.x * dist)); // normalise angle to grid
            playerDirection = Vector2.zero;
            lastAutoDirectionChangeTime = 0;
        } else {
            // not about to hit wall, move as normal
            currentAngleX = (currentAngleX + playerDirection.x * nextMoveSpeed.x) % 360;
            currentAngleY = currentAngleY + playerDirection.y * nextMoveSpeed.y;
        }
    }

    void NormalizeAngles ()
    {
        if (currentAngleX < 0) {
            currentAngleX = 360 + currentAngleX;
        }
        if (currentAngleY < minAngleY) {
            currentAngleY = minAngleY;
        } else if (currentAngleY > maxAngleY) {
            currentAngleY = maxAngleY;
        }
    }

    void UpdateNextPlayerPosition ()
    {
        int[] gridRef = map.GridReferenceAtLatitudeLongitude (currentAngleY, currentAngleX);
        playerGridX = gridRef [0];
        playerGridY = gridRef [1];
        Vector2 nextMoveSpeed = new Vector2 (
                (speed * Time.deltaTime * map.LatitudeSpeedAdjust (currentAngleY)),
                (speed * Time.deltaTime)
        );

        ChangeDirectionIfAble (nextMoveSpeed);

        MoveUnlessBlockedByWall (nextMoveSpeed);

        NormalizeAngles ();

        Debug.Log ("MIKEDEBUG: "
            + " gridX: " + playerGridX
            + " gridY: " + playerGridY
            + " currentX: " + currentAngleX
            + " currentY: " + currentAngleY
            + " dist: " + map.angularDistanceToNextGridLine (currentAngleY, currentAngleX, playerDirection)
            + " dirX: " + playerDirection.x
            + " dirY: " + playerDirection.y
            + " IdirX: " + playerIntendedDirection.x
            + " IdirY: " + playerIntendedDirection.y
        );
    }

}
