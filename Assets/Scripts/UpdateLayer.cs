using UnityEngine;
using System.Collections;

public class UpdateLayer : MonoBehaviour {
	GameObject player;
	BoatScript boatScript;
	SpriteRenderer myRender;

	// Use this for initialization
	void Start () {
		player = GameObject.FindWithTag ("Player");
		boatScript = player.GetComponent<BoatScript> ();
		myRender = transform.GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		float playerY = player.transform.position.y;
		float myY = transform.position.y;
		if (myY < playerY && boatScript.Status != BoatStatus.Flying) {
			myRender.sortingOrder = 2;
		} 
		else {
			myRender.sortingOrder = 0;
		}
	}
}
