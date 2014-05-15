
using UnityEngine;
using System.Collections;

public class PlayerCollision1 : MonoBehaviour {

    public int numPills;
    public bool chaseMode = false;

    void Start ()
    {
        numPills = GameObject.FindGameObjectsWithTag ("Pill").Length;
    }

    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.tag == "Baddy") {
            if ( chaseMode ) {
                other.gameObject.SetActive (false);
            } else {
                this.gameObject.SetActive (false);
            }
        } else if (other.gameObject.tag == "Pill") {
            other.gameObject.SetActive (false);
            numPills = GameObject.FindGameObjectsWithTag ("Pill").Length;
            Debug.Log (numPills + " pills remaining");
            if ( numPills == 0 ) {
                // reset map
                Debug.Log ("MAP COMPLETE!");
            }
        } else if (other.gameObject.tag == "Power Pill") {
            other.gameObject.SetActive (false);
            chaseMode = true;
        }
    }

}
