using UnityEngine;
using System.Collections;

public class PlayerSphericalMovement : MonoBehaviour {

  	public float horizontalSpeed = 2.0F;
  	public float verticalSpeed = 2.0F;
  	public float rotateSpeed = 2.0F;
  	public float playerSpeed = 0.5F;
  	public float gridSpacing = 10.0F;
  	public int framesPerGridMove = 10;

  	public SphericalCoordinates sc;
  	public int numPills;
  	private Vector2 playerDirection = new Vector2 (0, 0);
  	private Vector2 playerIntendedDirection = new Vector2 (0, 0);
  	private Vector2 playerCurrentGridSpace = new Vector2 (0, 0);
  	private Vector2 playerNextGridSpace = new Vector2 (0, 0);

	public float speed = 0.5F;
	private float currentAngleX = 0F;
	private float currentAngleY = 0F;
	public float maxAngleY = 80F;
	public float minAngleY = -80F;

  	void Start () {
		sc = new SphericalCoordinates (transform.localPosition, 0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f) );
		transform.localPosition = sc.toCartesian;
		numPills = GameObject.FindGameObjectsWithTag("Pill").Length;
	}

  	void FixedUpdate () {

		//Debug.Log (Time.deltaTime + " - deltaTime");

  	  	float h = Input.GetAxisRaw("Horizontal");
  	  	float v = Input.GetAxisRaw("Vertical");
		if ( h == 1 ) {
	  		playerIntendedDirection = Vector2.right;
		} else if ( h == -1 ) {
			playerIntendedDirection = - Vector2.right;
		} else if ( v == 1 ) {
	  		playerIntendedDirection = Vector2.up;
		} else if ( v == -1 ) {
	  		playerIntendedDirection = - Vector2.up;
		} 

		if ( currentAngleX % gridSpacing == 0 && playerIntendedDirection.y != 0 ) {
		  playerDirection = playerIntendedDirection;
		} else if ( currentAngleY % gridSpacing == 0 && playerIntendedDirection.x != 0 ) {
		  playerDirection = playerIntendedDirection;
		}
		//Debug.Log (currentAngleX + " - currentAngleX");
		Debug.Log (currentAngleY + " - currentAngleY");

		/* TODO: Need to adjust speed based on latitude
		 * equator, moves 1x angular speed.
		 * at 80deg, moves 6x angular speed.
		 * TODO: need to ensure that our angle % grid calculation still works though.
		 */
		currentAngleX = ( currentAngleX + playerDirection.x * speed ) % 360;
		currentAngleY = currentAngleY + playerDirection.y * speed;
		if ( currentAngleX < 0 ) {
			currentAngleX = 360 + currentAngleX;
		}
		if ( currentAngleY < minAngleY ) {
			currentAngleY = minAngleY;
		} else if ( currentAngleY > maxAngleY ) {
			currentAngleY = minAngleY;
		} 

		transform.localPosition = sc.SetRotation( 
            degreesToRadians(currentAngleX),
	  		degreesToRadians(currentAngleY)
    	).toCartesian;

		transform.LookAt (Vector3.zero);
		transform.Rotate(Vector3.right, 90);
  	}

	void OnTriggerEnter(Collider other) {
		if ( other.gameObject.tag == "Baddy" ) {
			other.gameObject.SetActive(false);
		} else if ( other.gameObject.tag == "Pill" ) {
			other.gameObject.SetActive(false);
			numPills = GameObject.FindGameObjectsWithTag("Pill").Length;
			Debug.Log (numPills + " pills remaining");
		} else if ( other.gameObject.tag == "Power Pill" ) {
			other.gameObject.SetActive(false);
		}
	}

  	void LastUpdate () {
		if ( numPills == 0 ) {
			Debug.Log("ALL PILLS MUNCHED! NOM NOM");
		}
  	}

	float radiansToDegrees (float rads) {
		return rads * 180 / Mathf.PI;
	}

	float degreesToRadians (float degrees) {
		return degrees * Mathf.PI / 180;
	}

}
