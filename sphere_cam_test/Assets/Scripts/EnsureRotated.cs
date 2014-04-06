using UnityEngine;
using System.Collections;

public class EnsureRotated : MonoBehaviour {

	void Start () {
		transform.LookAt (Vector3.zero);
		transform.Rotate(Vector3.right, 90);	
	}
	
}
