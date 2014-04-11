using UnityEngine;
using System.Collections;

public class CreateMap : MonoBehaviour
{

    public Color gridColor = Color.grey;

    public float offset;

    // Use this for initialization
    void Start ()
    {

        Map map = new Map ("map1");

        Texture2D texture = map.UVMappedTexture (360 * 5, 180 * 5, 0);

        texture.Apply ();
        renderer.material.mainTexture = texture;

    }

    void Update ()
    {
        renderer.material.mainTextureOffset = new Vector2 (offset, 0);
    }
	
}
