using UnityEngine;
using System.Collections;

public class MagicAreaScript : MonoBehaviour {
	public bool flyIntoTouched = false;
	BoatScript boat;

	void Start () {
		boat = GameObject.Find ("Player").GetComponent<BoatScript> ();
		Collider2D collider = GetComponent<Collider2D> ();
		if (collider != null) {
			collider.isTrigger = true;
		}
		BroadcastSystem.defaultBoardcast.AddListener (BoatScript.BoatEndFlyEvent, new BroadcastSystem.Callback (BoatEndFlying));
	}

	void BoatEndFlying(BroadcastInfo arg) {
		BoatScript boat = arg.from as BoatScript;
		Collider2D collider = GetComponent<Collider2D> ();

		if (collider.IsTouching (boat.LandCollider)) {
			flyIntoTouched = true;
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject.tag == "Player") {
			flyIntoTouched = false;
		}
	}
}
