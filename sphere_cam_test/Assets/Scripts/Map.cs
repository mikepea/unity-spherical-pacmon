using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;

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
                mapData [row, i] = EntityDataValue("Wall");
            } else if (entries [i] == "p") {
                mapData [row, i] = EntityDataValue("RegularPill");
            } else if (entries [i] == "*") {
                mapData [row, i] = EntityDataValue("PowerPill");
            } else if (entries [i] == "-") {
                mapData [row, i] = EntityDataValue("BaddyDoor");
            } else if (entries [i] == "X") {
                mapData [row, i] = EntityDataValue("PlayerStart");
            } else if (entries [i] == "1") {
                mapData [row, i] = EntityDataValue("Baddy1Start");
            } else if (entries [i] == "2") {
                mapData [row, i] = EntityDataValue("Baddy2Start");
            } else if (entries [i] == "3") {
                mapData [row, i] = EntityDataValue("Baddy3Start");
            } else if (entries [i] == "4") {
                mapData [row, i] = EntityDataValue("Baddy4Start");
            } else {
                mapData [row, i] = EntityDataValue("EmptyCell");
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
        // y = 0 => minLatitude
        // y = 14 => 0deg
        // y = [numRows - 1] => maxLatitude

        float longitude = (float)x * gridSpacing - 180f;
        float latitude = minLatitude + (float)y * gridSpacing;

        float [] latLong = {latitude, longitude};
        return latLong;

    }

    public float[] LatitudeLongitudeAtGridReference (Vector2 gridRef)
    {
        return LatitudeLongitudeAtGridReference((int)gridRef.x, (int)gridRef.y);
    }

    public int EntityDataValue (string entityName)
    {
        if ( entityName == "EmptyCell" ) {
            return 0;
        } else if ( entityName == "Wall" ) {
            return 1;
        } else if ( entityName == "RegularPill" ) {
            return 2;
        } else if ( entityName == "PowerPill" ) {
            return 4;
        } else if ( entityName == "BaddyDoor" ) {
            return 8;
        } else if ( entityName == "PlayerStart" ) {
            return 16;
        } else if ( entityName == "BaddyStart" ) {
            return 8;
        } else if ( entityName == "Baddy1Start" ) {
            return 32;
        } else if ( entityName == "Baddy2Start" ) {
            return 64;
        } else if ( entityName == "Baddy3Start" ) {
            return 128;
        } else if ( entityName == "Baddy4Start" ) {
            return 256;
        } else {
            throw new System.InvalidOperationException ("Invalid entityName " + entityName);
        }
    }

    public Vector2 FindEntityGridRef (string entityName)
    {
        for ( int col=0; col<numColumns; col++ ) {
            for ( int row=0; row<numRows; row++ ) {
                if ( mapData [row, col] == EntityDataValue(entityName) ) {
                    return new Vector2 (col, (numRows - 1) - row);
                }
            }
        }
        throw new System.InvalidOperationException ("Entity " + entityName + " not found");
    }

    public bool IsEntityAtGridRef (string entityName, int x, int y)
    {
        if ( y < 0 || y >= numRows ) {
          return false;
        }

        if ( mapData [(numRows - 1) - y, NormalizeGridX(x)] == EntityDataValue(entityName) ) {
          return true;
        } else {
          return false;
        }
    }

    public bool IsEntityAtGridRef (string entityName, Vector2 gridRef)
    {
        int x = (int)gridRef.x;
        int y = (int)gridRef.y;
        return IsEntityAtGridRef(entityName, x, y);
    }

    public Vector2 GridReferenceAtLatitudeLongitude (float latitude, float longitude)
    {
        if (latitude > maxLatitude) {
            latitude = maxLatitude;
        } else if (latitude < minLatitude) {
            latitude = minLatitude;
        }

        longitude = NormalizeLongitude (longitude);

        latitude = latitude + (gridSpacing / 2);
        int gridY = Mathf.FloorToInt ((latitude - minLatitude) / gridSpacing);
        int intLongitude = Mathf.FloorToInt (Mathf.Round (longitude + gridSpacing / 2));
        int gridX = ((intLongitude + 180) % 360) / intGridSpacing;
        return new Vector2 ( gridX, gridY );
    }

    public bool PillAtGridReference (int x, int y)
    {
        if (IsEntityAtGridRef("RegularPill", x, y) || IsEntityAtGridRef("PowerPill", x, y) ) {
            return true;
        } else {
            return false;
        }
    }

    public bool PowerPillAtGridReference (int x, int y)
    {
        return IsEntityAtGridRef("PowerPill", x, y);
    }

    public bool PillAtMapReference (float latitude, float longitude)
    {
        Vector2 gridRef = GridReferenceAtLatitudeLongitude (latitude, longitude);
        return PillAtGridReference ((int)gridRef.x, (int)gridRef.y);
    }

    public bool WallAtMapReference (float latitude, float longitude)
    {
        Vector2 gridRef = GridReferenceAtLatitudeLongitude (latitude, longitude);
        return WallAtGridReference ((int)gridRef.x, (int)gridRef.y);
    }

    public bool WallAtGridReference (int x, int y)
    {
        if (y >= numRows) {
            return true;
        } else if (y < 0) {
            return true;
        } else if (IsEntityAtGridRef("Wall", x, y)) {
            return true;
        } else {
            return false;
        }
    }

    public bool WallAtGridReference (Vector2 gridRef)
    {
        int x = (int)gridRef.x;
        int y = (int)gridRef.y;
        return WallAtGridReference(x, y);
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
        return ( 1.0F / Mathf.Cos(degreesToRadians(angle)) );
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

    public List<Vector2> AvailableDirectionsAtGridRef (int x, int y)
    {
       List<Vector2> directions = new List<Vector2>();
       if ( ! WallAtGridReference(x-1, y) ) {
         directions.Add(- Vector2.right);
       }
       if ( ! WallAtGridReference(x, y+1) ) {
         directions.Add(Vector2.up);
       }
       if ( ! WallAtGridReference(x+1, y) ) {
         directions.Add(Vector2.right);
       }
       if ( ! WallAtGridReference(x, y-1) ) {
         directions.Add(- Vector2.up);
       }
       return directions;
    }

    public List<Vector2> AvailableDirectionsAtGridRef (Vector2 gridRef)
    {
      int x = (int)gridRef.x;
      int y = (int)gridRef.y;
      return AvailableDirectionsAtGridRef(x, y);
    }

    public float DistanceBetween (Vector2 point1, Vector2 point2)
    {
      if ( point1.x - point2.x > (float)numColumns / 2 ) {
        Debug.Log("DistanceBetween: Wrapping " + point1 + " " + point2);
        return Vector2.SqrMagnitude ( point1 - point2 - new Vector2 ((float)numColumns, 0) );
      } else if ( point2.x - point1.x > (float)numColumns / 2 ) {
        Debug.Log("DistanceBetween: Wrapping " + point1 + " " + point2);
        return Vector2.SqrMagnitude ( point2 - point1 - new Vector2 ((float)numColumns, 0) );
      } else {
        return Vector2.SqrMagnitude(point1 - point2);
      }
    }

}

