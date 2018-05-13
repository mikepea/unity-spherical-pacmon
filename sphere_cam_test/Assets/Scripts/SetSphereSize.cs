using UnityEngine;
using System.Collections;

public class SetSphereSize : MonoBehaviour
{

    private GlobalGameDetails ggd;

    void FixedUpdate ()
    {
        GameObject[] states = GameObject.FindGameObjectsWithTag ("PersistedState");
        ggd = states[0].GetComponent<GlobalGameDetails>();
        float size = ggd.SphereSize();
        transform.localScale = new Vector3(size, size, size);
    }
	
}
