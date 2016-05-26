using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using InControl;

[RequireComponent(typeof(AudioSource))]
public class PlayerSphericalMovement : MonoBehaviour
{

    public SphericalCoordinates sc;
    private Vector2 playerDirection = new Vector2 (0, 0);
    private Vector2 playerIntendedDirection = new Vector2 (0, 0);

    public string startMarkerTag;
    public bool simpleBaddyBehaviour;

    public int inputManagerDeviceIndex;
    private InputDevice inputdev;

    public Vector2 playerScatterSpot;

    public float speed = 20.0F;
    public bool humanControlEnabled;
    private bool humanControl = false;
    public bool animatePlayer;

    private bool highlightSprite = false;

    public AudioClip startSound;
    public AudioClip deadSound;

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
    private float gameStartTime = 0;
    private float gameStartDelay = 4.0F;
    private float levelStartDelay = 4.0F;

    private float lastControllerRefreshTime = 0.0F;
    private float lastControllerRefreshDelay = 5.0F;

    private Map map;

    private int tile = 0;
    private int lastTileChangeTicks = 0;
    private int maxLastTileChangeTicks = 1;
    private int maxDeadLastTileChangeTicks = 10;
    private bool playerTileBounceDirection = true;

    private float iX=0;
    private float iY=1;
    public int _uvTieX;
    public int _uvTieY;
    private Vector2 _size;
    private Renderer _myRenderer;
    private int _lastIndex = -1;

    private GlobalGameDetails ggd;

    private GameObject infoDisplay;

    GlobalGameDetails GlobalState() {
        if (!ggd) {
          GameObject[] states = GameObject.FindGameObjectsWithTag ("PersistedState");
          ggd = states[0].GetComponent<GlobalGameDetails>();
        }
        return ggd;
    }

    void Start ()
    {

        RefreshControllers();

        GameObject[] infoDisplays = GameObject.FindGameObjectsWithTag("InfoDisplay");
        infoDisplay = infoDisplays[0];
        SetInfoDisplayText("");
        DisableInfoDisplay();

        string mapName = GlobalState().MapName();
        map = new Map (mapName);
        Debug.Log(this.name + ": In PlayerSphericalMovement.Start, mapName = " + mapName);
        Debug.Log(this.name + ": Game Mode: " + GlobalState().GameMode());

        string mode = GlobalState().GameMode();
        if ( mode == "GameStart" ) {
          gameStartTime = Time.time;
          if ( this.name == "Player" ) {
            if ( ! GlobalState().InDemoMode() ) {
              if ( GlobalState().AudioEnabled() ) {
                GetComponent<AudioSource>().PlayOneShot(startSound);
              }
            }
            StartLevel();
          }
        } else if ( mode == "GameInProgress" ) {
          // new map started.
          StartLevel();
        }

        sc = new SphericalCoordinates (transform.localPosition, 0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f));
        transform.localPosition = sc.toCartesian;
        PutPlayerAtStartPosition ();
        isDead = false;
        isScared = false;

        _size = new Vector2 (1.0f / _uvTieX, 1.0f / _uvTieY);
        _myRenderer = GetComponent<Renderer>();
        _myRenderer.material.SetTextureScale ("_MainTex", _size);

    }

    public void RefreshControllers() {
      lastControllerRefreshTime = Time.time;

      if ( humanControlEnabled ) {
        List<InputDevice> devices = InputManager.Devices;
        Debug.Log ( this.name + ": found " + devices.Count + " inputdevs");
        if ( inputManagerDeviceIndex + 1 > devices.Count ) {
          Debug.Log ( this.name + ": device " + inputManagerDeviceIndex + " is not present");
          humanControl = false;
        } else {
          inputdev = devices[inputManagerDeviceIndex];
          if ( IsNull(inputdev) ) {
            Debug.Log ( this.name + ": device + " + inputManagerDeviceIndex + " is not Active");
            humanControl = false;
          } else {
            if ( this.name != "Player" && inputdev.Name == "Keyboard Overlay" ) {
              Debug.Log ( this.name + ": disabling, as cannot use Keyboard Overlay" );
              humanControl = false;
            } else {
              Debug.Log ( this.name + ": using InputDevice " + inputManagerDeviceIndex + ": " + inputdev.Name + ":" + inputdev.Meta );
              if ( ! GlobalState().InDemoMode() ) humanControl = true;
            }
          }
        }
      }
    }

    public static bool IsNull(System.Object aObj) {
      return aObj == null || aObj.Equals(null);
    }

    public void EnableInfoDisplay() {
      infoDisplay.GetComponent<Renderer>().enabled = true;
    }

    public void DisableInfoDisplay() {
      infoDisplay.GetComponent<Renderer>().enabled = false;
    }

    public void SetInfoDisplayText ( string message ) {
      infoDisplay.GetComponent<TextMesh>().text = message;
    }

    void PutPlayerAtStartPosition ()
    {
        playerGridRef = map.FindEntityGridRef (startMarkerTag);
        if ( playerGridRef == new Vector2 (-1, -1) ) {
            this.gameObject.SetActive (false);
            return;
        }
        float[] mapRef = map.LatitudeLongitudeAtGridReference (playerGridRef);
        currentAngleX = mapRef [1];
        currentAngleY = mapRef [0];
        isScared = false;
        isDead = false;
        isHome = true;
        UpdatePlayerObjectLocationAndRotation ();
        this.GetComponent<Renderer>().enabled = true;
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
        HasDied();
    }

    Vector2 ProcessInputsIntoDirection (Vector2 direction)
    {
        if ( humanControl ) {
          float h = inputdev.Direction.X;
          float v = inputdev.Direction.Y;

          if (h > 0.8) {
              direction = Vector2.right;
          } else if (h < -0.8) {
              direction = (- Vector2.right);
          } else if (v > 0.8) {
              direction = Vector2.up;
          } else if (v < -0.8) {
              direction = (- Vector2.up);
          } else if ( IAmABaddy() ) {
              // allow baddy to say 'i dont care'
              direction = Vector2.zero;
          } else {
              // continue in existing direction
          }
          Debug.Log(this.name + ": being asked to go " + direction);
        }

        return NextComputerDirection (playerDirection, direction);

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

    Vector2 GetPlayerAiTarget() {
      Vector2 target = Vector2.zero;
      int random = Random.Range(0,3);
      if ( random > 1 ) {
        target = new Vector2 (40,20);
      }
      return target;
    }

    Vector2 GetBaddyAiTarget() {
      Vector2 target = Vector2.zero;
      if ( isScared ) {
        target = playerScatterSpot;
        Debug.Log(this.name + " is scared, and heading to " + target);
      } else if ( isDead ) {
        target = map.FindEntityGridRef("Baddy2Start"); // the baddy home box centre
        Debug.Log(this.name + " is dead, and heading to " + target);
      } else if ( humanControl ) {
        // don't help out the human!
        target = playerScatterSpot;
        Debug.Log(this.name + " is human controlled, and heading to " + target);
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
        //Debug.Log(this.name + " is on the attack! and heading to " + target);
      }
      return target;
    }

    Vector2 NextComputerDirection (Vector2 current, Vector2 intended)
    {

        Vector2 direction = current;
        Vector2 target;

        if ( this.gameObject.tag == "Player" && humanControl ) {
          return intended;
        }

        if ( this.gameObject.tag == "Baddy" && humanControl && simpleBaddyBehaviour ) {
          return intended;
        }

        if ( this.gameObject.tag == "Player" ) {
          target = GetPlayerAiTarget();
        } else {
          target = GetBaddyAiTarget();
        }

        // under AI/partial AI control - work out where we want to head to.
        List<Vector2> availableDirections = map.AvailableDirectionsAtGridRef(playerGridRef);

        if ( map.IsEntityAtGridRef("BaddyDoor", playerGridRef - Vector2.up ) && ! isDead ) {
          // remove down as a direction, as we cannot go thru BaddyDoor
          availableDirections.Remove(-Vector2.up);
        }

        if ( availableDirections.Count == 1 ) {
          direction = availableDirections[0]; // always take only available dir.
        } else if ( availableDirections.Count == 2 && current != Vector2.zero ) {
          availableDirections.Remove(-current); // cannot reverse
          direction = availableDirections[0];
        } else {
          // work out which direction takes us closest to target
          float lowest = 100000000.0F;
          availableDirections.Remove(-current); // cannot reverse

            // just baddy humanControl override here.
            if ( humanControl ) {
              foreach ( Vector2 dir in availableDirections) {
                if ( dir == intended ) {
                  return dir; // override with the players choice
                }
              }
              if ( ! isDead ) {
                foreach ( Vector2 dir in availableDirections) {
                  if ( dir == current ) {
                    return dir;  // prefer to continue in a straight line
                  }
                }
              }
            }

            foreach ( Vector2 dir in availableDirections) {
              Vector2 newLocation = playerGridRef + dir;
              float dist = map.DistanceBetween(newLocation, target);
              //Debug.Log(this.name + " at " + playerGridRef + " going " + dir + ", distance from " + newLocation + " to " + target + " = " + dist);
              if ( dist < lowest ) {
                lowest = dist;
                direction = dir;
              }
            }

        }
        return direction;
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

    void CheckForButtonPress() {
      if ( IsNull(inputdev) ) {
        return;
      }
      InputControl control = inputdev.GetControl( InputControlType.Action1 );
      if ( control.IsPressed ) {
        if ( this.name == "Player" ) {
          Debug.Log(this.name + ": Start Button pressed!");
          string mode = GlobalState().GameMode();
          if ( GlobalState().InDemoMode() || mode == "GameOver" ) {
            GlobalState().GameStart();
          }
        } else {
          // we are a baddy, button helps to figure out which one!
          highlightSprite = true;
        }
      } else {
        highlightSprite = false;
      }
    }

    void FixedUpdate ()
    {
        if ( lastControllerRefreshTime + lastControllerRefreshDelay < Time.time ) {
          RefreshControllers();
        }

        CheckForButtonPress();

        if ( GlobalState().MovementEnabled() ) {
          playerIntendedDirection = ProcessInputsIntoDirection (playerIntendedDirection);
        } else {
          if ( GlobalState().GameMode() == "StartLevel" &&
               Time.time > GlobalState().LevelStartTime() + levelStartDelay) {
            GlobalState().SendMessage("EnableMovement");
            playerDirection = - Vector2.right; // so wakka wakka begins :)
            SetInfoDisplayText("");
            DisableInfoDisplay();
            GlobalState().SendMessage("GameInProgress");
          }
        }

        if ( IAmABaddy() ) {
          UpdateBaddyState ();
        }
        UpdateNextPlayerPosition ();

        UpdatePlayerObjectLocationAndRotation ();
        UpdatePlayerAnimation();

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

      if ( ! animatePlayer ) {
        return;
      }

      int numAnimTiles = _uvTieX;

      if ( isDead ) {
          lastTileChangeTicks++;
          if ( tile >= _uvTieX ) {
            tile = 0;
          } else {
            if ( lastTileChangeTicks > maxDeadLastTileChangeTicks ) {
              lastTileChangeTicks = 0;
              tile++;
            }
          }

          if ( tile >= numAnimTiles ) {
            // finished dead player animation, restart level
            if ( GlobalState().GameMode() == "GameOver" ) {
              if ( this.name == "Player" ) {
                this.GetComponent<Renderer>().enabled = false;
                tile = numAnimTiles;
              }
            } else {
              StartLevel();
            }
          }

      } else if ( ! ( playerDirection == Vector2.zero ) ) {
          lastTileChangeTicks++;
          if ( lastTileChangeTicks > maxLastTileChangeTicks ) {
              lastTileChangeTicks = 0;
              if ( playerTileBounceDirection ) {
                tile++;
              } else {
                tile--;
              }
              if ( tile <= _uvTieX ) {
                playerTileBounceDirection = true;
                tile = _uvTieX;
              } else if ( tile >= _uvTieX + numAnimTiles - 1 ) {
                playerTileBounceDirection = false;
              }

         }
      }
      AlterTextureSpriteTile(tile);
    }

    void UpdateBaddySprite() {

      if ( ! animatePlayer ) {
        return;
      }

      if ( isScared == true ) {
        if ( ( maxTicksInScaredMode - ticksInScaredMode ) < alertScaredModeTimeout ) {
          tile = ( ticksInScaredMode / 10 ) % 2 + 8;
        } else {
          tile = 8;
        }
      } else {
        if ( playerDirection == Vector2.up ) {
          tile = 0 + 1*_uvTieX;
        } else if ( playerDirection == Vector2.right ) {
          tile = 1 + 1*_uvTieX;
        } else if ( playerDirection == - Vector2.up ) {
          tile = 2 + 1*_uvTieX;
        } else if ( playerDirection == - Vector2.right ) {
          tile = 3 + 1*_uvTieX;
        } else {
          tile = 0 + 1*_uvTieX;
        }
      }

      if ( isDead ) {
        tile -= _uvTieX; // 3rd row
      }

      if ( highlightSprite ) { tile = 9; } 

      AlterTextureSpriteTile(tile);
    }

    void AlterTextureSpriteTile(int index) {

      if (index != _lastIndex) {
        iX = index % _uvTieX;
        iY = index / _uvTieX;
        Vector2 offset = new Vector2(iX*_size.x, 1-(_size.y*iY));
        //Debug.Log(this.name + " using sprite tile " + tile + ", offset " + offset );
        _myRenderer.material.SetTextureOffset ("_MainTex", offset);
        _lastIndex = index;
      }

    }

    void TransformPlayerForSpriteAnimation ()
    {
        if ( isDead ) {
          return;
        }
        Vector2 directionToFace = playerDirection;
        if (playerDirection == Vector2.zero) {
            Debug.Log(this.name + ": stopped, so facing " + playerIntendedDirection);
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

    void HasDied() {
        isDead = true;
        if ( GlobalState().AudioEnabled() ) {
          GetComponent<AudioSource>().clip = deadSound;
          GetComponent<AudioSource>().Play();
        }
    }

    void GameOver() {
        GlobalState().GameOver();
        SetInfoDisplayText("GAME OVER");
        EnableInfoDisplay();
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

        if ( isDead && this.name == "Player" ) {
          baseSpeed = 0;
        } else if ( isDead ) {
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

    void StartLevel() {
      GlobalState().SendMessage("StartLevel");
      Debug.Log ("MIKEDEBUG: StartLevel() called! - mode = " + GlobalState().GameMode());
      ResetPlayerPositions();
      RefreshControllers();
      if ( this.name == "Player" && ! GlobalState().InDemoMode() ) {
        SetInfoDisplayText("READY!");
        EnableInfoDisplay();
      }
    }

    void ResetPlayerPositions ()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
        foreach (GameObject player in players) {
            player.SendMessage ("Stop");
            player.SendMessage ("PutPlayerAtStartPosition");
        }
        GameObject[] baddies = GameObject.FindGameObjectsWithTag ("Baddy");
        foreach (GameObject baddy in baddies) {
            baddy.SendMessage ("Stop");
            baddy.SendMessage ("PutPlayerAtStartPosition");
        }
    }

}
