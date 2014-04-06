using UnityEngine;
using System.Collections;

public class PlayerSphericalMovement : MonoBehaviour {

  public float horizontalSpeed = 2.0F;
  public float verticalSpeed = 2.0F;
  public float rotateSpeed = 2.0F;

  public SphericalCoordinates sc;

  void Start () {
	sc = new SphericalCoordinates (transform.localPosition, 0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f) );
	transform.localPosition = sc.toCartesian;
  }

  void FixedUpdate () {
    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");
	transform.localPosition = sc.Rotate( h * rotateSpeed * Time.deltaTime, v * rotateSpeed * Time.deltaTime ).toCartesian;
	transform.LookAt (Vector3.zero);
	transform.Rotate(Vector3.right, 90);
  }

	void OnTriggerEnter(Collider other) {
		if ( other.gameObject.tag == "Baddy" ) {
			other.gameObject.SetActive(false);
		} else if ( other.gameObject.tag == "Pill" ) {
			other.gameObject.SetActive(false);
		} else if ( other.gameObject.tag == "Power Pill" ) {
			other.gameObject.SetActive(false);
		}
	}

}
