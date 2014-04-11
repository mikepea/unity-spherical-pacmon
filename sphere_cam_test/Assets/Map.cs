using UnityEngine;
using System.Text;
using System.IO;

public class Map
{
    public static int numRows = 33;
    public static int numColumns = 72;
    public static int gridSpacing = 5;

    private bool mapLoaded = false;
    private int[,] mapData = new int[numRows, numColumns];

    public Map (string name)
    {
        Load ("Assets/Maps/" + name + ".csv");
    }

    private bool Load (string fileName)
    {

        if (mapLoaded == true) {
            return true;
        }

        try {

            int rowCount = 0;
            int columnCount = 0;
            string line;

            StreamReader theReader = new StreamReader (fileName, Encoding.Default);

            using (theReader) {
                do {
                    line = theReader.ReadLine ();
                    if (line != null) {
                        if (rowCount == 0) {
                            columnCount = line.Split (',').Length;
                        }
                        string[] entries = line.Split (',');
                        ProcessMapRow (rowCount, entries);
                        rowCount++;
                    }
                } while (line != null);

                theReader.Close ();
                Debug.Log ("Map Loaded");
                mapLoaded = true;
                return true;
            }

        } catch (IOException e) {
            Debug.Log (e.Message);
            return false;
        }
    }

    private void ProcessMapRow (int row, string[] entries)
    {

        for (int i = 0; i < numColumns; i++) {
            if (entries [i] == "w") {
                mapData [row, i] = 1;
            } else if (entries [i] == "p") {
                mapData [row, i] = 2;
            } else {
                mapData [row, i] = 0;
            }
        }
    }

    public int[] GridReferenceAtLatitudeLongitude (float latitude, float longitude)
    {
        // longitude goes from -180 to 180
        // latitude goes from -80 to 80
        // our grid[0,0] is -180 long, +80 lat
        int intLatitude = Mathf.FloorToInt (Mathf.Round (latitude));
        int intLongitude = Mathf.FloorToInt (Mathf.Round (longitude));
        //Debug.Log ("intLatitude: " + intLatitude + ", intLongitude: " + intLongitude);
        int gridX = ((intLongitude + 180) % 360) / gridSpacing;
        int gridY = (160 - ((intLatitude + 80) % 180)) / gridSpacing;
        int[] gridRef = {gridX, gridY};
        return gridRef;
    }

    public bool PillAtMapReference (float latitude, float longitude)
    {
        int[] gridRef = GridReferenceAtLatitudeLongitude (latitude, longitude);

        if (mapData [gridRef [1], gridRef [0]] == 2) {
            return true;
        } else {
            return false;
        }
    }

    public bool WallAtMapReference (float latitude, float longitude)
    {
        int[] gridRef = GridReferenceAtLatitudeLongitude (latitude, longitude);
        return WallAtGridReference (gridRef [1], gridRef [0]);
    }

    public bool WallAtGridReference (int x, int y)
    {
        if (mapData [y, x] == 1) {
            return true;
        } else {
            return false;
        }
    }

    public Texture2D UVMappedTexture (int xPixels, int yPixels, int xOffsetPixels)
    {
        Texture2D texture = new Texture2D (xPixels, yPixels);
        int xScaling = xPixels / numColumns;
        int yScaling = yPixels / numRows + 3;
        int fiveDegrees = 5 * xPixels / 360;

        Debug.Log ("xScaling: " + xScaling + ", yScaling: " + yScaling);

        for (int x = 0; x < xPixels; x++) {
            int gridX = x / xScaling; 

            for (int y = 0; y < yPixels; y++) {
                int gridY = 33 - ((y / yScaling) - 2); 
                //Debug.Log ("gridX: " + gridX + ", gridY: " + gridY);
                if (y < 20 * 5 || y > 160 * 5) {
                    texture.SetPixel (x, y, Color.red);
                } else if (WallAtGridReference (gridX, gridY)) {
                    texture.SetPixel (x, y, Color.blue);
                } else {
                    texture.SetPixel (x, y, Color.black);
                }
                if (y % fiveDegrees == 0 || x % fiveDegrees == 0) {
                    texture.SetPixel (x, y, Color.white);
                }
            }
        }
        return texture;

    }

}

