using UnityEngine;
using System.Collections;

public class AddRegularPills : MonoBehaviour {

	public GameObject pillObject;
	public float pillSpacing;       // degrees
	public float pillMinSeparation; // distance
	public float latitudeMin = -80;
	public float latitudeMax = 80;

	void Start () {
		int pillCount = 0;
		for ( float latitude = latitudeMin; latitude < latitudeMax; latitude += pillSpacing) {
			Vector3 lastPillPosition = Vector3.zero;
		    for (float longitude = -180; longitude < 180; longitude += pillSpacing) {
  				SphericalCoordinates sc = new SphericalCoordinates (
					0.5f, 
					degreesToRadians(longitude), 
					degreesToRadians(latitude),
					0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f)
				);
  				Vector3 newPillPosition = sc.toCartesian;
				float distanceFromLastPill = Vector3.Distance(lastPillPosition, newPillPosition);
				if ( longitude == 0 || distanceFromLastPill > pillMinSeparation ) {
     	            GameObject pill = Instantiate(pillObject) as GameObject;
				    pillCount++;
   		            pill.transform.parent = transform;
				    pill.transform.localPosition = newPillPosition;
  		 		    lastPillPosition = newPillPosition;
				}
			}
		}
        Debug.Log("Created " + pillCount + " pills");
	}

	float degreesToRadians ( float degrees ) {
		return (degrees * Mathf.PI / 180f);
	}
	
}
