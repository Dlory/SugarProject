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
		Debug.Log ("PlayerY:" + playerY + ",myY:" + myY);
		if (myY < playerY) {
			int myOrder = transform.GetComponent<SpriteRenderer> ().sortingOrder;
			myOrder = 1;
			transform.GetComponent<SpriteRenderer> ().sortingOrder = myOrder;
			int playerOrder = player.transform.GetComponent<SpriteRenderer> ().sortingOrder;
			playerOrder = 0;
			player.transform.GetComponent<SpriteRenderer> ().sortingOrder = playerOrder;
		} 
		else {
			int myOrder = transform.GetComponent<SpriteRenderer> ().sortingOrder;
			myOrder = 0;
			transform.GetComponent<SpriteRenderer> ().sortingOrder = myOrder;
			int playerOrder = player.transform.GetComponent<SpriteRenderer> ().sortingOrder;
			playerOrder = 1;
			player.transform.GetComponent<SpriteRenderer> ().sortingOrder = playerOrder;
		
		}
	}
}
