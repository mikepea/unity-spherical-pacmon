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
        Vector2 nextMoveSpeed = new Vector2 (
                (speed * Time.deltaTime * LatitudeSpeedAdjust (latitude)),
                (speed * Time.deltaTime)
        );

        float dist = angularDistanceToNextGridLine (latitude, longitude, playerDirection);

        if (intendedDirection.y != 0 && dist < nextMoveSpeed.x) {
            latitude = latitude + (playerDirection.x * dist);
            playerDirection = intendedDirection;
        } else if (intendedDirection.x != 0 && dist < nextMoveSpeed.y) {
            longitude = longitude + (playerDirection.y * dist);
            playerDirection = intendedDirection;
        }

        float newLongitude = (longitude + playerDirection.x * nextMoveSpeed.x) % 360;
        float newLatitude = latitude + playerDirection.y * nextMoveSpeed.y;

        if (newLongitude < 0) {
            newLongitude = 360 + newLongitude;
        }
        if (newLatitude < minAngleY) {
            newLatitude = minAngleY;
        } else if (newLatitude > maxAngleY) {
            newLatitude = maxAngleY;
        }

        Debug.Log ("MIKEDEBUG: "
            + " gridX: " + playerGridX
            + " gridXangle: " + playerGridX * gridSpacing
            + " gridY: " + playerGridY
            + " gridYangle: " + playerGridY * gridSpacing
            + " dist: " + angularDistanceToNextGridLine (latitude, longitude, playerDirection)
            + " lat: " + latitude
            + " long: " + longitude
            + " newLat: " + newLatitude
            + " newLong: " + newLongitude
        );


        return new Vector2 (newLongitude, newLatitude);

    }

    float angularDistanceToNextGridLine (float latitude, float longitude, Vector2 direction)
    {
        if (direction.y > 0) {
            // going up
            float nextGridLine = Mathf.Ceil (latitude / gridSpacing) * gridSpacing;
            return nextGridLine - latitude;
        } else if (direction.y < 0) {
            // going down
            float nextGridLine = Mathf.Floor (latitude / gridSpacing) * gridSpacing;
            return latitude - nextGridLine;
        } else if (direction.x > 0) {
            // going east
            float nextGridLine = Mathf.Ceil (longitude / gridSpacing) * gridSpacing;
            return nextGridLine - longitude;
        } else if (direction.x < 0) {
            // going west
            float nextGridLine = Mathf.Floor (longitude / gridSpacing) * gridSpacing;
            return longitude - nextGridLine;
        } else {
            // stopped, return a sensible default
            return 0;
        }
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
