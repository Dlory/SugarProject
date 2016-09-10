//#define DEBUGPATH

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public enum MovementMode {
	MovingAround = 0,
	Following = 1,
};

public enum SpeedMode {
	Slow = 0,
	MediumSpeed = 1,
	HighSpeed = 2,
};
	
public class FishBehaviourScript : MonoBehaviour {
	public float fastestSpeed = 5f;
	[Range(0, 360)]
	public float fishSeekTurnDegree = 100f;
	public float rippleEffectInterval = 5f;
	public float randomMoveAboutRadius = 20f;
	public float exitMagicCircleFreeSecond = 5f;
	public float interferenceRippleLastsSecond = 5f;

	public Animator fishAnimator;
	public Animator fishLightAnimator;

	public bool isFlllowInMagicCircle {
		get {
			return movementMode == MovementMode.Following && exitCircleElapse == -1;
		}
	}

	MovementMode movementMode {
		set {
			if (m_movementMode != value) {
				m_movementMode = value;
				if (m_movementMode == MovementMode.Following) {
					fishLight.FadeIn ();
					magicCircle.EnterFish (this);
				} else {
					fishLight.FadeOut ();
					magicCircle.ExitFish (this);
				}
			}
		}
		get {
			return m_movementMode;
		}
	}
	SpeedMode speedMode;
	float currentSpeed = 0f;
	Vector3 moveAboutCenter;
	bool isInTransition = false;
	List<int> triggeredRipples = new List<int>();

	float rippleEffectElapse = -1;
	float exitCircleElapse = -1;
	float followBoatDirectionChangeElapse = -1;
	float interferenceRippleEffectElapse = -1;
	float moveAroundSpeedChangeElapse = 0;

	MovementMode m_movementMode;
	GameObject boat;
	MagicCircle magicCircle;
	FishLight fishLight;

	#if DEBUGPATH
	LineRenderer lineRenderer;
	List<Vector3> debugPoints = new List<Vector3>();
	#endif

	void Start () {
		boat = GameObject.FindGameObjectWithTag ("Player");
		magicCircle = boat.GetComponentInChildren<MagicCircle> ();
		fishLight = GetComponentInChildren<FishLight> ();

		moveAboutCenter = this.transform.position;
		movementMode = MovementMode.MovingAround;
		speedMode = SpeedMode.MediumSpeed;
		UpdateCurrentSpeed ();

		BroadcastSystem.defaultBoardcast.AddListener (RippleScript.RippleDestroyEvent, new BroadcastSystem.Callback (OnRippleDestroyed));

		#if DEBUGPATH
		lineRenderer = gameObject.AddComponent<LineRenderer> ();
		lineRenderer.SetWidth(0.1f, 0.1f);
		lineRenderer.SetColors (Color.red, Color.red);
		debugPoints.Add (new Vector3(transform.position.x, transform.position.y, -1));
		#endif
	}

	void FixedUpdate() {
		float deltaTime = Time.fixedDeltaTime;
		if (exitCircleElapse >= 0) {
			exitCircleElapse += deltaTime;
		}
		if (rippleEffectElapse >= 0) {
			rippleEffectElapse += deltaTime;
		}
		if (interferenceRippleEffectElapse >= 0) {
			interferenceRippleEffectElapse += deltaTime;
			if (interferenceRippleEffectElapse >= interferenceRippleLastsSecond) {
				interferenceRippleEffectElapse = -1;
			}
		}
		if (followBoatDirectionChangeElapse >= 0 && movementMode == MovementMode.Following) {
			followBoatDirectionChangeElapse += deltaTime;

			//在圈内游动几秒后强制转向，追赶状态则1秒瞄准一次圈内
			float changeDirectionInterval = exitCircleElapse > 0 ? 1f : Random.Range (3, 5); 
			if (followBoatDirectionChangeElapse > changeDirectionInterval) {
				isInTransition = false; 
			}
		}
		moveAroundSpeedChangeElapse += deltaTime;
	}

	void Update () {
		NextStep ();
	}

	void StepDone() {
		isInTransition = false;
	}

	void NextStep() {
		if (isInTransition) return;

		if (movementMode == MovementMode.MovingAround) {  //随意游动状态
			if (moveAroundSpeedChangeElapse > Random.Range (5, 10) && (rippleEffectElapse < 0 || rippleEffectElapse > rippleEffectInterval)) {
				rippleEffectElapse = -1;
				followBoatDirectionChangeElapse = -1;
				moveAroundSpeedChangeElapse = 0;
				RandomSpeedImmediately ();
			}
			MoveAround ();
		} else if (exitCircleElapse > exitMagicCircleFreeSecond) { //出圈超时
			exitCircleElapse = -1;
			followBoatDirectionChangeElapse = -1;
			movementMode = MovementMode.MovingAround;
			RandomSpeedImmediately ();
			moveAboutCenter = this.transform.position;
		} else {  //进圈跟随状态
			rippleEffectElapse = -1;

			bool findway = MoveTowardMagicCircle ();
			if (!findway) {
				exitCircleElapse = -1;
				followBoatDirectionChangeElapse = -1;
				movementMode = MovementMode.MovingAround;
				RandomSpeedImmediately ();
			} else if (exitCircleElapse == -1) {  // 在圈内
				followBoatDirectionChangeElapse = 0;
				this.speedMode = SpeedModeFromBoatSpeed() ;  //在圈内时候速度跟船有关
				UpdateCurrentSpeed ();
			} else { // 在圈外
				followBoatDirectionChangeElapse = 0;
				if (this.speedMode != SpeedMode.HighSpeed) {
					this.speedMode =  SpeedMode.HighSpeed;  //在圈外追赶则为高速
					UpdateCurrentSpeed ();
				}
			}
		}
	}

	void MoveAround() {
		MoveToAngle (transform.rotation.eulerAngles.z, this.fishSeekTurnDegree, 1f, 5f);
	}

	void Disperse() {
		movementMode = MovementMode.MovingAround;
		speedMode = SpeedMode.HighSpeed;
		UpdateCurrentSpeed ();
		MoveToAngle (transform.rotation.eulerAngles.z + 180f + Random.Range(-30, 30), this.fishSeekTurnDegree, 3f, 5f);
	}

	bool MoveTowardMagicCircle() {
		float radius = magicCircle.Radius() + 0.1f;
		Vector2 reachPos = Vector2.zero;
		bool loop = true;
		int loopCount = 0;

		while (loop) {
			loopCount++;
			Vector2 target = boat.transform.position + new Vector3 (Random.Range (-radius, radius), Random.Range (-radius*0.75f, radius*0.75f), 0);
			RaycastHit2D[] hits = Physics2D.RaycastAll (target, boat.transform.position);
			foreach (RaycastHit2D hit in hits) {
				if (hit.collider.GetComponent<MagicCircle>() != null) {
					reachPos = target;
					break;
				}
			}

			if (loopCount > 20) {
				loop = false;
			}
		}
		if (reachPos != Vector2.zero) {
			MoveToTarget (reachPos);
			return true;
		}
		return false;
	}

	bool MoveToAngle(float angle, float seekTurnDegree, float minDistance, float maxDistance)
	{
		bool loop = true;
		int loopCount = 0;
		Vector2 availablePos = Vector2.zero;
		float circleCastRadius = 0.5f * Mathf.Min(transform.localScale.x, transform.localScale.y);
		float seekAngle = angle + Random.Range (- seekTurnDegree/2f, seekTurnDegree/2f);
		int thisSeekRotate = (Random.Range (0, 1) == 0 ? 1 : -1);

		while (loop) {
			float distance = Random.Range (minDistance, maxDistance);
			Vector3 direction = Quaternion.AngleAxis (seekAngle, Vector3.forward) * Vector3.right;
			Vector3 newPos = direction * distance + transform.position;
			Vector2 target = new Vector2 (newPos.x, newPos.y);

			loopCount += 1;
			RaycastHit2D hit = Physics2D.CircleCast (transform.position, circleCastRadius, newPos - transform.position, distance, 1 << LayerMask.NameToLayer ("Stone") | 1 << LayerMask.NameToLayer ("Bank"));
			if (hit.collider == null) {
				availablePos = target;
				if (randomMoveAboutRadius < 0 || Vector2.Distance (moveAboutCenter, target) <= randomMoveAboutRadius) {
					loop = false;
					break;
				} else {
					seekAngle += Random.Range (5, 90) * thisSeekRotate;
				}
			} else if (minDistance == maxDistance) {
				loop = false;
			} else if (loopCount > 40) {
				loop = false;
			} else if (loopCount > 10) {
				seekAngle += Random.Range(10, 60) * thisSeekRotate;
			}
		}

		if (availablePos != Vector2.zero) {
			MoveToTarget (availablePos);
			return true;
		} else {
			// 没有发现可行进路线
			return false;
		}
	}

	void MoveToTarget(Vector2 target) {
		if (currentSpeed > 0) {
			isInTransition = true;
		
			DOTween.Kill (transform, false);

			Vector2 toward = target - new Vector2(transform.position.x, transform.position.y);
			float angle = Vector2.Angle (Vector2.right, toward) * (toward.y < 0 ? -1 : 1);

			float moveDuration = Vector3.Distance (target, transform.position) / currentSpeed;
			float turnDuration = Mathf.Clamp(Mathf.Abs(angle) / 360f, 0.15f, 0.5f);
			Vector3 rotation = transform.localEulerAngles; 
			rotation.z = angle;

			// TODO. bezier curves movement
			transform.DOMove (target, moveDuration, false).OnComplete (StepDone).SetId (gameObject);
			transform.DORotate (rotation, turnDuration, RotateMode.Fast);

			#if DEBUGPATH
			debugPoints.RemoveAt(debugPoints.Count - 1);
			debugPoints.Add(new Vector3(transform.position.x, transform.position.y, -1));
			debugPoints.Add (new Vector3(target.x, target.y, -1));
			lineRenderer.SetVertexCount (debugPoints.Count);
			lineRenderer.SetPositions (debugPoints.ToArray ());
			#endif
		}
	}

	void UpdateCurrentSpeed() {
		float speed = 0;
		switch (speedMode) {
		case SpeedMode.Slow:
			speed = Random.Range (0.25f, fastestSpeed / 3f);
			break;
		case SpeedMode.MediumSpeed:
			speed = Random.Range (fastestSpeed / 3f, fastestSpeed / 3f * 2f);
			break;
		case SpeedMode.HighSpeed:
			speed = Random.Range (fastestSpeed / 3f * 2f, fastestSpeed);
			break;
		}

		float speedScale = Mathf.Clamp(speed / fastestSpeed * 1.22f, 0.5f, 1.4f);
		fishAnimator.SetFloat ("speed", speedScale);
		fishLightAnimator.SetFloat ("speed", speedScale);

		currentSpeed = speed;
	}

	SpeedMode SpeedModeFromBoatSpeed() {
		Rigidbody2D rigidBoat = boat.GetComponent<Rigidbody2D> ();
		float maxSpeed = GameObject.Find ("GameGUI").GetComponent<GameControl> ().Impact;
		float speed = Vector2.Distance (rigidBoat.velocity, Vector2.zero);
		float percent = speed / maxSpeed;
		if (percent < 0.5f) {
			return SpeedMode.MediumSpeed;
		} else {
			return SpeedMode.HighSpeed;
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (interferenceRippleEffectElapse >= 0 && interferenceRippleEffectElapse < interferenceRippleLastsSecond) {
			return;
		}

		if (other.GetComponentInParent<RippleScript>() != null) {
			int hash = other.gameObject.GetHashCode();
			if (!triggeredRipples.Contains (hash)) {
				triggeredRipples.Add (hash);
				RippleScript ripple = other.GetComponentInParent<RippleScript> ();

				if (ripple.isInterferenceRipple == true) { // 干扰波
					interferenceRippleEffectElapse = 0;
					rippleEffectElapse = 0;
					movementMode = MovementMode.MovingAround;
					speedMode = SpeedMode.HighSpeed;
					UpdateCurrentSpeed ();
					MoveToAngle (transform.rotation.eulerAngles.z + 180, this.fishSeekTurnDegree, 3f, 5f);

				} else if (movementMode == MovementMode.MovingAround) { // 正常水波
					rippleEffectElapse = 0;
					Vector2 diff = transform.position - other.transform.position;
					float angle = Vector2.Angle (Vector2.right, diff) * (diff.y < 0 ? -1 : 1);
					speedMode = SpeedMode.HighSpeed;
					UpdateCurrentSpeed ();
					MoveToAngle (angle, 50f, 3f, 5f);
				}
			}
		} else if (other.GetComponent<MagicCircle>() != null) {
			exitCircleElapse = -1;
			movementMode = MovementMode.Following;
			speedMode = SpeedModeFromBoatSpeed();
			UpdateCurrentSpeed ();
			isInTransition = false;
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		if (other.GetComponent<MagicCircle>() != null) {
			exitCircleElapse = 0f;
			speedMode = SpeedMode.HighSpeed;
			UpdateCurrentSpeed ();
		}
	}

	void OnRippleDestroyed(BroadcastInfo arg) {
		int hash = (arg.from as MonoBehaviour).gameObject.GetHashCode ();
		triggeredRipples.Remove (hash);
	}

	void RandomSpeedImmediately() {
		this.speedMode = (SpeedMode)Random.Range (0, 2);
		UpdateCurrentSpeed ();
	}
}
