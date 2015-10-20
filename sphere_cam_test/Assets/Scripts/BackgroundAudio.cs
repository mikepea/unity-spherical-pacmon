using UnityEngine;
using System.Collections;

public class BackgroundAudio : MonoBehaviour
{
  public AudioClip scaredSound;

  private GlobalGameDetails ggd;

  GlobalGameDetails GlobalState() {
      if (!ggd) {
        GameObject[] states = GameObject.FindGameObjectsWithTag ("PersistedState");
        ggd = states[0].GetComponent<GlobalGameDetails>();
      }
      return ggd;
  }

  void FixedUpdate() {
    GameObject[] baddies = GameObject.FindGameObjectsWithTag ("Baddy");
    bool someoneIsScared = false;
    foreach (GameObject baddy in baddies) {
      if ( baddy.GetComponent<PlayerSphericalMovement>().IsScared() ) {
        someoneIsScared = true;
      }
    }
    if ( GlobalState().AudioEnabled() ) {
      if ( someoneIsScared ) {
        if ( !GetComponent<AudioSource>().isPlaying ) {
          Debug.Log("Starting scaredSound!");
          GetComponent<AudioSource>().clip = scaredSound;
          GetComponent<AudioSource>().loop = true;
          GetComponent<AudioSource>().Play();
        }
      } else {
        GetComponent<AudioSource>().loop = false;
        GetComponent<AudioSource>().Stop();
      }
    }
  }

}
