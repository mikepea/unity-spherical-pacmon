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

    public int mapNumber = 0;
    public bool disableAudio;
    private bool audioEnabled = true;

    public Texture2D mapTiles;

    private string gameMode = "Init";

    public string nextGameModeKey;

    private int score;
    private int highScore;

    public void Start() {

        if ( disableAudio ) {
          DisableAudio();
        }

        if ( gameMode == "Init" ) {
          ZeroScore();
          GameDemo();
        }

    }

    public void ZeroScore() {
      score = 0;
    }

    public void IncreaseScore(int increment) {
      score += increment;
    }

    public int Score() {
        return score;
    }

    public int HighScore() {
        return highScore;
    }

    public void SetHighScore(int score) {
        highScore = score;
    }

    public string MapName() {
        return "map" + mapNumber;
    }

    public void NextMap() {
        mapNumber++;
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
      EnableAudio();
      Application.LoadLevel(0);
    }

    public void GameOver() {
      gameMode = "GameOver";
      DisableAudio();
    }

    public void GameDemo() {
      gameMode = "GameDemo";
      DisableAudio();
      Application.LoadLevel(0);
    }

    public void GameInProgress() {
      gameMode = "GameInProgress";
    }

    public void FixedUpdate() {
      Debug.Log("MIKEDEBUG: " + GameMode() );
      if (Input.GetKey (nextGameModeKey)) {
        Debug.Log("nextGameModeKeyPressed: " + gameMode);
        if ( gameMode == "GameDemo" ) {
          GameStart();
        } else {
          GameDemo();
        }
      }
    }

}
