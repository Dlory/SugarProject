using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StreamWaterScript : MonoBehaviour {
	public float Force = 1;
	public bool Inverse = false;
	public Collider2D[] FixForces;

	bool m_Inverse {
		set {
			Animator.SetBool ("Inverse", value);
		}
		get {
			return Animator.GetBool("Inverse");
		}
	}

	GameObject Boat;
	Rigidbody2D BoatRigidbody;
	Collider2D Collider2D;
	Animator Animator;

	void Start () {
		Boat = GameObject.FindGameObjectWithTag ("Player");
		BoatRigidbody = Boat.GetComponent<Rigidbody2D> ();
		Collider2D = GetComponent<Collider2D> ();
		Animator = GetComponent<Animator> ();
		m_Inverse = Inverse;

		if (Collider2D) {
			Collider2D.isTrigger = true;
		}
	}

	void OnTriggerStay2D (Collider2D other) {
		if (m_Inverse != Inverse) {
			m_Inverse = Inverse;
		}

		if (other.gameObject.tag == "Player") {
			BoxCollider2D[] colliders = GetComponentsInChildren<BoxCollider2D> ();
			if (colliders != null) {
				foreach (BoxCollider2D c in colliders) {
					if (c != Collider2D && c.IsTouching (other)) {
						BoatRigidbody.AddForce (ForceAtCollider (c), ForceMode2D.Force);
					}
				}
			}
		}
	}

	Vector2 ForceAtCollider(BoxCollider2D collider) {
		//Vector3 rotation = collider.gameObject.transform.rotation.eulerAngles;
		//float rad = Mathf.Deg2Rad * rotation.z;

		float angle = collider.gameObject.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
		Vector2 target = Vector2.right;
		float x = Mathf.Cos(angle) * (target.x) - Mathf.Sin(angle) * (target.y);
		float y = Mathf.Cos(angle) * (target.y) + Mathf.Sin(angle) * (target.x);
		Vector2 v = new Vector2(x, y);
		Vector2 force =  Force * v * -1;

		bool isFixForce = false;
		foreach(Collider2D c in FixForces) {
			if (c == collider) {
				isFixForce = true;
				break;
			}
		}
		if (!isFixForce) {
			force *= (Inverse ? -1 : 1);
		}

		return force;
	}
}
