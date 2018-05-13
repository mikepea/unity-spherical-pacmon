using UnityEngine;
using System.Collections;

public class BlindBottomPositioner : MonoBehaviour
{

    private GlobalGameDetails ggd;
    public float multiplier;

    void FixedUpdate ()
    {
        GameObject[] states = GameObject.FindGameObjectsWithTag ("PersistedState");
        ggd = states[0].GetComponent<GlobalGameDetails>();
        float x = ggd.BlindBottomX();
        float y = ggd.BlindBottomY();
        float rot = ggd.BlindBottomRot();
        Vector3 v = new Vector3 ( x , y * multiplier, 0 );
        Quaternion rotation = Quaternion.Euler(0, 0, rot * multiplier);
        transform.localPosition = v;
        transform.localRotation = rotation;
    }
	
}
