using UnityEngine;
using System.Collections;

public class CreateMap : MonoBehaviour {

	public Color gridColor = Color.grey;

	public float offset;

	// Use this for initialization
	void Start () {
	    Texture2D texture = new Texture2D(360 * 5, 180 * 5);
		for ( int x = 0; x < 360 * 5; x++ ) {
			for ( int y = 0; y < 180 * 5; y++ ) {
				//if ( x % (10 * 5) == 0 ) {
				if ( x == 0 ) {
					texture.SetPixel(x, y, Color.blue);
				//} else if ( y % (10 * 5) == 0 ) {
				} else if ( y == ( 90 * 5 ) ) {
					texture.SetPixel(x, y, Color.red);
				}
				
			}
		}
		texture.Apply();
	    renderer.material.mainTexture = texture;
	}

	void Update() {
		renderer.material.mainTextureOffset = new Vector2(offset, 0);
	}
	
}
