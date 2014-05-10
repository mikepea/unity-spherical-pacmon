using UnityEngine;
using System.Collections;

public class AddWallMarkers : MonoBehaviour
{

    public GameObject wallObject;
    private float latitudeMin = GlobalGameDetails.minAngleY;
    private float latitudeMax = GlobalGameDetails.maxAngleY;
        
    private float gridSpacing = GlobalGameDetails.cellSpacing;
        
    void Start ()
    {
            
        Map map = new Map (GlobalGameDetails.mapName);
            
        int pillCount = 0;
            
        for (int gridX = 0; gridX < GlobalGameDetails.mapColumns; gridX++) {
            for (int gridY = 0; gridY < GlobalGameDetails.mapRows; gridY++) {
                if (map.WallAtGridReference (gridX, gridY)) {
                    float[] latLongRef = map.LatitudeLongitudeAtGridReference (gridX, gridY);
                    float latitude = latLongRef [0];
                    float longitude = latLongRef [1];
                        
                    Debug.Log ("DEBUG: Placing wall at latitude: " + latitude + ", longitude: " + longitude);
                    SphericalCoordinates sc = new SphericalCoordinates (
                            0.5f, 
                            degreesToRadians (longitude), 
                            degreesToRadians (latitude),
                            0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f)
                    );
                    Vector3 newPillPosition = sc.toCartesian;
                    GameObject pill = Instantiate (wallObject) as GameObject;
                    pillCount++;
                    pill.transform.parent = transform;
                    pill.transform.localPosition = newPillPosition;
                }
            }
        }
    }
        
    float degreesToRadians (float degrees)
    {
        return (degrees * Mathf.PI / 180f);
    }

}