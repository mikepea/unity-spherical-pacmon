using UnityEngine;
using System.Collections;

public class AddRegularPills : MonoBehaviour
{

    public GameObject pillObject;
    public GameObject powerPillObject;

    void Start ()
    {

        Map map = new Map (GlobalGameDetails.mapName);

        int pillCount = 0;
        int powerPillCount = 0;

        for (int gridX = 0; gridX < GlobalGameDetails.mapColumns; gridX++) {
            for (int gridY = 0; gridY < GlobalGameDetails.mapRows; gridY++) {
                if (map.PillAtGridReference (gridX, gridY)) {
                    float[] latLongRef = map.LatitudeLongitudeAtGridReference (gridX, gridY);
                    float latitude = latLongRef [0];
                    float longitude = latLongRef [1];

                    // TODO: DRY this out - we should get GlobalGameDetails to return
                    // a SphericalCoordinates instance for consistency
                    SphericalCoordinates sc = new SphericalCoordinates (
           				      0.5f, 
        				        degreesToRadians (longitude), 
        				        degreesToRadians (latitude),
        				        0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f)
                    );
                    Vector3 newPillPosition = sc.toCartesian;
                    GameObject pill;
                    if ( map.PowerPillAtGridReference (gridX, gridY) ) {
                        pill = Instantiate (powerPillObject) as GameObject;
                        powerPillCount++;
                    } else {
                        pill = Instantiate (pillObject) as GameObject;
                    }
                    pillCount++; // powerPills are still pills
                    pill.transform.parent = transform;
                    pill.transform.localPosition = newPillPosition;
                }
            }
        }
        Debug.Log ("Created " + pillCount + " pills, including "
            + powerPillCount + " power pills");
    }

    // TODO: DRY this out
    float degreesToRadians (float degrees)
    {
        return (degrees * Mathf.PI / 180f);
    }
    
}
