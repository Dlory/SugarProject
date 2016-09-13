using UnityEngine;
using System.Collections;

public class MagicAreaScript : MonoBehaviour {
	public bool flyIntoTouched = false;
	BoatScript boat;
	Collider2D[] colliders;

	void Start () {
		boat = GameObject.Find ("Player").GetComponent<BoatScript> ();
		colliders = GetComponents<Collider2D> ();
		foreach (Collider2D c in colliders) {
			c.isTrigger = true;
		}
		BroadcastSystem.defaultBoardcast.AddListener (BoatScript.BoatEndFlyEvent, new BroadcastSystem.Callback (BoatEndFlying));
	}

	void BoatEndFlying(BroadcastInfo arg) {
		BoatScript boat = arg.from as BoatScript;

		foreach (Collider2D c in colliders) {
			if (c.IsTouching (boat.LandCollider)) {
				flyIntoTouched = true;
			}
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject.tag == "Player") {
			flyIntoTouched = false;

			foreach (Collider2D c in colliders) {
				if (c.IsTouching (boat.LandCollider)) {
					flyIntoTouched = true;
				}
			}
		}
	}
}
