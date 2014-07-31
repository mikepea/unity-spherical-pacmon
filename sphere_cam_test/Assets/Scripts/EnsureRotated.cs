using UnityEngine;
using System.Collections;

public class EnsureRotated : MonoBehaviour
{

    public bool rotateRight = false;

    void Start ()
    {
        transform.LookAt (Vector3.zero);
        if (rotateRight) {
            transform.Rotate (Vector3.right, 90);
        }
    }
	
}
