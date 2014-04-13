using UnityEngine;
using System.Collections;

public class RotateSphere : MonoBehaviour {

	public float rotateSpeed;

	// Update is called once per frame
	void FixedUpdate () {
		transform.RotateAround(Vector3.zero, Vector3.up, rotateSpeed * Time.deltaTime);
	}
}
