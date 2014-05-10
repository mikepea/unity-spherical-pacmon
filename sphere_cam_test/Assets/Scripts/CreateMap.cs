using UnityEngine;
using System.Collections;

public class CreateMap : MonoBehaviour
{

    public Color gridColor = Color.grey;

    public int offset;

    // Use this for initialization
    void Start ()
    {

        Map map = new Map (GlobalGameDetails.mapName);

        Texture2D texture = map.UVMappedTexture (360 * 5, 180 * 5, offset);

        texture.Apply ();
        renderer.material.mainTexture = texture;

    }

}
