using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameObjectPositioner : MonoBehaviour
{

    public SphericalCoordinates sc;

    public string startMarkerTag;

    private float currentAngleX = 0F;
    private float currentAngleY = 0F;
    private Vector2 objectGridRef = new Vector2 (0, 0);

    private Map map;

    private GlobalGameDetails ggd;

    GlobalGameDetails GlobalState() {
        if (!ggd) {
          GameObject[] states = GameObject.FindGameObjectsWithTag ("PersistedState");
          ggd = states[0].GetComponent<GlobalGameDetails>();
        }
        return ggd;
    }

    void Start ()
    {

        string mapName = GlobalState().MapName();
        map = new Map (mapName);

        Debug.Log("Game Mode: " + GlobalState().GameMode());

        sc = new SphericalCoordinates (transform.localPosition, 0f, 10f, 0f, (Mathf.PI * 2f), -(Mathf.PI / 3f), (Mathf.PI / 3f));
        transform.localPosition = sc.toCartesian;
        PutObjectAtStartPosition ();

    }

    void PutObjectAtStartPosition ()
    {
        objectGridRef = map.FindEntityGridRef (startMarkerTag);
        if ( objectGridRef == new Vector2 (-1, -1) ) {
            this.gameObject.SetActive (false);
            return;
        }
        float[] mapRef = map.LatitudeLongitudeAtGridReference (objectGridRef);
        currentAngleX = mapRef [1];
        currentAngleY = mapRef [0];
        //DisableRendering();
        UpdateObjectLocationAndRotation ();
    }

    void EnableRendering() {
        this.GetComponent<Renderer>().enabled = true;
    }

    void DisableRendering() {
        this.GetComponent<Renderer>().enabled = false;
    }

    void UpdateObjectLocationAndRotation ()
    {
        transform.localPosition = sc.SetRotation (
            map.degreesToRadians (currentAngleX),
            map.degreesToRadians (currentAngleY)
        ).toCartesian;
        transform.LookAt (Vector3.zero);
    }

}
