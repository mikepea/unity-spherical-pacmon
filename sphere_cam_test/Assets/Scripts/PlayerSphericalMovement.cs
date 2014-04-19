using UnityEngine;
using System.Collections;

public class PlayerSphericalMovement : MonoBehaviour
{

    public float gridSpacing;

    public SphericalCoordinates sc;
    public int numPills;
    private Vector2 playerDirection = new Vector2 (0, 0);
    private Vector2 playerIntendedDirection = new Vector2 (0, 0);

    public float speed = 0.5F;
    private float currentAngleX = 0F;
    private float currentAngleY = 0F;
    private float maxAngleY = GlobalGameDetails.maxAngleY;
    private float minAngleY = GlobalGameDetails.minAngleY;

    public Map map = new Map ("map1");

    void Start ()
    {
        sc = new SphericalCoordinates (transform.localPosition, 0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f));
        transform.localPosition = sc.toCartesian;
        numPills = GameObject.FindGameObjectsWithTag ("Pill").Length;
    }

    void FixedUpdate ()
    {

        float h = Input.GetAxisRaw ("Horizontal");
        float v = Input.GetAxisRaw ("Vertical");
        Vector2 stop = Vector2.zero;

        if (h == 1) {
            playerIntendedDirection = Vector2.right;
        } else if (h == -1) {
            playerIntendedDirection = - Vector2.right;
        } else if (v == 1) {
            playerIntendedDirection = Vector2.up;
        } else if (v == -1) {
            playerIntendedDirection = - Vector2.up;
        } 

        int[] gridRef = map.GridReferenceAtLatitudeLongitude (currentAngleY, currentAngleX);
        Debug.Log ("playerLat: " + currentAngleX + ", playerLong: " + currentAngleY);
        Debug.Log ("Player GridX: " + gridRef [0] + ", GridY: " + gridRef [1]);

        if (currentAngleX % gridSpacing == 0 && playerIntendedDirection.y != 0) {
            if (! map.WallAtGridReference (gridRef [0], gridRef [1] + (int)playerIntendedDirection.y)) {
                playerDirection = playerIntendedDirection;
            } else {
                Debug.Log ("Wall at gridX: " + gridRef [0] + ", gridY: " + (gridRef [1] + (int)playerIntendedDirection.y));
                playerDirection = stop;
            }
        } else if (currentAngleY % gridSpacing == 0 && playerIntendedDirection.x != 0) {
            Debug.Log ("Player GridX: " + gridRef [0] + ", GridY: " + gridRef [1]);
            if (! map.WallAtGridReference (gridRef [0] + (int)playerIntendedDirection.x, gridRef [1])) {
                playerDirection = playerIntendedDirection;
            } else {
                Debug.Log ("Wall at gridX: " + gridRef [0] + (int)playerIntendedDirection.x + ", gridY: " + gridRef [1]);
                playerDirection = stop;
            }
        }

        currentAngleX = (currentAngleX + playerDirection.x * speed * LatitudeSpeedAdjust (currentAngleY)) % 360;
        currentAngleY = currentAngleY + playerDirection.y * speed;
        if (currentAngleX < 0) {
            currentAngleX = 360 + currentAngleX;
        }
        if (currentAngleY < minAngleY) {
            currentAngleY = minAngleY;
        } else if (currentAngleY > maxAngleY) {
            currentAngleY = maxAngleY;
        } 

        //Debug.Log ("X: " + currentAngleX + " -- Y: " + currentAngleY);

        transform.localPosition = sc.SetRotation (
            degreesToRadians (currentAngleX),
	  		degreesToRadians (currentAngleY)
        ).toCartesian;

        transform.LookAt (Vector3.zero);
        transform.Rotate (Vector3.right, 90);
    }

    float LatitudeSpeedAdjust (float angle)
    {
        float a = Mathf.Abs (angle);
        if (a >= 80) {
            return 6.0F;
        } else if (a >= 70) {
            return 3.0F;
        } else if (a >= 60) {
            return 2.0F;
        } else if (a >= 50) {
            return 1.5F;
        } else {
            return 1.0F;
        }
    }

    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.tag == "Baddy") {
            other.gameObject.SetActive (false);
        } else if (other.gameObject.tag == "Pill") {
            other.gameObject.SetActive (false);
            numPills = GameObject.FindGameObjectsWithTag ("Pill").Length;
            Debug.Log (numPills + " pills remaining");
        } else if (other.gameObject.tag == "Power Pill") {
            other.gameObject.SetActive (false);
        }
    }

    float radiansToDegrees (float rads)
    {
        return rads * 180 / Mathf.PI;
    }

    float degreesToRadians (float degrees)
    {
        return degrees * Mathf.PI / 180;
    }

}
