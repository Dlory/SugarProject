using UnityEngine;
using System.Collections;

public class TrampolineScript : MonoBehaviour {
	public bool hitOnlyOnce = false;  // 只允许弹跳一次
	public bool relativeDirection = true;
	[Range(0, 360)]
	public float direction = 0f;
	public Vector2 ParabolaSimulatorForce = new Vector2(3, 10);

	BoatScript boat;
	Vector2 srcFlyDirection = Vector2.zero;

	// Use this for initialization
	public void Start () {
		boat = GameObject.Find ("Player").GetComponent<BoatScript> ();
		Collider2D collider = GetComponent<Collider2D> ();
		if (collider != null) {
			collider.isTrigger = true;
		}
		BroadcastSystem.defaultBoardcast.AddListener (BoatScript.BoatEndFlyEvent, new BroadcastSystem.Callback (BoatEndFlying));
	}

	void BoatEndFlying(BroadcastInfo arg) {
		BoatScript boat = arg.from as BoatScript;
		Collider2D collider = GetComponent<Collider2D> ();

		if (collider.IsTouching (boat.LandCollider)) {
			if (hitOnlyOnce && srcFlyDirection != Vector2.zero) { // 只允许弹跳一次
				return;
			}
			if (srcFlyDirection == Vector2.zero) {
				srcFlyDirection = boat.flyingDirection;
			}
			BounceBoat ();
		}
	}

	public virtual void BounceBoat() {
		Vector2 d = Vector2.zero;
		if (relativeDirection) { //相对旋转
			d = srcFlyDirection;
			Vector3 d3 = new Vector3 (d.x, d.y, 0);
			d3 = Quaternion.AngleAxis (direction, Vector3.forward) * d3;
			d = new Vector2 (d3.x, d3.y).normalized;
		} else { //绝对弹跳角度
			Vector3 d3 = Quaternion.AngleAxis (direction, Vector3.forward) * new Vector3(1,0,0);
			d = new Vector2 (d3.x, d3.y).normalized;
		}
		boat.FlyToDirection (d, ParabolaSimulatorForce);
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject.tag == "Player") {
			srcFlyDirection = Vector2.zero;
		}
	}
}
