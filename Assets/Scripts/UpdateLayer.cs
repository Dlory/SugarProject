using UnityEngine;
using System.Collections;

public class UpdateLayer : MonoBehaviour {
	GameObject player;
	float playerY;
	float myY;
	// Use this for initialization
	void Start () {
		player = GameObject.FindWithTag ("Player");
	}
	
	// Update is called once per frame
	void Update () {
		playerY = player.transform.localPosition.y;
		myY = transform.localPosition.y;
		if (myY < playerY) {
			int myOrder = transform.GetComponent<SpriteRenderer> ().sortingOrder;
			myOrder = 2;
			transform.GetComponent<SpriteRenderer> ().sortingOrder = myOrder;
		} 
		else {
			int myOrder = transform.GetComponent<SpriteRenderer> ().sortingOrder;
			myOrder = 0;
			transform.GetComponent<SpriteRenderer> ().sortingOrder = myOrder;
		}
	}
}
