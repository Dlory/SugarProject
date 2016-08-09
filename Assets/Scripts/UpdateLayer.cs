using UnityEngine;
using System.Collections;

public class UpdateLayer : MonoBehaviour {
	GameObject player;
	float playerY;
	float myY;
	SpriteRenderer myRender;
	// Use this for initialization
	void Start () {
		player = GameObject.FindWithTag ("Player");
		myRender = transform.GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		playerY = player.transform.position.y;
		myY = transform.position.y;
		if (myY < playerY) {
			myRender.sortingOrder = 2;
		} 
		else {
			myRender.sortingOrder = 0;
		}
	}
}
