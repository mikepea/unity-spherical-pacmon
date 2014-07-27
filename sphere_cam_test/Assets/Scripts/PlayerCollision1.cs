
using UnityEngine;
using System.Collections;

public class PlayerCollision1 : MonoBehaviour
{

    public int numPills;
    public int playerMaxLives = 3;
    public string gameOverScene;

    private int playerLivesRemaining;

    void Start ()
    {
        numPills = GameObject.FindGameObjectsWithTag ("Pill").Length +
                   GameObject.FindGameObjectsWithTag ("Power Pill").Length;
        playerLivesRemaining = playerMaxLives;
    }

    void MapIsCleared ()
    {
        Debug.Log ("MAP COMPLETE!");
        Application.LoadLevel (Application.loadedLevel);
    }

    void ResetPlayerPositions ()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
        foreach (GameObject player in players) {
            player.SendMessage ("Stop");
            player.SendMessage ("PutPlayerAtStartPosition");
        }
        GameObject[] baddies = GameObject.FindGameObjectsWithTag ("Baddy");
        foreach (GameObject baddy in baddies) {
            baddy.SendMessage ("Stop");
            baddy.SendMessage ("PutPlayerAtStartPosition");
        }
    }

    void PlayerHasDied ()
    {
        if (playerLivesRemaining == 0) {
            Application.LoadLevel (gameOverScene);
            playerLivesRemaining = playerMaxLives;
        } else {
            playerLivesRemaining--;
            ResetPlayerPositions ();
        }
    }

    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.tag == "Baddy") {
            if ( other.gameObject.GetComponent<PlayerSphericalMovement>().IsScared() == true ) {
                other.gameObject.SendMessage ("EnterDeadMode");
            } else if ( other.gameObject.GetComponent<PlayerSphericalMovement>().IsDead() == true ) {
              // ignore the dead
            } else {
                PlayerHasDied ();
            }
        } else if (other.gameObject.tag == "Pill" || other.gameObject.tag == "Power Pill" ) {
            other.gameObject.SetActive (false);
            numPills = GameObject.FindGameObjectsWithTag ("Pill").Length + 
                       GameObject.FindGameObjectsWithTag ("Power Pill").Length;
            Debug.Log (numPills + " pills remaining");
            if (numPills == 0) {
                MapIsCleared ();
            }
            if (other.gameObject.tag == "Power Pill") {
                other.gameObject.SetActive (false);
                GameObject[] baddies = GameObject.FindGameObjectsWithTag ("Baddy");
                foreach (GameObject baddy in baddies) {
                    baddy.SendMessage ("EnterScaredMode");
                }
            }
        }
    }

}
