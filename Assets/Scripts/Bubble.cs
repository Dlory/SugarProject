using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Bubble : MonoBehaviour {
	public Animator animator;
	// Use this for initialization
	void Start () {
		Invoke("RandomAnimation",Random.Range(0,0.5f));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void RandomAnimation(){
		int i = Random.Range (1, 5);
		switch (i) {
		case 1:
			animator.PlayInFixedTime ("BubblePath1", 0, 0);
			break;
		case 2:
			animator.PlayInFixedTime ("BubblePath2", 0, 0);
			break;
		case 3:
			animator.PlayInFixedTime ("BubblePath3", 0, 0);
			break;
		case 4:
			animator.PlayInFixedTime ("BubblePath4", 0, 0);
			break;
		}

	}

	public void OnTriggerEnter2D(Collider2D col){
		if (col.gameObject.tag == "Hamster") {
			gameObject.GetComponent<PolygonCollider2D>().enabled = false;
			gameObject.GetComponent<SpriteRenderer>().sortingLayerName = "OverAll";
			switch (gameObject.name) {
			case "Bubble1":
				animator.PlayInFixedTime ("BubbleDestroy1", 0, 0);
				break;
			case "Bubble2":
				animator.PlayInFixedTime ("BubbleDestroy2", 0, 0);
				break;
			case "Bubble3":
				animator.PlayInFixedTime ("BubbleDestroy3", 0, 0);
				break;
			case "Bubble4":
				animator.PlayInFixedTime ("BubbleDestroy4", 0, 0);
				break;
			}

			BubbleJump (col.gameObject.transform.parent.gameObject);
		}

	}

	public void BubbleJump(GameObject g){
		Vector3 myPosition = gameObject.transform.position;
		Vector3 playerPosition = g.transform.position;
		Vector3 direction = (myPosition - playerPosition).normalized;
		iTween.MoveTo (gameObject,iTween.Hash("position",myPosition + direction*2,"time",0.75f,"easetype",iTween.EaseType.easeOutQuart));
		gameObject.transform.SetParent (g.transform);
	}

	public void BubbleToCircle(){
		Debug.Log ("test");
		//iTween.MoveTo (gameObject, iTween.Hash ("position", new Vector3 (1, 1, 0), "time", 1));
		transform.DOLocalMove(new Vector3(1,1,0),1);
	}
}
