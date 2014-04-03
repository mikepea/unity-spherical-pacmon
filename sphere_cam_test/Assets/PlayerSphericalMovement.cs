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
    sc = new SphericalCoordinates (transform.position, 3f, 10f, 0f);
    //transform.position = sc.toCartesian + pivot.position;
	transform.position = sc.toCartesian;
  }

  // Update is called once per frame
  void Update () {
    float h = Input.GetAxis("Horizontal");
    float v = Input.GetAxis("Vertical");
    //transform.Translate(h, v, 0);
	transform.position = sc.Rotate( h * rotateSpeed * Time.deltaTime, v * rotateSpeed * Time.deltaTime ).toCartesian;
	transform.LookAt (Vector3.zero);
	transform.Rotate(Vector3.right, 90);
  }

}
