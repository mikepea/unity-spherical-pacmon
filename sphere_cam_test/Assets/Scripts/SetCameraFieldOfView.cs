using UnityEngine;
using System.Collections;

public class SetCameraFieldOfView : MonoBehaviour
{

    private GlobalGameDetails ggd;

    void FixedUpdate ()
    {
        GameObject[] states = GameObject.FindGameObjectsWithTag ("PersistedState");
        ggd = states[0].GetComponent<GlobalGameDetails>();
        float fov = ggd.CameraFieldOfView();
        Camera[] cams = Camera.allCameras;
        foreach (Camera cam in cams) {
          cam.fieldOfView = fov;
        }
    }
	
}
