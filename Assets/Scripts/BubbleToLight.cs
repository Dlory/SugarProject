using UnityEngine;
using System.Collections;

public class BubbleToLight : MonoBehaviour {
	public Animator animator;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnTriggerEnter2D(Collider2D col){
		if (col.gameObject.tag == "Hamster") {
			gameObject.GetComponent<PolygonCollider2D>().enabled = false;
			gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "OverAll";
			animator.PlayInFixedTime ("BubbleDestroy");
		}

	}
}
