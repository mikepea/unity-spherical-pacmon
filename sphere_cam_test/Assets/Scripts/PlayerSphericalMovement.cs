using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private Vector2 playerGridRef = new Vector2 (0, 0);
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
        playerGridRef = map.FindEntityGridCell (startMarkerTag);
        float[] mapRef = map.LatitudeLongitudeAtGridReference (playerGridRef);
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

        if ( this.gameObject.tag == "Player" ) {
          // we don't support computer control of player
          return direction;
        } else {
          // we are a baddy
          List<Vector2> availableDirections = map.AvailableDirectionsAtGridRef(playerGridRef);
          Vector2 target = GameObject.FindWithTag("Player").GetComponent<PlayerSphericalMovement>().GridRef();
          if ( availableDirections.Count == 1 ) {
            direction = availableDirections[0]; // always take only available dir.
          } else if ( availableDirections.Count == 2 && direction != Vector2.zero ) {
            availableDirections.Remove(-direction); // cannot reverse
            direction = availableDirections[0];
          } else {
            //availableDirections.Remove(-direction); // cannot reverse
            // work out which direction takes us closest to target
            float lowest = 100000000.0F;
            availableDirections.Remove(-direction); // cannot reverse
            foreach ( Vector2 dir in availableDirections) {
              // invert the Y value of dir, as DOWN actually increases Y in
              // grid :(
              Vector2 newLocation = playerGridRef + dir;
              float dist = Vector2.SqrMagnitude(newLocation - target);
              Debug.Log(this.name + " at " + playerGridRef + " going " + dir + ", distance from " + newLocation + " to " + target + " = " + dist);
              if ( dist < lowest ) {
                lowest = dist;
                direction = dir;
              }
            }
            Debug.Log(this.name + " going " + direction + " because WOO CHOICE");
          }
          return direction;
        }

    }

    Vector2 GridRef ()
    {
        return playerGridRef;
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
            playerIntendedDirection = NextComputerDirection (playerDirection);
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
            if (map.WallAtGridReference ((int)playerGridRef.x, (int)playerGridRef.y + (int)playerIntendedDirection.y)) {
                lastAutoDirectionChangeTime = 0;
            } else if (dist < nextMoveSpeed.x) {
                // can turn -- we're on/about to be on a grid line
                currentAngleX = Mathf.Round (currentAngleX + (playerDirection.x * dist)); // normalise angle to grid
                playerDirection = playerIntendedDirection;
            }
        } else if (playerIntendedDirection.x != 0) {
            // player is going north/south, wants to go east/west
            if (map.WallAtGridReference (map.NormalizeGridX ((int)playerGridRef.x + (int)playerIntendedDirection.x), (int)playerGridRef.y)) {
                lastAutoDirectionChangeTime = 0;
            } else if (dist < nextMoveSpeed.y) {
                // can turn -- we're on/about to be on a grid line
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
            && map.WallAtGridReference ((int)playerGridRef.x, (int)playerGridRef.y + (int)playerDirection.y)
            ) {
            // going north/south, blocked by wall.
            currentAngleY = Mathf.Round (currentAngleY + (playerDirection.y * dist)); // normalise angle to grid
            if ( this.name == "Player" ) {
              playerDirection = Vector2.zero;
            } else {
              playerDirection = playerIntendedDirection; // baddies dont stop
            }
            lastAutoDirectionChangeTime = 0;
        } else if (playerDirection.x != 0
            && dist < nextMoveSpeed.x
            && map.WallAtGridReference (map.NormalizeGridX ((int)playerGridRef.x + (int)playerDirection.x), (int)playerGridRef.y)
                   ) {
            // going east/west, blocked by wall.
            currentAngleX = Mathf.Round (currentAngleX + (playerDirection.x * dist)); // normalise angle to grid
            if ( this.name == "Player" ) {
              playerDirection = Vector2.zero;
            } else {
              playerDirection = playerIntendedDirection; // baddies dont stop
            }
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
        playerGridRef = map.GridReferenceAtLatitudeLongitude (currentAngleY, currentAngleX);
        Vector2 nextMoveSpeed = new Vector2 (
                (speed * Time.deltaTime * map.LatitudeSpeedAdjust (currentAngleY)),
                (speed * Time.deltaTime)
        );

        ChangeDirectionIfAble (nextMoveSpeed);

        MoveUnlessBlockedByWall (nextMoveSpeed);

        NormalizeAngles ();

        /*
        Debug.Log ("MIKEDEBUG: "
            + this.name
            + " gridRef: " + playerGridRef
            + " currentX: " + currentAngleX
            + " currentY: " + currentAngleY
            + " dist: " + map.angularDistanceToNextGridLine (currentAngleY, currentAngleX, playerDirection)
            + " dirX: " + playerDirection.x
            + " dirY: " + playerDirection.y
            + " IdirX: " + playerIntendedDirection.x
            + " IdirY: " + playerIntendedDirection.y
        );
        */
    }

}
