using UnityEngine;
using System.Collections;

public enum BoatStatus {
	Idle = 0,
	ReadyToFly = 1,
	Flying = 2,
};

public class BoatScript : MonoBehaviour {
	public BoatStatus Status = BoatStatus.Idle;
	public float JoyfulPoint = 0;
	public Vector3 ParabolaSimulateForce = new Vector3(3, 10, 0);
	public GameObject ShadowSprite;

	BoatStatus m_Status = BoatStatus.Idle;
	Rigidbody2D Rigidbody2D;
	Collider2D PlayerCollider2D;

	FlyingSimulator FlyingSim;
	Vector2 FlyingDirection = Vector2.zero;
	Vector3 FlyingSrcPos = Vector3.zero;
	Vector3 ShadowSpriteOffset;
	Animator BoatAnim;
	Animator ShadowAnim;
	int PlayerLayer;

	void Start() {
		Rigidbody2D = GetComponent<Rigidbody2D> ();
		BoatAnim = GetComponent<Animator> ();
		PlayerCollider2D = GetComponent<Collider2D> ();
		PlayerLayer = LayerMask.NameToLayer ("Player");

		if (ShadowSprite) {
			ShadowSpriteOffset = ShadowSprite.transform.position - this.transform.position;
			ShadowAnim = ShadowSprite.GetComponent<Animator> ();
		}
	}

	void Update(){
		UpdateStatusIfNeedly (Status);

		// Anim.SetBool (FlyActionHash, Input.GetKey (KeyCode.Z));
		// Anim.SetBool (ShakeActionHash, Input.GetKey (KeyCode.X));

		if (Status == BoatStatus.Flying) {
			if (Camera.main.orthographicSize <= 7.68f) {
				CameraSmoothZoom zoomer = Camera.main.GetComponent<CameraSmoothZoom> ();
				zoomer.ZoomCameraOrthographicSize (12f, 0f, 1f);
			}

			float angle = Vector2.Angle(Vector2.right, FlyingDirection) * (FlyingDirection.y < 0 ? -1 : 1);
			float flyingHeight = FlyingSim.gameObject.transform.position.y;
			Vector3 transformedPos = FlyingSim.gameObject.transform.position;
			transformedPos = Quaternion.AngleAxis (90, Vector3.right) * transformedPos;
			transformedPos = Quaternion.AngleAxis (angle, Vector3.forward)  * transformedPos;
			transformedPos = Quaternion.AngleAxis (-45, Vector3.right) * transformedPos;
			transformedPos = transformedPos + FlyingSrcPos;

			transform.position = new Vector3(transformedPos.x, transformedPos.y, transform.position.z);
			ShadowSprite.transform.localPosition = ShadowSpriteOffset +  Quaternion.AngleAxis (-45, Vector3.right) * new Vector3(0, -flyingHeight, 0);

			if (!FlyingSim.flying) {
				Status = BoatStatus.Idle;

				if (Camera.main.orthographicSize > 7.68f) {
					CameraSmoothZoom zoomer = Camera.main.GetComponent<CameraSmoothZoom> ();
					zoomer.ZoomCameraOrthographicSize (7.68f, 0.1f, 1f);
				}
			}
		}
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

	void UpdateStatusIfNeedly(BoatStatus status) {
		if (m_Status != status) {
			m_Status = status;

			Collider2D[] colliders = GetComponentsInChildren<Collider2D> ();
			foreach (Collider2D c in colliders) {
				c.enabled = m_Status != BoatStatus.Flying;
				print ("enable " + m_Status);
			}

			int FlyActionHash = Animator.StringToHash("Fly");

			if (ShadowAnim) {
				ShadowAnim.SetBool (FlyActionHash, m_Status == BoatStatus.Flying);
			}
			BoatAnim.SetBool (FlyActionHash, m_Status == BoatStatus.Flying);
		}
	}

	public void FlyToDirection(Vector2 direction) {
		Rigidbody2D.velocity = Vector2.zero;
		Status = BoatStatus.Flying;
		FlyingDirection = direction;
		FlyingSrcPos = transform.position;

		GetComponent<Collider2D> ().enabled = false;
		FlyingSim = GameObject.Find ("FlySimulator").GetComponent<FlyingSimulator>();
		FlyingSim.SimulateParabola (ParabolaSimulateForce);
	}
}
