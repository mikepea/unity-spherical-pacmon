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

    public Map map = new Map (GlobalGameDetails.mapName);

    void Start ()
    {
        sc = new SphericalCoordinates (transform.localPosition, 0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f));
        transform.localPosition = sc.toCartesian;
        numPills = GameObject.FindGameObjectsWithTag ("Pill").Length;
    }

    Vector2 ProcessInputsIntoDirection (Vector2 direction)
    {
        float h = Input.GetAxisRaw ("Horizontal");
        float v = Input.GetAxisRaw ("Vertical");

        if (h == 1) {
            direction = Vector2.right;
        } else if (h == -1) {
            direction = (- Vector2.right);
        } else if (v == 1) {
            direction = Vector2.up;
        } else if (v == -1) {
            direction = (- Vector2.up);
        }
        return direction;
    }

    void FixedUpdate ()
    {

        playerIntendedDirection = ProcessInputsIntoDirection (playerIntendedDirection);

        Vector2 playerNextLocation = GetPlayerNextLocation (currentAngleX, currentAngleY, playerIntendedDirection);
        currentAngleX = playerNextLocation.x;
        currentAngleY = playerNextLocation.y;

        updatePlayerObjectLocationAndRotation (currentAngleX, currentAngleY);

    }

    void updatePlayerObjectLocationAndRotation (float longitude, float latitude)
    {
        transform.localPosition = sc.SetRotation (
            degreesToRadians (longitude),
            degreesToRadians (latitude)
        ).toCartesian;
        transform.LookAt (Vector3.zero);
        transform.Rotate (Vector3.right, 90);
    }

    Vector2 GetPlayerNextLocation (float longitude, float latitude, Vector2 intendedDirection)
    {
        int[] gridRef = map.GridReferenceAtLatitudeLongitude (latitude, longitude);
        int playerGridX = gridRef [0];
        int playerGridY = gridRef [1];
        Vector2 stop = Vector2.zero;
        //Debug.Log ("playerLat: " + latitude + ", playerLong: " + longitude);
        //Debug.Log ("Player GridX: " + playerGridX + ", GridY: " + playerGridY);

        if (longitude % gridSpacing == 0 && intendedDirection.y != 0) {
            if (map.WallAtGridReference (playerGridX, playerGridY + (int)intendedDirection.y)) {
                Debug.Log ("Wall at gridX: " + playerGridX + ", gridY: " + (playerGridY + (int)intendedDirection.y));
                if (playerDirection == playerIntendedDirection) {
                    playerDirection = stop;
                }
            } else {
                playerDirection = intendedDirection;
            }
        } else if (latitude % gridSpacing == 0 && intendedDirection.x != 0) {
            if (map.WallAtGridReference (playerGridX + (int)intendedDirection.x, playerGridY)) {
                Debug.Log ("Wall at gridX: " + (playerGridX + (int)playerIntendedDirection.x) + ", gridY: " + playerGridY);
                if (playerDirection == playerIntendedDirection) {
                    playerDirection = stop;
                }
            } else {
                playerDirection = intendedDirection;
            }
        }

        float newLongitude = (longitude + playerDirection.x * speed * LatitudeSpeedAdjust (latitude)) % 360;
        float newLatitude = latitude + playerDirection.y * speed;
        if (newLongitude < 0) {
            newLongitude = 360 + newLongitude;
        }
        if (newLatitude < minAngleY) {
            newLatitude = minAngleY;
        } else if (newLatitude > maxAngleY) {
            newLatitude = maxAngleY;
        } 

        return new Vector2 (newLongitude, newLatitude);

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
