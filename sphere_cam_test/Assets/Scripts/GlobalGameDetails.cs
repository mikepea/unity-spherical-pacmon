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

    private string gameMode = "Demo";

    public void Start() {
        if ( disableAudio ) {
          DisableAudio();
        }
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

}
