using UnityEngine;
using System.Collections;

public class CreateMap : MonoBehaviour
{

    public Color gridColor = Color.grey;

    public int xOffset;
    public int yOffset;
    public bool enableGridLines;

    public Texture2D mapTiles;

    void Start ()
    {

        Map map = new Map (GlobalGameDetails.mapName);

        //Texture2D baseTexture = map.UVMappedTexture (360 * 5, 180 * 5, xOffset, yOffset, enableGridLines);
        Texture2D texture = TileBasedMap(map);

        Shader mapShader = Shader.Find("Diffuse");

        texture.Apply ();
        renderer.material.mainTexture = texture;
        renderer.material.shader = mapShader;
        float offset = 0.057F;
        renderer.material.mainTextureOffset = new Vector2(offset, 0);

    }

    public Texture2D TileBasedMap (Map map)
    {
        int tileSize = 40;
        int tileDegrees = 5;

        int numColumns = map.Columns();
        int numRows = map.Rows();

        int bottomPadding = 140;

        int numTilesW = mapTiles.width / tileSize;
        int numTilesH = mapTiles.height / tileSize;

        Texture2D texture = new Texture2D (tileSize * 360 / tileDegrees, tileSize * 180 / tileDegrees);

        for ( int x = 0; x < numColumns; x++ ) {
            for ( int y = 0; y < numRows; y++ ) {
              int tileNum = MapTileReferenceNumber(map, x, y);
              int tilesX = ( (tileNum - 1) % numTilesW ) * tileSize;
              int tilesY = ( (numTilesH - 1) - ( (tileNum - 1) / 4 ) ) * tileSize;
              //Debug.Log("Tile: " + tileNum + ", X: " + tilesX + ", Y: " + tilesY);
              Color[] tile = mapTiles.GetPixels(tilesX, tilesY, tileSize, tileSize);
              int textureX = x * tileSize;
              int textureY = (y * tileSize) + bottomPadding;
              Debug.Log("Texture: X: " + textureX + ", Y: " + textureY);
              texture.SetPixels(textureX, textureY, tileSize, tileSize, tile);
            }
        }
        return texture;
    }

    public int MapTileReferenceNumber (Map map, int x, int y)
    {
        int tileNum = 0;
        Debug.Log("MapTileReference: X: " + x + ", Y: " + y);

        if ( ! map.WallAtGridReference(x, y) ) {
          // our 'blank map tile'
          return 32;
        }

        if ( map.WallAtGridReference(x - 1, y) ) {
          tileNum += 1;
        }
        if ( map.WallAtGridReference(x, y + 1) ) {
          tileNum += 2;
        }
        if ( map.WallAtGridReference(x + 1, y) ) {
          tileNum += 4;
        }
        if ( map.WallAtGridReference(x, y - 1) ) {
          tileNum += 8;
        }

        if ( tileNum == 0 ) {
          tileNum = 16;
        }

        if ( tileNum == 3 ) {
          if ( ! map.WallAtGridReference(x-1, y+1) ) { tileNum = 17; }
        } else if ( tileNum == 6 ) {
          if ( ! map.WallAtGridReference(x+1, y+1) ) { tileNum = 18; }
        } else if ( tileNum == 9 ) {
          if ( ! map.WallAtGridReference(x-1, y-1) ) { tileNum = 19; }
        } else if ( tileNum == 12 ) {
          if ( ! map.WallAtGridReference(x+1, y-1) ) { tileNum = 20; }
        } else if ( tileNum == 7 ) {
          if ( ! map.WallAtGridReference(x-1, y+1) && ! map.WallAtGridReference(x+1, y+1) ) { tileNum = 21; }
          else if ( ! map.WallAtGridReference(x-1, y+1) && map.WallAtGridReference(x+1, y+1) ) { tileNum = 37; }
          else if ( map.WallAtGridReference(x-1, y+1) && ! map.WallAtGridReference(x+1, y+1) ) { tileNum = 41; }
        } else if ( tileNum == 11 ) {
          if ( ! map.WallAtGridReference(x-1, y-1) && ! map.WallAtGridReference(x-1, y+1) ) { tileNum = 24; }
          else if ( ! map.WallAtGridReference(x-1, y-1) && map.WallAtGridReference(x-1, y+1) ) { tileNum = 40; }
          else if ( map.WallAtGridReference(x-1, y-1) && ! map.WallAtGridReference(x-1, y+1) ) { tileNum = 44; }
        } else if ( tileNum == 13 ) {
          if ( ! map.WallAtGridReference(x-1, y-1) && ! map.WallAtGridReference(x+1, y-1) ) { tileNum = 23; }
          else if ( ! map.WallAtGridReference(x-1, y-1) && map.WallAtGridReference(x+1, y-1) ) { tileNum = 43; }
          else if ( map.WallAtGridReference(x-1, y-1) && ! map.WallAtGridReference(x+1, y-1) ) { tileNum = 39; }
        } else if ( tileNum == 14 ) {
          if ( ! map.WallAtGridReference(x+1, y-1) && ! map.WallAtGridReference(x+1, y+1) ) { tileNum = 22; }
          else if ( ! map.WallAtGridReference(x+1, y-1) && map.WallAtGridReference(x+1, y+1) ) { tileNum = 42; }
          else if ( map.WallAtGridReference(x+1, y-1) && ! map.WallAtGridReference(x+1, y+1) ) { tileNum = 38; }
        } else if ( tileNum == 15 ) {
          if ( ! map.WallAtGridReference(x-1, y+1) &&
               ! map.WallAtGridReference(x+1, y+1) &&
               ! map.WallAtGridReference(x+1, y-1) &&
               ! map.WallAtGridReference(x-1, y-1)) { tileNum = 31; }
          else if ( map.WallAtGridReference(x-1, y+1) &&
               ! map.WallAtGridReference(x+1, y+1) &&
               ! map.WallAtGridReference(x+1, y-1) &&
               ! map.WallAtGridReference(x-1, y-1)) { tileNum = 25; }
          else if ( ! map.WallAtGridReference(x-1, y+1) &&
               map.WallAtGridReference(x+1, y+1) &&
               ! map.WallAtGridReference(x+1, y-1) &&
               ! map.WallAtGridReference(x-1, y-1)) { tileNum = 26; }
          else if ( ! map.WallAtGridReference(x-1, y+1) &&
               ! map.WallAtGridReference(x+1, y+1) &&
               map.WallAtGridReference(x+1, y-1) &&
               ! map.WallAtGridReference(x-1, y-1)) { tileNum = 27; }
          else if ( ! map.WallAtGridReference(x-1, y+1) &&
               ! map.WallAtGridReference(x+1, y+1) &&
               ! map.WallAtGridReference(x+1, y-1) &&
               map.WallAtGridReference(x-1, y-1)) { tileNum = 28; }
          else if ( ! map.WallAtGridReference(x-1, y+1) &&
               map.WallAtGridReference(x+1, y+1) &&
               ! map.WallAtGridReference(x+1, y-1) &&
               map.WallAtGridReference(x-1, y-1)) { tileNum = 29; }
          else if ( map.WallAtGridReference(x-1, y+1) &&
               ! map.WallAtGridReference(x+1, y+1) &&
               map.WallAtGridReference(x+1, y-1) &&
               ! map.WallAtGridReference(x-1, y-1)) { tileNum = 30; }
          else if ( ! map.WallAtGridReference(x-1, y+1) &&
               ! map.WallAtGridReference(x+1, y+1) &&
               map.WallAtGridReference(x+1, y-1) &&
               map.WallAtGridReference(x-1, y-1)) { tileNum = 45; }
          else if ( map.WallAtGridReference(x-1, y+1) &&
               ! map.WallAtGridReference(x+1, y+1) &&
               ! map.WallAtGridReference(x+1, y-1) &&
               map.WallAtGridReference(x-1, y-1)) { tileNum = 46; }
          else if ( map.WallAtGridReference(x-1, y+1) &&
               map.WallAtGridReference(x+1, y+1) &&
               ! map.WallAtGridReference(x+1, y-1) &&
               ! map.WallAtGridReference(x-1, y-1)) { tileNum = 47; }
          else if ( ! map.WallAtGridReference(x-1, y+1) &&
               map.WallAtGridReference(x+1, y+1) &&
               map.WallAtGridReference(x+1, y-1) &&
               ! map.WallAtGridReference(x-1, y-1)) { tileNum = 48; }
          else if ( ! map.WallAtGridReference(x-1, y+1) ) { tileNum = 25; }
          else if ( ! map.WallAtGridReference(x+1, y+1) ) { tileNum = 26; }
          else if ( ! map.WallAtGridReference(x+1, y-1) ) { tileNum = 27; }
          else if ( ! map.WallAtGridReference(x-1, y-1) ) { tileNum = 28; }
        }
        return tileNum;

    }

}
