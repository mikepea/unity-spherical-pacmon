
using UnityEngine;
using System.Collections;

public class PlayerCollision1 : MonoBehaviour
{

    public int numPills;
    public int playerMaxLives = 3;
    public string gameOverScene;
    public bool superPlayer;

    private int score;
    private int playerLivesRemaining;

    void Start ()
    {
        numPills = GameObject.FindGameObjectsWithTag ("Pill").Length +
                   GameObject.FindGameObjectsWithTag ("Power Pill").Length;
        playerLivesRemaining = playerMaxLives;
        score = 0;
    }

    int Score ()
    {
        return score;
    }

    void MapIsCleared ()
    {
        Debug.Log ("MAP COMPLETE!");
        Application.LoadLevel (Application.loadedLevel);
    }

    void DisableAllBaddies() {
        GameObject[] baddies = GameObject.FindGameObjectsWithTag ("Baddy");
        foreach (GameObject baddy in baddies) {
            baddy.renderer.enabled = false; // dont SetActive(false), as cannot then find it.
        }
    }

    void PlayerHasDied ()
    {
        DisableAllBaddies();
        if (playerLivesRemaining == 0) {
            //Application.LoadLevel (gameOverScene);
            //playerLivesRemaining = playerMaxLives;
            this.SendMessage ("HasDied");
            this.SendMessage ("GameOver");
        } else {
            playerLivesRemaining--;
            this.SendMessage ("HasDied");
            //ResetPlayerPositions ();
        }
    }

    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.tag == "Baddy") {
            if ( other.gameObject.GetComponent<PlayerSphericalMovement>().IsScared() == true ) {
                score += 200;
                other.gameObject.SendMessage ("EnterDeadMode");
            } else if ( other.gameObject.GetComponent<PlayerSphericalMovement>().IsDead() == true ) {
              // ignore the dead
            } else {
                if ( ! superPlayer ) {
                  PlayerHasDied ();
                }
            }
        } else if (other.gameObject.tag == "Pill" || other.gameObject.tag == "Power Pill" ) {
            score += 10;
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

    void FixedUpdate ()
    {
        DisplayScore();
    }

    void DisplayScore() 
    {
        Debug.Log("Score is " + score);
    }

}
