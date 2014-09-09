using UnityEngine;
using System.Collections;

public class GlobalGameDetails : MonoBehaviour
{

    public static float maxAngleY = 70F;
    public static float minAngleY = -70F;
    public static float cellSpacing = 5F;
    public static int mapRows = 29;
    public static int mapColumns = 72;
    public static GlobalGameDetails i;

    public float blindOffset;
    public bool displayTestPattern = false;

    public int initialMapNumber;
    public int numberOfMaps;
    private int mapNumber = 0;
    public bool disableAudio;
    private bool audioEnabled = true;

    private bool movementEnabled = false;

    public Texture2D mapTiles;

    private bool demoMode = true;
    private string gameMode = "Init";
    private float gameModeStartTime = 0;
    private float levelStartTime = 0;
    private float demoStartDelay = 10; // seconds

    public string nextGameModeKey;
    public string nextLevelKey;

    private int score;
    private int highScore;

    public void Start() {

        if ( disableAudio ) {
          DisableAudio();
        }

        if ( gameMode == "Init" ) {
          ZeroScore();
          ResetMapNumber();
          StartGameDemo();
        }

    }

    public void ResetMapNumber() {
      if ( displayTestPattern ) {
        mapNumber = 0;
      } else {
        mapNumber = initialMapNumber;
      }
    }

    public float BlindOffset() {
        return blindOffset;
    }

    public float LevelStartTime() {
      return levelStartTime;
    }

    public bool MovementEnabled() {
      return movementEnabled;
    }

    public bool InDemoMode() {
      Debug.Log ( "InDemoMode: " + demoMode );
      return demoMode;
    }

    public void EnableMovement() {
      movementEnabled = true;
    }

    public void DisableMovement() {
      movementEnabled = false;
    }

    public void ZeroScore() {
      score = 0;
    }

    public void IncreaseScore(int increment) {
      if ( ! InDemoMode() ) score += increment;
    }

    public int Score() {
        return score;
    }

    public int HighScore() {
        return highScore;
    }

    public void SetHighScore(int score) {
      if ( ! InDemoMode() ) highScore = score;
    }

    public string MapName() {
        return "map" + mapNumber;
    }

    public void NextMap() {
      if ( displayTestPattern ) {
        mapNumber = 0;
      } else {
        mapNumber++;
        mapNumber = mapNumber % numberOfMaps;
        if ( mapNumber == 0 )  mapNumber++; //  always skip map0 -- test pattern
      }
      DisableMovement();
      Debug.Log("MIKEDEBUG: (in NextMap) " + GameMode() );
      Application.LoadLevel(0);
    }

    public bool AudioEnabled() {
        return audioEnabled;
    }

    public void DisableAudio() {
        audioEnabled = false;
    }

    public void EnableAudio() {
      if ( ! disableAudio ) {
        audioEnabled = true;
      }
    }

    public string GameMode() {
        return gameMode;
    }

    public float GameModeStartTime() {
        return gameModeStartTime;
    }

    public void Awake() {
      if ( !i ) {
        i = this;
        DontDestroyOnLoad(this);
      } else {
        Destroy(gameObject);
      }
    }

    public void GameStart() {
      gameMode = "GameStart";
      demoMode = false;
      ZeroScore();
      EnableAudio();
      ResetMapNumber();
      Application.LoadLevel(0);
    }

    public void StartLevel() {
      gameMode = "StartLevel";
      levelStartTime = Time.time;
      gameModeStartTime = Time.time;
      DisableMovement();
    }

    public void GameOver() {
      if ( gameMode != "GameOver" ) {
        gameMode = "GameOver";
        gameModeStartTime = Time.time;
        DisableAudio();
      }
    }

    public void StartGameDemo() {
      demoMode = true;
      gameMode = "GameStart";
      DisableAudio();
      ResetMapNumber();
      Application.LoadLevel(0);
    }

    public void GameInProgress() {
      gameModeStartTime = Time.time;
      gameMode = "GameInProgress";
    }

    public void FixedUpdate() {
      Debug.Log("MIKEDEBUG: " + GameMode() );
      if (Input.GetKey (nextGameModeKey)) {
        Debug.Log("nextGameModeKeyPressed: " + gameMode);
        if ( demoMode ) {
          demoMode = false;
          GameStart();
        } else {
          StartGameDemo();
        }
      }

      if (Input.GetKey (nextLevelKey)) {
        Debug.Log("nextLevelKeyPressed");
        NextMap();
      }

      Debug.Log("GameMode " + GameMode() + " start time " + GameModeStartTime());
      if ( GameMode() == "GameOver" && 
           GameModeStartTime() + demoStartDelay < Time.time
           ) {
        StartGameDemo();
      }
    }

}
