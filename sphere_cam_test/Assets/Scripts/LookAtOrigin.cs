using UnityEngine;
using System.Collections;

public class LookAtOrigin : MonoBehaviour
{

    public float rotX, rotY, rotZ;

    void Start ()
    {
        transform.LookAt (Vector3.zero);
        transform.Rotate (rotX, rotY, rotZ);
    }

    void FixedUpdate ()
    {
        transform.LookAt (Vector3.zero);
        transform.Rotate (rotX, rotY, rotZ);
    }
	
}
