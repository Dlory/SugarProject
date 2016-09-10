using UnityEngine;
using System.Collections;
using DG.Tweening;
public class FishLight : MonoBehaviour {
	public float OutTime = 1f;
	SpriteRenderer sr;

	// Use this for initialization
	void Start () {
		sr = transform.GetComponent<SpriteRenderer> ();
		sr.color = new Color32 (255, 255, 255, 0);
	}

	void UpdateLight(Color color){
		if (sr.color.a < color.a) {
			sr.DOColor (color, OutTime * (color.a - sr.color.a)).SetId(gameObject);
		}
		else{
			sr.DOColor (color, OutTime * (sr.color.a - color.a)).SetId(gameObject);
		}
	}

	public void FadeIn() {
		UpdateLight (new Color32 (255, 248, 0, 255));
	}

	public void FadeOut() {
		UpdateLight (new Color32 (255, 255, 255, 0));
	}
}
