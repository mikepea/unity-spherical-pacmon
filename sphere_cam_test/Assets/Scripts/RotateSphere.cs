using UnityEngine;
using System.Collections;

public class RotateSphere : MonoBehaviour {

  public  Vector3 rotateSpeed;
  public  float rotateSpeedIncrement = 0.1f;
  public  float maxSpeed = 2.0f;
  private Vector3 rotateDelta;
  private string rotateMode = "waitForAction";
  private float waitTimerStart = -1.0f;
  private float waitTime = 3.8f;

  void Start () {
    rotateDelta = rotateSpeed;
  }

  void IncreaseSpeed () {
    rotateSpeed.y += rotateSpeedIncrement;
    if ( rotateSpeed.y > maxSpeed ) {
      rotateSpeed.y = maxSpeed;
    }
  }

  void DecreaseSpeed () {
    rotateSpeed.y -= rotateSpeedIncrement;
    if ( rotateSpeed.y < 0 ) {
      rotateSpeed.y = 0;
    }
  }

  void ResetToOrigin () {
    this.transform.localRotation = Quaternion.identity;
    rotateMode = "waitForAction";
  }

	void FixedUpdate () {
    if ( rotateMode == "quickToOrigin" ) {
      //float y = this.transform.localRotation.y;
      //if ( y + rotateSpeedIncrement >= 360.0f ) {
      rotateMode = "normal";
    } else if ( rotateMode == "waitForAction" ) {
      if ( waitTimerStart < 0 ) {
        waitTimerStart = Time.time;
      } else if ( waitTimerStart + waitTime > Time.time ) {
        rotateDelta = Vector3.zero;
      } else {
        waitTimerStart = -1F;
        rotateMode = "normal";
      }
    } else {
      // normal - reset rotate speed
      rotateDelta = rotateSpeed;
    }
    transform.Rotate(rotateDelta);
  }

}
