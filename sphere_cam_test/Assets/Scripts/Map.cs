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
    private long[,] mapData = new long[numRows, numColumns];


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
            } else if (entries [i] == "*") {
                mapData [row, i] = 4;
            } else if (entries [i] == "G") {
                mapData [row, i] = 8;
            } else if (entries [i] == "X") {
                mapData [row, i] = 16;
            } else if (entries [i] == "1") {
                mapData [row, i] = 32;
            } else if (entries [i] == "2") {
                mapData [row, i] = 64;
            } else if (entries [i] == "3") {
                mapData [row, i] = 128;
            } else if (entries [i] == "4") {
                mapData [row, i] = 256;
            } else {
                mapData [row, i] = 0;
            }
        }
    }

    public int NormalizeGridX (int unnormalizedGridX)
    {
        if ( unnormalizedGridX < 0 ) {
            return unnormalizedGridX + numColumns;
        } else if ( unnormalizedGridX >= numColumns ) {
            return numColumns - unnormalizedGridX;
        } else {
            return unnormalizedGridX;
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

    public int[] FindEntityGridCell (string entityName)
    {
        long mapDataValue;
        int[] gridRef = { -1, -1 };
        if ( entityName == "PlayerStart" ) {
            mapDataValue = 16;
        } else if ( entityName == "BaddyStart" ) {
            mapDataValue = 8;
        } else if ( entityName == "Baddy1Start" ) {
            mapDataValue = 32;
        } else if ( entityName == "Baddy2Start" ) {
            mapDataValue = 64;
        } else if ( entityName == "Baddy3Start" ) {
            mapDataValue = 128;
        } else if ( entityName == "Baddy4Start" ) {
            mapDataValue = 256;
        } else {
            throw new System.InvalidOperationException ("Invalid entityName " + entityName);
        }
        for ( int x=0; x<numColumns; x++ ) {
            for ( int y=0; y<numRows; y++ ) {
                //if ( ( mapData [y, x] & mapDataValue ) == 1 ) {
                if ( mapData [y, x] == mapDataValue ) {
                    gridRef[0] = x;
                    gridRef[1] = y;
                }
            }
        }
        if ( gridRef[0] == -1 ) {
            throw new System.InvalidOperationException ("Entity " + entityName + " not found");
        } else {
            return gridRef;
        }
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
        latitude = latitude - (gridSpacing / 2);
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
        if (y >= numRows) {
            return true;
        } else if (y < 0) {
            return true;
        } else if (x < 0) {
            return WallAtGridReference(x + numColumns, y);
        } else if (x >= numColumns ) {
            return WallAtGridReference(x - numColumns, y);
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

    public float angularDistanceToNextGridLine (float latitude, float longitude, Vector2 direction)
    {
        if (direction.y > 0) {
            // going up
            float nextGridLine = Mathf.Ceil (latitude / gridSpacing) * gridSpacing;
            return nextGridLine - latitude;
        } else if (direction.y < 0) {
            // going down
            float nextGridLine = Mathf.Floor (latitude / gridSpacing) * gridSpacing;
            return latitude - nextGridLine;
        } else if (direction.x > 0) {
            // going east
            float nextGridLine = Mathf.Ceil (longitude / gridSpacing) * gridSpacing;
            return nextGridLine - longitude;
        } else if (direction.x < 0) {
            // going west
            float nextGridLine = Mathf.Floor (longitude / gridSpacing) * gridSpacing;
            return longitude - nextGridLine;
        } else {
            // stopped, return a sensible default
            return 0;
        }
    }

    public float LatitudeSpeedAdjust (float angle)
    {
        float a = Mathf.Abs (angle);
        if (a >= 80) {
            return 6.0F;
        } else if (a >= 70) {
            return 3.0F;
        } else if (a >= 60) {
            return 2.0F;
        } else if (a >= 50) {
            return 1.5F;
        } else {
            return 1.0F;
        }
    }

    public float radiansToDegrees (float rads)
    {
        return rads * 180 / Mathf.PI;
    }

    public float degreesToRadians (float degrees)
    {
        return degrees * Mathf.PI / 180;
    }

    public int Rows ()
    {
        return numRows;
    }

    public int Columns ()
    {
        return numColumns;
    }

}

