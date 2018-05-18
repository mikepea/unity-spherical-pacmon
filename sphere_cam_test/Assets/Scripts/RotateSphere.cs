using UnityEngine;
using System.Collections;

public class RotateSphere : MonoBehaviour {

  public  Vector3 rotateSpeed;
  public  float rotateSpeedIncrement = 0.1f;
  private Vector3 rotateDelta;

  void Start () {
    rotateDelta = rotateSpeed;
  }

  void IncreaseSpeed () {
    rotateDelta.y += rotateSpeedIncrement;
  }

  void DecreaseSpeed () {
    rotateDelta.y -= rotateSpeedIncrement;
  }

	void FixedUpdate () {
    transform.Rotate(rotateDelta);
  }

}
