using UnityEngine;
using System.Collections;

public class BlindOffsetter : MonoBehaviour
{

    public float multiplier;
    private GlobalGameDetails ggd;

    void FixedUpdate ()
    {
        GameObject[] states = GameObject.FindGameObjectsWithTag ("PersistedState");
        ggd = states[0].GetComponent<GlobalGameDetails>();
        float offset = ggd.BlindOffset();
        Vector3 v = new Vector3 ( 0, offset * multiplier, 0.5F );
        transform.localPosition = v;
    }
	
}
