using UnityEngine;
using System.Collections;

public class BoatScript : MonoBehaviour {
	public float joyfulPoint = 0;
	public float attractionArea {
		get { return joyfulPoint * 2.0f; }
	}
	private Rigidbody2D Rigidbody2D;
	private Vector2 PreviousFrameVelocity = Vector3.zero;
	private Animator anim;
	private int FlyActionHash = Animator.StringToHash("Fly");
	private int ShakeActionHash = Animator.StringToHash("Shake");

	void Start() {
		Rigidbody2D = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
	}

	void Update(){
		PreviousFrameVelocity = Rigidbody2D.velocity;
		anim.SetBool (FlyActionHash, Input.GetKey (KeyCode.Z));
		anim.SetBool (ShakeActionHash, Input.GetKey (KeyCode.X));
	}

	void OnTriggerEnter2D(Collider2D other) {
		string tag = other.gameObject.tag;

		if (tag == Constant.TagRipple) {
			RippleScript script = other.gameObject.GetComponentInParent<RippleScript> ();
			float impact = script.Impact;
			Vector3 force = impact * (transform.position - other.gameObject.transform.position).normalized;
			UpdateVelocityByCombineNewForce (new Vector2(force.x, force.y));
		} else if (tag == Constant.TagFragment) {
			// TODO 做收集动画
			BroadcastSystem.defaultBoardcast.SendMessage (GameGUI.FragmenCollectEvent, other.gameObject, null);
			Destroy(other.gameObject);
		}
	}

	// TODO 多个波加入的时候对力的影响的作用优化
	void UpdateVelocityByCombineNewForce(Vector2 force) {
		Vector2 velocity = Rigidbody2D.velocity;
		Vector2 impact = Vector2.zero;

		if (force.x * velocity.x < 0) {
			impact.x = force.x;
		} else {
			impact.x = Mathf.Abs (force.x) > Mathf.Abs (velocity.x) ? force.x - velocity.x : 0;
		}
		if (force.y * velocity.y < 0) {
			impact.y = force.y;
		} else {
			impact.y = Mathf.Abs (force.y) > Mathf.Abs (velocity.y) ? force.y - velocity.y : 0;
		}
		Rigidbody2D.AddForce (impact, ForceMode2D.Impulse);
	}
}
