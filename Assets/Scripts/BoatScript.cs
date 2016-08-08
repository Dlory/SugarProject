using UnityEngine;
using System.Collections;

public enum BoatStatus {
	Idle = 0,
	ReadyToFly = 1,
	Flying = 2,
};

public class BoatScript : MonoBehaviour {
	public BoatStatus Status;
	public float JoyfulPoint = 0;
	public float AttractionArea {
		get { return JoyfulPoint * 2.0f; }
	}
	private Rigidbody2D Rigidbody2D;
	private Collider2D PlayerCollider2D;
	private Animator Anim;
	private Vector2 FirstTouchPoint;

	private int FlyActionHash = Animator.StringToHash("Fly");
	private int ShakeActionHash = Animator.StringToHash("Shake");
	private int PlayerLayer;

	void Start() {
		Rigidbody2D = GetComponent<Rigidbody2D> ();
		Anim = GetComponent<Animator> ();
		PlayerCollider2D = GetComponent<Collider2D> ();

		FirstTouchPoint = Vector2.zero;
		PlayerLayer = LayerMask.NameToLayer ("Player");
	}

	void Update(){
		if (GUIUtility.hotControl == 0 && (Input.GetMouseButton (0) || Input.touchCount > 0)) {
			Vector2 point;
			if (Input.GetMouseButton (0) == false) {
				point = Camera.main.ScreenToWorldPoint (Input.touches [0].position);
			} else {
				point = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			}

			RaycastHit2D hit = Physics2D.Raycast (point, Vector2.zero, 100, PlayerLayer);
			if (hit.collider) {
				if (FirstTouchPoint == Vector2.zero) {
					FirstTouchPoint = point;
				}
				if (Vector2.Distance (FirstTouchPoint, point) > 1) {
					Vector2 v = point - FirstTouchPoint;
					print ("v " + v);
				}
			}
		} else {
			FirstTouchPoint = Vector2.zero;
		}

		// Anim.SetBool (FlyActionHash, Input.GetKey (KeyCode.Z));
		// Anim.SetBool (ShakeActionHash, Input.GetKey (KeyCode.X));
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (PlayerCollider2D.IsTouching (other)) {
			string tag = other.gameObject.tag;
			if (tag == Constant.TagRipple) {
				RippleScript script = other.gameObject.GetComponentInParent<RippleScript> ();
				float impact = script.Impact;
				Vector3 force = impact * (transform.position - other.gameObject.transform.position).normalized;
				UpdateVelocityByCombineNewForce (new Vector2(force.x, force.y));
			} else if ( tag == Constant.TagFragment) {
				// TODO 做收集动画
				BroadcastSystem.defaultBoardcast.SendMessage (GameGUI.FragmenCollectEvent, other.gameObject, null);
				Destroy(other.gameObject);
			}
		}
	}

	void UpdateVelocityByCombineNewForce(Vector2 force) {
		Vector2 velocity = Rigidbody2D.velocity;
		Vector2 impact = Vector2.zero;

		if (force.x * velocity.x < 0) { //力的方向与移动方向一致
			impact.x = force.x;
		} else {
			impact.x = Mathf.Abs (force.x) > Mathf.Abs (velocity.x) ? force.x - velocity.x : 0;
		}
		if (force.y * velocity.y < 0) { //力的方向与移动方向一致
			impact.y = force.y;
		} else {
			impact.y = Mathf.Abs (force.y) > Mathf.Abs (velocity.y) ? force.y - velocity.y : 0;
		}
		Rigidbody2D.AddForce (impact, ForceMode2D.Impulse);
	}
}
