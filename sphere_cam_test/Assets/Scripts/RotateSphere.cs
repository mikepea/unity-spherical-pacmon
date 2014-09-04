using UnityEngine;
using System.Collections;

public class RotateSphere : MonoBehaviour {

  public  Vector3 rotateSpeed;
  private Vector3 rotateDelta;

  void Start () {
    rotateDelta = rotateSpeed;
  }

	void FixedUpdate () {
    transform.Rotate(rotateDelta);
  }

}
