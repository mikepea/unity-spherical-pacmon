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

    public Texture2D regularSprite;
    public Texture2D scaredSprite;
    public Texture2D deadSprite;

    public Vector2 playerScatterSpot;

    public float speed = 20.0F;
    public bool humanControl;

    private float currentAngleX = 0F;
    private float currentAngleY = 0F;
    private float maxAngleY = GlobalGameDetails.maxAngleY;
    private float minAngleY = GlobalGameDetails.minAngleY;
    private Vector2 playerGridRef = new Vector2 (0, 0);

    private bool isScared = false;
    private bool isDead = false;
    private bool isHome = true;

    public Map map = new Map (GlobalGameDetails.mapName);

    void Start ()
    {
        sc = new SphericalCoordinates (transform.localPosition, 0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f));
        transform.localPosition = sc.toCartesian;
        PutPlayerAtStartPosition ();
    }

    void PutPlayerAtStartPosition ()
    {
        playerGridRef = map.FindEntityGridRef (startMarkerTag);
        float[] mapRef = map.LatitudeLongitudeAtGridReference (playerGridRef);
        currentAngleX = mapRef [1];
        currentAngleY = mapRef [0];
    }

    void EnterScaredMode ()
    {
        if ( ! isDead || ! isHome ) {
          isScared = true;
        }
    }

    public bool IsScared () {
      return isScared;
    }

    public bool IsDead () {
      return isDead;
    }

    void EnterDeadMode ()
    {
        isScared = false;
        isDead = true;
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
          Vector2 target;
          if ( isScared == true ) {
            target = playerScatterSpot;
          } else if ( isDead == true ) {
            target = map.FindEntityGridRef("Baddy2Start"); // the baddy home box centre
          } else {
            // TODO: Different hunt target per baddy
            target = GameObject.FindWithTag("Player").GetComponent<PlayerSphericalMovement>().GridRef();
          }

          if ( availableDirections.Count == 1 ) {
            direction = availableDirections[0]; // always take only available dir.
          } else if ( availableDirections.Count == 2 && direction != Vector2.zero ) {
            availableDirections.Remove(-direction); // cannot reverse
            direction = availableDirections[0];
          } else {
            // work out which direction takes us closest to target
            float lowest = 100000000.0F;
            availableDirections.Remove(-direction); // cannot reverse
            foreach ( Vector2 dir in availableDirections) {
              Vector2 newLocation = playerGridRef + dir;
              float dist = map.DistanceBetween(newLocation, target);
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

        if ( this.name != "Player" ) {
          UpdateBaddyState ();
        }
        UpdateNextPlayerPosition ();

        UpdatePlayerObjectLocationAndRotation ();
    }

    void UpdateBaddyState () {
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
            if ( isDead == true ) {
              renderer.material.mainTexture = deadSprite;
            } else if ( isScared == true ) {
              renderer.material.mainTexture = scaredSprite;
            } else {
              renderer.material.mainTexture = regularSprite;
            }
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

        if ( map.WallAtGridReference(playerGridRef + playerIntendedDirection) ) {
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
            if (dist < nextMoveSpeed.x) {
                // can turn -- we're on/about to be on a grid line
                currentAngleX = Mathf.Round (currentAngleX + (playerDirection.x * dist)); // normalise angle to grid
                playerDirection = playerIntendedDirection;
            }
        } else if (playerIntendedDirection.x != 0) {
            if (dist < nextMoveSpeed.y) {
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
            && map.WallAtGridReference (playerGridRef + playerDirection)
            ) {
            // going north/south, blocked by wall.
            currentAngleY = Mathf.Round (currentAngleY + (playerDirection.y * dist)); // normalise angle to grid
            if ( this.name == "Player" ) {
              playerDirection = Vector2.zero;
            } else {
              playerDirection = playerIntendedDirection; // baddies dont stop
            }
        } else if (playerDirection.x != 0
            && dist < nextMoveSpeed.x
            && map.WallAtGridReference (playerGridRef + playerDirection)
                   ) {
            // going east/west, blocked by wall.
            currentAngleX = Mathf.Round (currentAngleX + (playerDirection.x * dist)); // normalise angle to grid
            if ( this.name == "Player" ) {
              playerDirection = Vector2.zero;
            } else {
              playerDirection = playerIntendedDirection; // baddies dont stop
            }
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

    Vector2 PlayerSpeed ()
    {
        float baseSpeed = speed;

        if ( isDead == true ) {
          baseSpeed = 50;
        }
        Vector2 newSpeed = new Vector2 (
                (baseSpeed * Time.deltaTime * map.LatitudeSpeedAdjust (currentAngleY)),
                (baseSpeed * Time.deltaTime)
            );
        return newSpeed;
    }

    void UpdateNextPlayerPosition ()
    {
        playerGridRef = map.GridReferenceAtLatitudeLongitude (currentAngleY, currentAngleX);
        Vector2 nextMoveSpeed = PlayerSpeed();

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
