using UnityEngine;
using System.Collections;

public class CrocodileHitScript : TrampolineScript {
	public GameObject Fragment;
	new void Start(){
		base.Start ();

		Collider2D collider = GetComponent<Collider2D> ();
		collider.isTrigger = false;
	}

	override public void BounceBoat() {
		base.BounceBoat ();

		Crocodile crocodile = GetComponent<Crocodile> ();
		if (crocodile.currentStatus != CrocodileStatus.Crush) {
			crocodile.PlayAnimation (CrocodileStatus.Crush);
			if (!Fragment.active) {
				Fragment.SetActive (true);
				Fragment.GetComponent<Animator>().SetTrigger ("Spit");
			}
		}
	}
}
