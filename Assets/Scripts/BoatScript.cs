using UnityEngine;
using System.Collections;

public class BoatScript : MonoBehaviour {
	public float forceScale = 1;
	public float joyfulPoint = 0;
	public float attractionArea {
		get { return joyfulPoint * 2.0f; }
	}
	private Rigidbody2D Rigidbody;
	private Vector2 PreviousFrameVelocity = Vector3.zero;
	private Animator anim;
	private int FlyActionHash = Animator.StringToHash("Fly");
	private int ShakeActionHash = Animator.StringToHash("Shake");

	void Start() {
		Rigidbody = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
	}

	void Update(){
		PreviousFrameVelocity = Rigidbody.velocity;
		anim.SetBool (FlyActionHash, Input.GetKey (KeyCode.Z));
		anim.SetBool (ShakeActionHash, Input.GetKey (KeyCode.X));
	}

	void OnTriggerEnter2D(Collider2D other) {
		string tag = other.gameObject.tag;

		if (tag == "Ripple") {
			RippleScript script = other.gameObject.GetComponentInParent<RippleScript> ();
			float impact = script.Impact;
			Vector3 force = impact * (transform.position - other.gameObject.transform.position).normalized;
			UpdateVelocityByCombineNewForce (new Vector2(force.x, force.y));
		} else if (tag == "Fragment") {
			// TODO 做收集动画
			BroadcastSystem.defaultBoardcast.SendMessage (GameGUI.FragmenCollectEvent, other.gameObject, null);
			Destroy(other.gameObject);
		}
	}

	// TODO 多个波加入的时候对力的影响的作用优化
	void UpdateVelocityByCombineNewForce(Vector2 force) {
		Vector2 currentForce = Rigidbody.velocity;

		Vector2 newForce = new Vector2 ();

		if (force.x * currentForce.x > 0) {
			newForce.x = MaxForce (force.x, currentForce.x) + MinForce(force.x, currentForce.x) * 0;
		} else {
			newForce.x = currentForce.x + force.x;
		}
		if (force.y * currentForce.y > 0) {
			newForce.y = MaxForce (force.y, currentForce.y);
		} else {
			newForce.y = currentForce.y + force.y + MinForce(force.y, currentForce.y) * 0;
		}
		Rigidbody.velocity = newForce;
		print("force:" + force);
		Rigidbody.AddForce (force * forceScale, ForceMode2D.Force);


		GameObject.FindWithTag ("Fish").SendMessage ("UpdateForce",force * forceScale);

	}

	float MaxForce(float force1, float force2) {
		if (Mathf.Abs (force1) > Mathf.Abs (force2)) {
			return force1;
		}
		return force2;
	}

	float MinForce(float force1, float force2) {
		if (Mathf.Abs (force1) > Mathf.Abs (force2)) {
			return force2;
		}
		return force1;
	}

}