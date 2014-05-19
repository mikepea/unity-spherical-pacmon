using UnityEngine;
using System.Collections;

public class EnterNextScene : MonoBehaviour
{

    public string nextScene;
    public string nextSceneKey;

    void Update ()
    {
        if (Input.GetKey (nextSceneKey)) {
            Application.LoadLevel (nextScene);
        }
    }

}
