using UnityEngine;
using System.Collections;

public class StreamWaterScript : MonoBehaviour {
	public float Force = 1;

	Rigidbody2D boatRigidbody;
	Collider2D Collider2D;

	// Use this for initialization
	void Start () {
		Collider2D = GetComponent<Collider2D> ();
		if (Collider2D) {
			Collider2D.isTrigger = true;
		}
	}
	
	// Update is called once per frame
	void FixedUpdate() {
		if (boatRigidbody) {
			float rad = Mathf.Deg2Rad * this.transform.rotation.eulerAngles.z;
			Vector2 force =  Force * new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
			boatRigidbody.AddRelativeForce (Force * force, ForceMode2D.Force);
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == Constant.TagPlayer && boatRigidbody == null) {
			boatRigidbody = other.gameObject.GetComponent<Rigidbody2D> ();
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.tag == Constant.TagPlayer) {
			boatRigidbody = null;
		}
	}
}
