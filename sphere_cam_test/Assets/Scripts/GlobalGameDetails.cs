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
    public float blindTopX;
    public float blindTopY;
    public float blindTopRot;
    public float blindBottomX;
    public float blindBottomY;
    public float blindBottomRot;
    public float cameraFieldOfView;
    public float cameraYOffset;
    public float cameraDistanceFromOrigin;

    public float sphereSize;

    public bool displayTestPattern = false;
    public bool displayTestMap = false;

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
    public string increaseRotateSpeedKey;
    public string decreaseRotateSpeedKey;

    private int score;
    private int highScore;

    private GameObject mainSphere;

    public void Start ()
    {

        if (disableAudio) {
            DisableAudio ();
        }

        if (gameMode == "Init") {
            ZeroScore ();
            ResetMapNumber ();
            StartGameDemo ();
        }

    }

    public void ResetMapNumber ()
    {
        if (displayTestMap) {
            mapNumber = 0;
        } else {
            mapNumber = initialMapNumber;
        }
    }

    public float CameraYOffset ()
    {
        return cameraYOffset;
    }

    public float CameraDistanceFromOrigin ()
    {
        return cameraDistanceFromOrigin;
    }

    public float CameraFieldOfView ()
    {
        return cameraFieldOfView;
    }

    public float BlindOffset ()
    {
        return blindOffset;
    }

    public float BlindTopX ()
    {
        return blindTopX;
    }

    public float BlindTopY ()
    {
        return blindTopY;
    }

    public float BlindTopRot ()
    {
        return blindTopRot;
    }

    public float BlindBottomX ()
    {
        return blindBottomX;
    }

    public float BlindBottomY ()
    {
        return blindBottomY;
    }

    public float BlindBottomRot ()
    {
        return blindBottomRot;
    }

    public float SphereSize ()
    {
        return sphereSize;
    }

    public float LevelStartTime ()
    {
        return levelStartTime;
    }

    public bool MovementEnabled ()
    {
        return movementEnabled;
    }

    public bool InDemoMode ()
    {
        Debug.Log ("InDemoMode: " + demoMode);
        return demoMode;
    }

    public void EnableMovement ()
    {
        movementEnabled = true;
    }

    public void DisableMovement ()
    {
        movementEnabled = false;
    }

    public void ZeroScore ()
    {
        score = 0;
    }

    public void IncreaseScore (int increment)
    {
        if (! InDemoMode ())
            score += increment;
    }

    public int Score ()
    {
        return score;
    }

    public int HighScore ()
    {
        return highScore;
    }

    public void SetHighScore (int score)
    {
        if (! InDemoMode ())
            highScore = score;
    }

    public string MapName ()
    {
        return "map" + mapNumber;
    }

    public void NextMap ()
    {
        if (displayTestMap) {
            mapNumber = 0;
        } else {
            mapNumber++;
            mapNumber = mapNumber % numberOfMaps;
            if (mapNumber == 0)
                mapNumber++; //  always skip map0 -- test pattern
        }
        DisableMovement ();
        Debug.Log ("MIKEDEBUG: (in NextMap) " + GameMode ());
        Application.LoadLevel (0);
    }

    public bool AudioEnabled ()
    {
        return audioEnabled;
    }

    public void DisableAudio ()
    {
        audioEnabled = false;
    }

    public void EnableAudio ()
    {
        if (! disableAudio) {
            audioEnabled = true;
        }
    }

    public string GameMode ()
    {
        return gameMode;
    }

    public float GameModeStartTime ()
    {
        return gameModeStartTime;
    }

    public void Awake ()
    {
        if (!i) {
            i = this;
            DontDestroyOnLoad (this);
        } else {
            Destroy (gameObject);
        }
    }

    public void GameStart ()
    {
        gameMode = "GameStart";
        demoMode = false;
        ZeroScore ();
        EnableAudio ();
        ResetMapNumber ();
        Application.LoadLevel (0);
    }

    public void StartLevel ()
    {
        gameMode = "StartLevel";
        levelStartTime = Time.time;
        gameModeStartTime = Time.time;
        DisableMovement ();
    }

    public void GameOver ()
    {
        if (gameMode != "GameOver") {
            gameMode = "GameOver";
            gameModeStartTime = Time.time;
            DisableAudio ();
        }
    }

    public void StartGameDemo ()
    {
        demoMode = true;
        gameMode = "GameStart";
        DisableAudio ();
        ResetMapNumber ();
        Application.LoadLevel (0);
    }

    public void GameInProgress ()
    {
        gameModeStartTime = Time.time;
        gameMode = "GameInProgress";
    }

    public void FixedUpdate ()
    {

        if (displayTestPattern) {
            GameObject[] spheres = GameObject.FindGameObjectsWithTag ("MainSphere");
            mainSphere = spheres[0];
            mainSphere.SetActive(false);
            // This will lock the game, and a stop/play will be needed to
            // restart :(
        }

        Debug.Log ("MIKEDEBUG: " + GameMode ());
        if (Input.GetKey (nextGameModeKey)) {
            Debug.Log ("nextGameModeKeyPressed: " + gameMode);
            if (demoMode) {
                demoMode = false;
                GameStart ();
            } else {
                StartGameDemo ();
            }
        }

        if (Input.GetKey (nextLevelKey)) {
            Debug.Log ("nextLevelKeyPressed");
            NextMap ();
        }

        if (Input.GetKeyDown (increaseRotateSpeedKey)) {
            GameObject[] spheres = GameObject.FindGameObjectsWithTag ("MainSphere");
            mainSphere = spheres[0];
            Debug.Log ("increaseRotateSpeedKey pressed");
            mainSphere.SendMessage ("IncreaseSpeed");
        }
        if (Input.GetKeyDown (decreaseRotateSpeedKey)) {
            GameObject[] spheres = GameObject.FindGameObjectsWithTag ("MainSphere");
            mainSphere = spheres[0];
            Debug.Log ("decreaseRotateSpeedKey pressed");
            mainSphere.SendMessage ("DecreaseSpeed");
        }

        if (GameMode () == "GameOver" && 
            GameModeStartTime () + demoStartDelay < Time.time
           ) {
            StartGameDemo ();
        }
    }

}
