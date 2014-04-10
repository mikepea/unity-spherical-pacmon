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

    public bool PillAtMapReference (float latitude, float longitude)
    {
        // longitude goes from -180 to 180
        // latitude goes from -80 to 80
        // our grid[0,0] is -180 long, +80 lat
        int intLatitude = Mathf.FloorToInt (Mathf.Round (latitude));
        int intLongitude = Mathf.FloorToInt (Mathf.Round (longitude));

        Debug.Log ("intLatitude: " + intLatitude + ", intLongitude: " + intLongitude);

        int gridX = ((intLongitude + 180) % 360) / gridSpacing;
        int gridY = (160 - ((intLatitude + 80) % 180)) / gridSpacing;

        Debug.Log ("gridX: " + gridX + ", gridY: " + gridY);

        if (mapData [gridY, gridX] == 2) {
            return true;
        } else {
            return false;
        }
    }

}

