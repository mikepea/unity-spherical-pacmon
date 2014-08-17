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

    public Texture2D mapTiles;

    public string MapName() {
        return "map" + mapNumber;
    }

    public void NextMap() {
        mapNumber++;
        //Application.LoadLevel(2);
        Application.LoadLevel(0);
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
