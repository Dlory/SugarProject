using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragRippleScript : MonoBehaviour {
	public SpriteRenderer sr;
	public GameObject rippleObject;
	public List<Sprite> sprites = new List<Sprite>();
	public float force = 1;

	BoxCollider2D mBoxCollider2D;

	void Start () {
		sr.sprite = sprites [Random.Range (0, sprites.Count-1)];
		mBoxCollider2D = GetComponent<BoxCollider2D> ();
	}

	public void HideRipple() {
		if (rippleObject != null) {
			Destroy (rippleObject);
		}
	}

	void OnTriggerStay2D(Collider2D other) {
		if (other.tag == "Player" && mBoxCollider2D.IsTouching(other)) {
			Rigidbody2D boatRigidbody = other.GetComponent<Rigidbody2D> ();
			Vector2 f = ForceAtCollider (mBoxCollider2D);
			boatRigidbody.AddForce (f, ForceMode2D.Force);
		}	
	}

	Vector2 ForceAtCollider(BoxCollider2D collider) {
		float angle = collider.gameObject.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
		Vector2 target = Vector2.right;
		float x = Mathf.Cos(angle) * (target.x) - Mathf.Sin(angle) * (target.y);
		float y = Mathf.Cos(angle) * (target.y) + Mathf.Sin(angle) * (target.x);
		Vector2 v = new Vector2(x, y);
		Vector2 f =  force * v;
		return f;
	}
}
