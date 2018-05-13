using UnityEngine;
using System.Collections;

public class PositionCameraMountInSpace : MonoBehaviour
{

    private GlobalGameDetails ggd;
    public Vector3 generalPosition;

    void FixedUpdate ()
    {
        GameObject[] states = GameObject.FindGameObjectsWithTag ("PersistedState");
        ggd = states[0].GetComponent<GlobalGameDetails>();
        float distFromOrigin = ggd.CameraDistanceFromOrigin();
        float pos = Mathf.Sqrt( Mathf.Pow(distFromOrigin, 2) / 2);
        float x = generalPosition.x * pos;
        float z = generalPosition.z * pos;
        float y = ggd.CameraYOffset();
        Vector3 v = new Vector3 ( x, y, z );
        transform.localPosition = v;
    }
	
}
