using UnityEngine;
using System.Text;
using System.IO;

public class Map
{
    public static int numRows = GlobalGameDetails.mapRows;
    public static int numColumns = GlobalGameDetails.mapColumns;
    public static float gridSpacing = GlobalGameDetails.cellSpacing;
    private int intGridSpacing = Mathf.FloorToInt (gridSpacing);

    public static float maxLatitude = GlobalGameDetails.maxAngleY;
    public static float minLatitude = GlobalGameDetails.minAngleY;

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
            string line;

            StreamReader theReader = new StreamReader (fileName, Encoding.Default);

            using (theReader) {
                do {
                    line = theReader.ReadLine ();
                    if (line != null) {
                        if (line.StartsWith ("#")) {
                            continue;
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

    public float NormalizeLongitude (float longitude)
    { 
        if (longitude > 180) {
            longitude = NormalizeLongitude (longitude - 360);
        } else if (longitude < -180) {
            longitude = NormalizeLongitude (longitude + 360);
        } else if (longitude == 180) {
            longitude = -180;
        }
        return longitude;
    }

    public float[] LatitudeLongitudeAtGridReference (int x, int y)
    {
        // x = 0 => -180
        // x = [numColumns -1] => 175
        // y = 0 => maxLatitude
        // y = 14 => 0deg
        // y = [numRows - 1] => minLatitude

        float longitude = (float)x * gridSpacing - 180f;
        float latitude = maxLatitude - (float)y * gridSpacing;

        float [] latLong = {latitude, longitude};
        return latLong;

    }

    public int[] GridReferenceAtLatitudeLongitude (float latitude, float longitude)
    {
        // grid position is centered on lat/long, so:
        // grid [0,0] is latitude +72.5 to +67.5, longitude +177.5 to -177.5
        // grid [max,0] is latitude +72.5 to +67.5, longitude +175 to +177.5
        // grid [0,max] is latitude -67.5 to -72.5 , longitude +177.5 to +177.5

        if (latitude > maxLatitude) {
            latitude = maxLatitude;
        } else if (latitude < minLatitude) {
            latitude = minLatitude;
        }

        longitude = NormalizeLongitude (longitude);

        float latitudeRange = maxLatitude - minLatitude;
        float adjustedLatitude = latitude + (gridSpacing / 2);
        int gridY = Mathf.FloorToInt ((latitudeRange - (latitude - minLatitude)) / gridSpacing);
        int intLatitude = Mathf.FloorToInt (Mathf.Round (latitude + gridSpacing / 2));
        int intLongitude = Mathf.FloorToInt (Mathf.Round (longitude + gridSpacing / 2));
        //Debug.Log ("intLatitude: " + intLatitude + ", intLongitude: " + intLongitude);
        int gridX = ((intLongitude + 180) % 360) / intGridSpacing;
        //Debug.Log ("gridX: " + gridX + ", gridY: " + gridY);
        int[] gridRef = {gridX, gridY};
        return gridRef;
    }

    public bool PillAtGridReference (int x, int y)
    {
        if (mapData [y, x] == 2) {
            Debug.Log ("Pill at x: " + x + ", y: " + y);
            return true;
        } else {
            return false;
        }
    }

    public bool PillAtMapReference (float latitude, float longitude)
    {
        int[] gridRef = GridReferenceAtLatitudeLongitude (latitude, longitude);
        return PillAtGridReference (gridRef [0], gridRef [1]);
    }

    public bool WallAtMapReference (float latitude, float longitude)
    {
        int[] gridRef = GridReferenceAtLatitudeLongitude (latitude, longitude);
        return WallAtGridReference (gridRef [1], gridRef [0]);
    }

    public bool WallAtGridReference (int x, int y)
    {
        if (y >= numColumns) {
            return true;
        } else if (y < 0) {
            return true;
        } else if (mapData [y, x] == 1) {
            return true;
        } else {
            return false;
        }
    }

    public Texture2D UVMappedTexture (int xPixels, int yPixels, int xOffsetPixels, int yOffsetPixels, bool enableGridLines)
    {
        Texture2D texture = new Texture2D (xPixels, yPixels);
        int xScaling = xPixels / 360;
        int yScaling = yPixels / 180;
        int fiveDegrees = 5 * xPixels / 360;

        Debug.Log ("xPixels: " + xPixels + ", yPixels: " + yPixels);
        Debug.Log ("xScaling: " + xScaling + ", yScaling: " + yScaling);
        Debug.Log ("fiveDegrees: " + fiveDegrees);

        for (int x = 0; x < xPixels; x++) {
            int gridX = x / (xScaling * intGridSpacing); 
            int xOffsetted = x - xOffsetPixels;
            if (xOffsetted < 0) {
                xOffsetted += xPixels;
            }

            for (int y = 0; y < yPixels; y++) {
                int gridY = gridYAtPixelY (y, yPixels);
                int yOffsetted = y - yOffsetPixels;
                if (yOffsetted < 0) {
                    yOffsetted += yPixels;
                }
                if (gridY < 0 || gridY >= numRows) {
                    texture.SetPixel (xOffsetted, yOffsetted, Color.red);
                } else if (WallAtGridReference (gridX, gridY)) {
                    texture.SetPixel (xOffsetted, yOffsetted, Color.blue);
                } else {
                    texture.SetPixel (xOffsetted, yOffsetted, Color.black);
                }
                if (enableGridLines) {
                    if (y % fiveDegrees == 0 || x % fiveDegrees == 0) {
                        texture.SetPixel (xOffsetted, yOffsetted, Color.white);
                    }
                }
            }
        }
        return texture;

    }

    private int gridYAtPixelY (int y, int yTotalPixels)
    {
        int yScaling = yTotalPixels / 180;
        return (180 / intGridSpacing) - (y / (yScaling * intGridSpacing) + ((90 - Mathf.FloorToInt (maxLatitude)) / intGridSpacing));
    }

}

