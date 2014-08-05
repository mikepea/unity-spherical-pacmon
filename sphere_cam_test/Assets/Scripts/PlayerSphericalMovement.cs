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
    private int ticksInScaredMode = 0;
    private int maxTicksInScaredMode = 300;
    private int alertScaredModeTimeout = 50;

    public Map map = new Map (GlobalGameDetails.mapName);

    private int tileSize = 200;
    private int tile = 0;
    private int lastTileChangeTicks = 0;
    private int maxLastTileChangeTicks = 5;
    private bool playerTileBounceDirection = true;

    void Start ()
    {
        sc = new SphericalCoordinates (transform.localPosition, 0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f));
        transform.localPosition = sc.toCartesian;
        PutPlayerAtStartPosition ();
        isDead = false;
        isScared = false;

        Texture2D sprite = new Texture2D (200, 200);
        Color[] tileColor = regularSprite.GetPixels(tileSize * tile, 0, tileSize, tileSize);
        sprite.SetPixels(tileColor);
        sprite.Apply();
        renderer.material.mainTexture = sprite;

    }

    void PutPlayerAtStartPosition ()
    {
        playerGridRef = map.FindEntityGridRef (startMarkerTag);
        if ( playerGridRef == new Vector2 (-1, -1) ) {
            this.gameObject.SetActive (false);
        }
        float[] mapRef = map.LatitudeLongitudeAtGridReference (playerGridRef);
        currentAngleX = mapRef [1];
        currentAngleY = mapRef [0];
        isScared = false;
        isDead = false;
        isHome = true;
    }

    void EnterScaredMode ()
    {
        if ( ! isDead || ! isHome ) {
          isScared = true;
          ticksInScaredMode = 0;
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

    Vector2 TargetLookahead (Vector2 dir)
    {
        if ( dir == Vector2.up ) {
            return new Vector2 (-1, 0); // emulate 'up bug'
        } else if ( dir == - Vector2.up) {
            return new Vector2 (1, -1);
        } else if ( dir == Vector2.right) {
            return new Vector2 (1, 1);
        } else {
            return new Vector2 (-1, -1);
        }
    }

    Vector2 NextComputerDirection (Vector2 direction)
    {

        if ( this.gameObject.tag == "Player" ) {
          // we don't support computer control of player
          return direction;
        } else {

          // we are a baddy, work out which tile we are targetting
          List<Vector2> availableDirections = map.AvailableDirectionsAtGridRef(playerGridRef);
          Vector2 target;
          if ( isScared == true ) {
            target = playerScatterSpot;
          } else if ( isDead == true ) {
            target = map.FindEntityGridRef("Baddy2Start"); // the baddy home box centre
          } else {
            // attack!
            Vector2 playerLoc = GameObject.FindWithTag("Player").GetComponent<PlayerSphericalMovement>().GridRef();
            Vector2 playerDir = GameObject.FindWithTag("Player").GetComponent<PlayerSphericalMovement>().PlayerDirection();
            if ( this.name == "Baddy1" ) {
              target = playerLoc;
            } else if ( this.name == "Baddy2" ) {
              target = playerLoc + Vector2.Scale(TargetLookahead(playerDir), new Vector2 (4, 4));
            } else if ( this.name == "Baddy3" ) {
              target = playerLoc + Vector2.Scale(TargetLookahead(playerDir), new Vector2 (6, 6));
            } else if ( this.name == "Baddy4" ) {
              target = playerLoc + Vector2.Scale(TargetLookahead(playerDir), new Vector2 (2, 2));
            } else {
              target = playerLoc;
            }
          }

          if ( map.IsEntityAtGridRef("BaddyDoor", playerGridRef - Vector2.up ) && ! isDead ) {
              // remove down as a direction, as we cannot go thru BaddyDoor
              availableDirections.Remove(-Vector2.up);
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

    Vector2 PlayerDirection ()
    {
        return playerDirection;
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

        if ( IAmABaddy() ) {
          UpdateBaddyState ();
        }
        UpdateNextPlayerPosition ();

        UpdatePlayerObjectLocationAndRotation ();
    }

    void UpdateBaddyState () {
        if ( isScared ) {
            if ( ticksInScaredMode > maxTicksInScaredMode ) {
                isScared = false;
                ticksInScaredMode = 0;
            } else {
                ticksInScaredMode++;
            }
        } else if ( isDead ) {
            if ( playerGridRef == map.FindEntityGridRef("Baddy2Start") ) {
                // made it home!
                isDead = false;
            }
        }
    }

    void UpdatePlayerObjectLocationAndRotation ()
    {
        transform.localPosition = sc.SetRotation (
            map.degreesToRadians (currentAngleX),
            map.degreesToRadians (currentAngleY)
        ).toCartesian;
        transform.LookAt (Vector3.zero);
        UpdatePlayerAnimation();
    }
    
    void UpdatePlayerAnimation() {
      if ( this.gameObject.tag == "Player" ) {
        TransformPlayerForSpriteAnimation ();
        UpdatePlayerSprite();
      } else {
        UpdateBaddySprite();
      }
    }

    void UpdatePlayerSprite() {
      int numPlayerTiles = 6;
      Texture2D sprite = new Texture2D (200, 200);

      if ( ! ( playerDirection == Vector2.zero ) ) {
          lastTileChangeTicks++;
          if ( lastTileChangeTicks > maxLastTileChangeTicks ) {
              if ( playerTileBounceDirection ) {
                tile++;
              } else {
                tile--;
              }
              if ( tile == 0 ) {
                playerTileBounceDirection = true;
              } else if ( tile == numPlayerTiles - 1 ) {
                playerTileBounceDirection = false;
              }

              Color[] tileColor = regularSprite.GetPixels(tileSize * tile, 0, tileSize, tileSize);
              sprite.SetPixels(tileColor);
              sprite.Apply();
              renderer.material.mainTexture = sprite;
         }
      }
    }

    void UpdateBaddySprite() {
      int tile = 0;
      Texture2D baseSprite;
      Texture2D sprite = new Texture2D (200, 200);

      if ( isDead == true ) {
        baseSprite = deadSprite;
      } else if ( isScared == true ) {
        baseSprite = scaredSprite;
        if ( ( maxTicksInScaredMode - ticksInScaredMode ) < alertScaredModeTimeout )  {
          if ( ( ticksInScaredMode / 10 ) % 2 == 1 ) {
            tile = 1;
          }
        }
      } else {
        baseSprite = regularSprite;
        if ( playerDirection == Vector2.up ) {
          tile = 0;
        } else if ( playerDirection == Vector2.right ) {
          tile = 1;
        } else if ( playerDirection == - Vector2.up ) {
          tile = 2;
        } else if ( playerDirection == - Vector2.right ) {
          tile = 3;
        } else {
          tile = 0;
        }

      }

      Color[] tileColor = baseSprite.GetPixels(tileSize * tile, 0, tileSize, tileSize);
      sprite.SetPixels(tileColor);
      sprite.Apply();
      renderer.material.mainTexture = sprite;

    }


    void TransformPlayerForSpriteAnimation ()
    {
        Vector2 directionToFace = playerDirection;
        if (playerDirection == Vector2.zero) {
            directionToFace = playerIntendedDirection;
        }
        if (directionToFace.x < 0) {
            transform.Rotate (Vector3.forward, 180);
        } else if (directionToFace.y > 0) {
            transform.Rotate (Vector3.forward, 90);
        } else if (directionToFace.y < 0) {
            transform.Rotate (Vector3.forward, 270);
        }

    }

    bool IAmABaddy() {
        return ( this.gameObject.tag == "Baddy" );
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
        if ( map.IsEntityAtGridRef("BaddyDoor", playerGridRef + playerIntendedDirection) ) {
          if ( IAmABaddy() ) {
            // we want to enter/leave the baddy house
            if ( isDead && ( playerIntendedDirection == - Vector2.up ) ) {
              // sweet, door is open
            } else if ( ! isDead && ( playerIntendedDirection == Vector2.up ) ) {
              // sweet, leaving - door is open
            } else {
              // treat it like a wall
              return;
            }
          } else {
            return; // player cannot go thru door, ever
          }
        }

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
