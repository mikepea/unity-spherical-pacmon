
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class PlayerCollision1 : MonoBehaviour
{

    public int numPills;
    public int playerMaxLives = 3;
    public string gameOverScene;
    public bool superPlayer;

    private int playerLivesRemaining;

    public AudioClip munch;
    private float lastPillMunchTime;
    private float pillMunchDelay;

    private GameObject scoreboard;
    private GameObject hiscoreboard;

    private GlobalGameDetails ggd;

    GlobalGameDetails GlobalState() {
        if (!ggd) {
          GameObject[] states = GameObject.FindGameObjectsWithTag ("PersistedState");
          ggd = states[0].GetComponent<GlobalGameDetails>();
        }
        return ggd;
    }

    void Start ()
    {
        pillMunchDelay = munch.length;
        lastPillMunchTime = - pillMunchDelay;
        numPills = GameObject.FindGameObjectsWithTag ("Pill").Length +
                   GameObject.FindGameObjectsWithTag ("Power Pill").Length;
        playerLivesRemaining = playerMaxLives;

        GameObject[] scoreboards = GameObject.FindGameObjectsWithTag("Scoreboard");
        scoreboard = scoreboards[0];
        GameObject[] hiscoreboards = GameObject.FindGameObjectsWithTag("HiScoreboard");
        hiscoreboard = hiscoreboards[0];
    }

    bool AudioEnabled ()
    {
        return GlobalState().AudioEnabled();
    }

    void MapIsCleared ()
    {
        Debug.Log ("MAP COMPLETE!");
        GlobalState().SendMessage("NextMap");
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
                IncreaseScore(200);
                other.gameObject.SendMessage ("EnterDeadMode");
            } else if ( other.gameObject.GetComponent<PlayerSphericalMovement>().IsDead() == true ) {
              // ignore the dead
            } else {
                if ( ! superPlayer ) {
                  PlayerHasDied ();
                }
            }
        } else if (other.gameObject.tag == "Pill" || other.gameObject.tag == "Power Pill" ) {
            IncreaseScore(10);
            lastPillMunchTime = Time.time;
            other.renderer.enabled = false;
            Destroy(other.gameObject, 0.5f);
            if (other.gameObject.tag == "Power Pill") {
                other.gameObject.SetActive (false);
                GameObject[] baddies = GameObject.FindGameObjectsWithTag ("Baddy");
                foreach (GameObject baddy in baddies) {
                    baddy.SendMessage ("EnterScaredMode");
                }
            }
        }
    }

    int Score ()
    {
      return GlobalState().Score();
    }

    int HighScore() {
      return GlobalState().HighScore();
    }

    void IncreaseScore(int increment) {
      GlobalState().SendMessage("IncreaseScore", increment);
    }

    void UpdateHighScore() {
      if ( Score() > HighScore() ) {
        if ( GlobalState().GameMode() != "GameDemo" ) {
          GlobalState().SendMessage("SetHighScore", Score());
        }
      }
    }
    void FixedUpdate ()
    {
        UpdateHighScore();
        DisplayScore();
        numPills = GameObject.FindGameObjectsWithTag ("Pill").Length + 
                   GameObject.FindGameObjectsWithTag ("Power Pill").Length;
        if ( this.name == "Player" && numPills == 0 ) {
          MapIsCleared ();
        }
        if ( AudioEnabled() ) {
          if ( lastPillMunchTime + pillMunchDelay > Time.time ) {
            if ( ! audio.isPlaying ) {
              audio.clip = munch;
              audio.loop = true;
              audio.Play();
            }
          } else {
            audio.loop = false;
          }
        }
    }

    void DisplayScore() 
    {
        scoreboard.GetComponent<TextMesh>().text = Score().ToString();
        hiscoreboard.GetComponent<TextMesh>().text = "HI: " + HighScore().ToString();
    }

}
