
using UnityEngine;
using System.Collections;

public class AlignmentTexture : MonoBehaviour
{


    public Color gridColor = Color.grey;
    public int xOffset;
    public int yOffset;
    public bool enableGridLines;

    public int xPixels = 360 * 5;
    public int yPixels = 180 * 5;
    public int gridLine = 5 * 5;

    // Use this for initialization
    void Start ()
    {
        Texture2D texture = new Texture2D (xPixels, yPixels);

        for (int x = 0; x < xPixels; x++) {
            for (int y = 0; y < yPixels; y++) {
                if (y % gridLine == 0 || x % gridLine == 0) {
                    texture.SetPixel (x, y, gridColor);
                } else {
                    texture.SetPixel (x, y, Color.black);
                }
            }
        }

        texture.Apply ();
        GetComponent<Renderer>().material.mainTexture = texture;	

    }

}