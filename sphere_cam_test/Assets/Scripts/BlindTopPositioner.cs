using UnityEngine;
using System.Collections;

public class BlindTopPositioner : MonoBehaviour
{

    private GlobalGameDetails ggd;
    public float multiplier;

    void FixedUpdate ()
    {
        GameObject[] states = GameObject.FindGameObjectsWithTag ("PersistedState");
        ggd = states[0].GetComponent<GlobalGameDetails>();
        float x = ggd.BlindTopX();
        float y = ggd.BlindTopY();
        float rot = ggd.BlindTopRot();
        Vector3 v = new Vector3 ( x , y * multiplier, 0 );
        Quaternion rotation = Quaternion.Euler(0, 0, rot * multiplier);
        transform.localPosition = v;
        transform.localRotation = rotation;
    }
	
}
