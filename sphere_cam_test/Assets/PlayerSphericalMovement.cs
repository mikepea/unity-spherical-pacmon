using UnityEngine;
using System.Collections;

public class PlayerSphericalMovement : MonoBehaviour {

  public float horizontalSpeed = 2.0F;
  public float verticalSpeed = 2.0F;
  public float rotateSpeed = 2.0F;
  public Vector3 pivot;

  public SphericalCoordinates sc;

  // Use this for initialization
  void Start () {
		sc = new SphericalCoordinates (transform.localPosition, 0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f) );
	transform.localPosition = sc.toCartesian;
  }

  // Update is called once per frame
  void Update () {
    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");
	transform.localPosition = sc.Rotate( h * rotateSpeed * Time.deltaTime, v * rotateSpeed * Time.deltaTime ).toCartesian;
	transform.LookAt (Vector3.zero);
	transform.Rotate(Vector3.right, 90);
  }

}
