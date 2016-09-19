using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using DG.Tweening;

public enum BoatStatus {
	Normal = 0,
	Flying = 1,
	Reseting = 2,
};

public class BoatScript : MonoBehaviour {
	public static readonly string BoatBeginFlyEvent = "BoatBeginFlyEvent";
	public static readonly string BoatEndFlyEvent = "BoatEndFlyEvent";

	[HideInInspector]
	public BoatStatus Status = BoatStatus.Normal;
	public float ParabolaAnimationSpeed = 1f;
	public Vector2 ParabolaSimulateForce = new Vector3(3, 10);
	public GameObject HamsterSprite;
	public GameObject ShadowSprite;
	public GameObject WaterSpotSprite;
	public GameObject InterferenceRippleSprite;

	public float HamsterSnoreMinInterval = 10;
	public float HamsterSnoreMaxInterval = 20;
	public bool flyable {
		get {
			if (Status != BoatStatus.Normal) {
				return false;
			} else if (magicCircle.collectFish >= 3) {
				return true;
			} else if (touchingMagicArea != null){
				MagicAreaScript ma = touchingMagicArea.GetComponent<MagicAreaScript> ();
				if (ma != null) {
					return ma.flyIntoTouched;
				}
				return false;
			}
			return false;
		}
	}

	Animator HamsterAnim;
	Animator ShadowAnim;
	Rigidbody2D Rigidbody2D;
	Collider2D touchingMagicArea;

	MagicCircle magicCircle;
	FlyingSimulator FlyingSim;
	HamsterScript hamsterScript;

	BoatStatus m_AnimationStatus = BoatStatus.Normal;
	float m_ParabolaAnimationSpeed = 0f;
	Vector2 m_ParabolaForce = Vector2.zero;
	bool isOnReset;

	int layerMaskBank;
	int layerMaskStone;
	List<int> triggeredRipples = new List<int>();

	[HideInInspector]
	public Collider2D LandCollider;
	[HideInInspector]
	public Vector2 flyingDirection = Vector2.zero;
	[HideInInspector]
	public Vector3 flyingSrcPos = Vector3.zero;
	[HideInInspector]
	public float flyingAltitude = 0;

	void Start() {
		LandCollider = this.GetComponent<Collider2D> ();
		Rigidbody2D = this.GetComponent<Rigidbody2D> ();
		magicCircle = GetComponentInChildren<MagicCircle> ();
		hamsterScript = GetComponentInChildren<HamsterScript> ();
		HamsterSnoreMaxInterval = Mathf.Max (HamsterSnoreMinInterval, HamsterSnoreMaxInterval);

		HamsterAnim = HamsterSprite.GetComponent<Animator> ();
		ShadowAnim = ShadowSprite.GetComponent<Animator> ();
		layerMaskBank = LayerMask.GetMask ("Bank");
		layerMaskStone = LayerMask.GetMask ("Stone");

		BroadcastSystem.defaultBoardcast.AddListener (RippleScript.RippleDestroyEvent, new BroadcastSystem.Callback (OnRippleDestroyed));

		// 启动随机打呼噜
		if (HamsterSnoreMinInterval > 0) {
			StartCoroutine ("RandomSnore");
		}
	}

	void OnDestroy() {
		StopCoroutine ("RandomSnore");
	}

	void FixedUpdate() {
		// 动画飞行速度
		if (m_ParabolaAnimationSpeed != ParabolaAnimationSpeed) {
			m_ParabolaAnimationSpeed = ParabolaAnimationSpeed;
			HamsterAnim.SetFloat ("FlySpeed", ParabolaAnimationSpeed);
		}
	}

	void Update(){
		UpdateAnimationIfNeedly ();
		ShadowSprite.transform.eulerAngles = new Vector3 (0, 0, -HamsterSprite.transform.eulerAngles.z);

		// 根据当前速度动态转换
		if (Status == BoatStatus.Normal) {
			bool isIdle = Mathf.Abs (Rigidbody2D.velocity.x) < 0.05f && Mathf.Abs (Rigidbody2D.velocity.y) < 0.05f;
			if (isIdle) {
				Rigidbody2D.velocity = Vector2.zero;
			}

			float speed = Vector2.Distance (Vector2.zero, Rigidbody2D.velocity);
			ShadowAnim.SetFloat ("MoveSpeed", Mathf.Clamp (speed, 0.5f, 1f));
		} else {
			ShadowAnim.SetFloat ("MoveSpeed", 1f);
		}

		if (Status == BoatStatus.Flying) {
			const float maxCameraSize = 12f;
			const float minCameraSize = 7.68f;

			if (CameraSmoothZoom.instance.cameraSize < maxCameraSize && !CameraSmoothZoom.instance.isZoomingIn) {
				float duration = (maxCameraSize - CameraSmoothZoom.instance.cameraSize) / (maxCameraSize - minCameraSize);
				CameraSmoothZoom.instance.ZoomCameraOrthographicSize (maxCameraSize, 0f, duration);
			}

			float angle = Vector2.Angle (Vector2.right, flyingDirection) * (flyingDirection.y < 0 ? -1 : 1);
			float flyingHeight = FlyingSim.gameObject.transform.position.y;
			Vector3 transformedPos = FlyingSim.gameObject.transform.position;
			transformedPos = Quaternion.AngleAxis (90, Vector3.right) * transformedPos;
			transformedPos = Quaternion.AngleAxis (angle, Vector3.forward) * transformedPos;
			transformedPos = Quaternion.AngleAxis (-45, Vector3.right) * transformedPos;
			transformedPos = transformedPos + flyingSrcPos;

			Vector3 paraPos = new Vector3 (transformedPos.x, transformedPos.y, 0);
			Vector3 pos = paraPos + Quaternion.AngleAxis (-45, Vector3.right) * new Vector3 (0, -flyingHeight, 0);
			transform.position = pos;
			HamsterSprite.transform.position = paraPos;
			this.flyingAltitude = flyingHeight;

			// 根据抛物线高度动态改变阴影大小
			float scale = 1f;
			float topmostHeight = FlyingSim.TopmostHeightForParabola(m_ParabolaForce.y);
			if (topmostHeight > 0) {
				scale = (1.0f - Mathf.Clamp01(flyingHeight / FlyingSim.TopmostHeightForParabola(m_ParabolaForce.y))) * 0.2f + 0.8f;
			}
			ShadowSprite.transform.localScale = new Vector3(scale, scale, 1);

			if (!FlyingSim.flying) {
				Status = BoatStatus.Reseting;
				UpdateAnimationIfNeedly ();

				if (CameraSmoothZoom.instance.cameraSize > minCameraSize && !CameraSmoothZoom.instance.isZoomingOut) {
					float duration = (CameraSmoothZoom.instance.cameraSize - minCameraSize) / (maxCameraSize - minCameraSize);
					CameraSmoothZoom.instance.ZoomCameraOrthographicSize (minCameraSize, 0.1f, duration);
				}
			}
		} else if (Status == BoatStatus.Reseting && !isOnReset) {
			// 判断是否在水里
			if (!LandCollider.IsTouchingLayers(layerMaskBank | layerMaskStone)) {
				Status = BoatStatus.Normal;
				Rigidbody2D.velocity = flyingDirection * 1f;
				GenerateInterferenceRipple();
				BroadcastSystem.defaultBoardcast.SendMessage (BoatScript.BoatEndFlyEvent, this, null);
			} else {
				print ("重置位置");
				isOnReset = true;
				ResetBoatPosition ();
			}
		}
	}

	void ResetBoatPosition() {
		RaycastHit2D rayTest = Physics2D.CircleCast (flyingSrcPos, 1.5f, flyingDirection, Mathf.Infinity, layerMaskBank | layerMaskStone);
		Rigidbody2D.velocity = Vector2.zero;
		Vector2 target = rayTest.centroid + new Vector2 (0, -0.5f);
//		Rigidbody2D.transform.position = rayTest.centroid;

		float moveDuration = Vector3.Distance (target, transform.position) / 2f;
		Rigidbody2D.transform.DOMove (target, moveDuration, false).OnComplete (ResetBoatOver);
	}

	void ResetBoatOver() {
		isOnReset = false;
		Status = BoatStatus.Normal;
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (LandCollider.IsTouching (other)) {
			RippleScript ripple = other.GetComponentInParent<RippleScript> ();
			MagicAreaScript magicArea = other.GetComponent<MagicAreaScript>();

			if (ripple != null && Status == BoatStatus.Normal  && !ripple.isInterferenceRipple) {
				int hash = other.gameObject.GetHashCode();
				if (!triggeredRipples.Contains (hash)) {
					triggeredRipples.Add (hash);

					RippleScript script = other.gameObject.GetComponentInParent<RippleScript> ();
					float impact = script.Impact;
					Vector3 force = impact * (transform.position - other.gameObject.transform.position).normalized;
					UpdateVelocityByCombineNewForce (new Vector2 (force.x, force.y));

					hamsterScript.rotationMultiplier = other.transform.position.x > hamsterScript.transform.position.x ? 1 : -1f;
					int shake = Animator.StringToHash ("Shake");
					HamsterAnim.ResetTrigger (shake);
					HamsterAnim.SetTrigger (shake);
				}
			}
			if (magicArea != null) {
				touchingMagicArea = other;
			}
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (touchingMagicArea == other) {
			touchingMagicArea = null;
		}
	}

	void OnRippleDestroyed(BroadcastInfo arg) {
		int hash = (arg.from as MonoBehaviour).gameObject.GetHashCode ();
		triggeredRipples.Remove (hash);
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

	void GenerateInterferenceRipple() {
		GameObject obj = Instantiate (InterferenceRippleSprite, transform.position, transform.rotation) as GameObject;
		RippleScript interferenceRippleScript = obj.GetComponent<RippleScript> ();
		interferenceRippleScript.RippleScale = 1.5f;
		interferenceRippleScript.isInterferenceRipple = true;
	}

	void UpdateAnimationIfNeedly() {
		if (m_AnimationStatus != Status) {
			//print ("--> " + Status);
			LandCollider.enabled = true;
			LandCollider.isTrigger = Status == BoatStatus.Reseting || Status == BoatStatus.Flying;

			int FlyActionHash = Animator.StringToHash("Fly");
			int MoveActionHash = Animator.StringToHash("Move");
			int IdleActionHash = Animator.StringToHash("Idle");

			HamsterAnim.ResetTrigger (IdleActionHash);
			HamsterAnim.ResetTrigger (FlyActionHash);
			ShadowAnim.ResetTrigger (MoveActionHash);
			ShadowAnim.ResetTrigger (FlyActionHash);

			switch (Status) {
			case BoatStatus.Normal:
				HamsterAnim.SetTrigger (IdleActionHash);
				ShadowAnim.SetTrigger (MoveActionHash);
				break;
			case BoatStatus.Reseting:
				HamsterAnim.SetTrigger (IdleActionHash);
				ShadowAnim.SetTrigger (MoveActionHash);
				break;
			case BoatStatus.Flying:
				HamsterAnim.SetTrigger (FlyActionHash);
				ShadowAnim.SetTrigger (FlyActionHash);
				break;
			}

			// 升起下落时泛起水花
			int StatusHash = Animator.StringToHash("Status");
			if (Status == BoatStatus.Flying && (m_AnimationStatus == BoatStatus.Normal)) {
				GameObject gameObj = Instantiate (WaterSpotSprite, transform.position, transform.rotation) as GameObject;
				Animator WaterSpotAnim = gameObj.GetComponent<Animator> ();
				WaterSpotAnim.SetInteger (StatusHash, 1);
			} else if (m_AnimationStatus == BoatStatus.Flying && Status == BoatStatus.Reseting) {
				GameObject gameObj = Instantiate (WaterSpotSprite, transform.position, transform.rotation) as GameObject;
				Animator WaterSpotAnim = gameObj.GetComponent<Animator> ();
				WaterSpotAnim.SetInteger (StatusHash, 2);
				ShadowSprite.transform.localScale = new Vector3 (1, 1, 1);
			}

			m_AnimationStatus = Status;
		}
	}

	IEnumerator RandomSnore() {
		while (true) {
			yield return new WaitForSeconds(Random.Range(HamsterSnoreMinInterval, HamsterSnoreMaxInterval));
			if (Status == BoatStatus.Normal) {
				HamsterAnim.SetTrigger (Animator.StringToHash("Snore"));
			}
		}
	}

	public void FlyToDirection(Vector2 direction) {
		FlyToDirection (direction, this.ParabolaSimulateForce);
	}

	public void FlyToDirection(Vector2 direction, Vector2 parabolaForce) {
		if (parabolaForce == Vector2.zero)
			return;

		m_ParabolaForce = parabolaForce;

		Rigidbody2D.velocity = Vector2.zero;
		Status = BoatStatus.Flying;
		flyingDirection = direction.normalized;
		flyingSrcPos = transform.position;

		GetComponent<Collider2D> ().enabled = false;
		FlyingSim = GameObject.Find ("FlySimulator").GetComponent<FlyingSimulator>();
		FlyingSim.SimulateParabola (m_ParabolaForce);

		// 生成干扰波
		GenerateInterferenceRipple();
		BroadcastSystem.defaultBoardcast.SendMessage (BoatScript.BoatBeginFlyEvent, this, null);
	}

	public void AnnoyingAnimation() {
		if (Status == BoatStatus.Normal) {
			HamsterAnim.SetTrigger (Animator.StringToHash("Annoying"));
		}
	}
}
