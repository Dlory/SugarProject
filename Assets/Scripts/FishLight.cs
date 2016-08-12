using UnityEngine;
using System.Collections;
using DG.Tweening;
public class FishLight : MonoBehaviour {
	SpriteRenderer sr;
	public float OutTime;
	NewFishScript nfs;
	// Use this for initialization
	void Start () {
		sr = transform.GetComponent<SpriteRenderer> ();
		sr.color = new Color32 (255, 255, 255, 0);
		nfs = transform.parent.GetComponent<NewFishScript> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter2D(Collider2D col){
		DOTween.Kill (gameObject);
		UpdateLight (new Color32 (255, 248, 0, 255));
		nfs.RandomMoveInCircle ();
	}

	void OnTriggerExit2D(Collider2D col){
		DOTween.Kill (gameObject);
		UpdateLight (new Color32 (255, 255, 255, 0));
	}

	public void UpdateLight(Color color){
		if (sr.color.a < color.a) {
			sr.DOColor (color, OutTime * (color.a - sr.color.a)).SetId(gameObject);
		}
		else{
			sr.DOColor (color, OutTime * (sr.color.a - color.a)).SetId(gameObject);
		}
	}
}
