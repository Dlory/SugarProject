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
					followingElapse = 0f;
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
	Collider2D fishCollider;

	float circleCastRadius;
	float rippleEffectElapse = -1;
	float exitCircleElapse = -1;
	float followBoatDirectionChangeElapse = -1;
	float interferenceRippleEffectElapse = -1;
	float moveAroundSpeedChangeElapse = 0;
	float followingElapse = 0;

	MovementMode m_movementMode;
	GameObject boat;
	MagicCircle magicCircle;
	FishLight fishLight;
	int barrierLayerMask;
	int barrierLayer;

	#if DEBUGPATH
	LineRenderer lineRenderer;
	List<Vector3> debugPoints = new List<Vector3>();
	#endif

	void Start () {
		boat = GameObject.FindGameObjectWithTag ("Player");
		magicCircle = boat.GetComponentInChildren<MagicCircle> ();
		fishLight = GetComponentInChildren<FishLight> ();
		fishCollider = GetComponent<Collider2D> ();
		barrierLayerMask = 1 << LayerMask.NameToLayer ("Barrier") | 1 << LayerMask.NameToLayer ("Bank");
		barrierLayer = LayerMask.NameToLayer ("barrierLayer");
		circleCastRadius = 0.5f * Mathf.Min(transform.localScale.x, transform.localScale.y);

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

	void Update () {
		if (Vector2.Distance (transform.position, boat.transform.position) <= randomMoveAboutRadius) {
			NextStep ();

			float deltaTime = Time.deltaTime;
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

			if (movementMode == MovementMode.Following) {
				followingElapse += deltaTime;

				//在追赶状态每1秒瞄准一次圈内
				if (followBoatDirectionChangeElapse >= 0 && exitCircleElapse > 0) {
					followBoatDirectionChangeElapse += deltaTime;
					if (followBoatDirectionChangeElapse > Random.Range(0.5f, 1f)) {
						followBoatDirectionChangeElapse = 0f;
						isInTransition = false; 
					}
				}
			}
			moveAroundSpeedChangeElapse += deltaTime;
		}
	}

	void StepDone() {
		isInTransition = false;
	}

	void NextStep() {
		if (isInTransition) return;

		if (movementMode == MovementMode.MovingAround) {  //随意游动状态
			if (moveAroundSpeedChangeElapse > Random.Range (5, 10) && (rippleEffectElapse < 0 || rippleEffectElapse > rippleEffectInterval)) { // 过一段时间变速
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
			if (exitCircleElapse > 0 || exitCircleElapse == -1) {
				rippleEffectElapse = -1;
				followBoatDirectionChangeElapse = 0;

				bool findWay = false;
				if (exitCircleElapse > 0) { // 在圈外
					if (this.speedMode != SpeedMode.HighSpeed) {
						this.speedMode =  SpeedMode.HighSpeed;  //在圈外追赶则为高速
						UpdateCurrentSpeed ();
					}
					findWay = MoveTowardMagicCircle (0.5f);
				} else {  // 在圈内
					this.speedMode = SpeedModeFromBoatSpeed() ;  //在圈内时候速度跟船有关
					UpdateCurrentSpeed ();
					findWay = MoveTowardMagicCircle (0.98f);
				}
				if (!findWay && exitCircleElapse > 0) {
					print ("CAN NOT findWay: " + this.name);
					exitCircleElapse = -1;
					followBoatDirectionChangeElapse = -1;
					movementMode = MovementMode.MovingAround;
					RandomSpeedImmediately ();
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

	// angleCoefficient 越小离圆心越近
	bool MoveTowardMagicCircle(float angleCoefficient) {
		float radius = magicCircle.Radius();
		float disntance = Vector2.Distance (transform.position, boat.transform.position);
		float angleRange = Mathf.Atan2 (radius, disntance) * Mathf.Rad2Deg * Mathf.Clamp01(angleCoefficient) * 2f;
		Vector2 toward = boat.transform.position - transform.position;
		float towardAngle = Vector2.Angle (Vector2.right, toward) * (toward.y < 0 ? -1 : 1);

		const int step = 6;
		float stepAngle = angleRange / step;
		int stepCount = 0;
		float minAngle = towardAngle - angleRange/2f;
		float maxAngle = towardAngle + angleRange/2f;

		// 寻找最小角度
		while (stepCount < step) {
			float angle = minAngle + stepCount * stepAngle;
			if (TestAvailableToAngle (angle, disntance)) {
				minAngle = angle;
				stepCount = step;
				break;
			}
			if (stepCount >= step*2-1) {
				return false;
			}
			stepCount ++;
		}

		// 寻找最大角度
		stepCount = 0;
		while (stepCount < step) {
			float angle = maxAngle - stepCount * stepAngle;
			if (TestAvailableToAngle (angle, disntance)) {
				maxAngle = angle;
				stepCount = step;
				break;
			}
			if (stepCount >= step*2-1) {
				return false;
			}
			stepCount ++;
		}
		disntance = Mathf.Max (radius/2f, disntance);
		return MoveToAngle(Random.Range(minAngle, maxAngle), 0, disntance, disntance * 2f);
	} 

	bool MoveToAngle(float angle, float seekTurnDegree, float minDistance, float maxDistance) {
		bool loop = true;
		int loopCount = 0;
		Vector2 availablePos = Vector2.zero;
		float seekAngle = angle + Random.Range (-seekTurnDegree/2f, seekTurnDegree/2f);
		int thisSeekLeftRight = (Random.Range (0, 1) == 0 ? 1 : -1);

		while (loop) {
			float distance = Random.Range (minDistance, maxDistance);
			loopCount ++;

			if (TestAvailableToAngle(seekAngle, distance)) {
				Vector3 direction = Quaternion.AngleAxis (seekAngle, Vector3.forward) * Vector3.right;
				Vector3 newPos = direction * distance + transform.position;
				Vector2 target = new Vector2 (newPos.x, newPos.y);
				availablePos = target;
				if (randomMoveAboutRadius < 0 || Vector2.Distance (moveAboutCenter, target) <= randomMoveAboutRadius) {
					loop = false;
				} else {
					seekAngle += Random.Range (5, 90) * thisSeekLeftRight;
				}
			} else if (loopCount > 5) {
				seekAngle += Random.Range(10, 60) * thisSeekLeftRight;
			}
			if (seekTurnDegree == 0) {
				seekAngle = angle;
				if (minDistance == maxDistance) {
					loop = false;
				}
			}
			if (loopCount > 20) {
				loop = false;
			} 
		}
		if (availablePos != Vector2.zero) {
			MoveToTarget (availablePos);
			return true;
		}
		// 没有发现可行进路线
		return false;
	}

	bool TestAvailableToAngle(float angle, float distance) {
		Vector3 direction = Quaternion.AngleAxis (angle, Vector3.forward) * Vector3.right;
//		RaycastHit2D hit = Physics2D.CircleCast (transform.position, circleCastRadius, direction, distance, barrierLayerMask);
		RaycastHit2D hit = Physics2D.Raycast (transform.position, direction, distance + circleCastRadius, barrierLayerMask);
		if (hit.collider == null) {
			return true;
		}
		return false;
	}

	void MoveToTarget(Vector2 target) {
		if (currentSpeed > 0) {
			isInTransition = true;
			DOTween.Kill (transform, false);

			Vector2 toward = target - new Vector2(transform.position.x, transform.position.y);
			float angle = Vector2.Angle (Vector2.right, toward) * (toward.y < 0 ? -1 : 1);

			float moveDuration = Vector3.Distance (target, transform.position) / currentSpeed;
			float turnDuration = Mathf.Clamp(Mathf.Abs(angle) / 500f, 0.15f, 0.3f);
			Vector3 rotation = transform.localEulerAngles; 
			rotation.z = angle;

			int turnLeft = Animator.StringToHash ("turnLeft");
			int turnRight = Animator.StringToHash ("turnRight");

			// 旋转角度绝对值大于30°播放旋转动画
			if (Mathf.Abs(AngleBetweenTwoAngles(transform.localEulerAngles.z, angle)) >= 30f) {
				//fishAnimator.ResetTrigger (turnLeft);
				//fishAnimator.ResetTrigger (turnRight);
				fishAnimator.SetTrigger (AngleBetweenTwoAngles(transform.localEulerAngles.z, angle) > 0 ? turnLeft : turnRight);
			}

			// TODO. bezier curves movement
			transform.DOMove (target, moveDuration, false).OnComplete (StepDone);
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

	static float AngleBetweenTwoAngles(float from, float to) {
		float x = to - from;
		if (x>=180) {
			return x-360;
		}
		if (x<-180) {
			return x+360;
		}
		return x;
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
		float boatSpeed = Vector2.Distance (rigidBoat.velocity, Vector2.zero);
		if (boatSpeed > fastestSpeed/3f*2f) {
			return SpeedMode.HighSpeed;
		} else {
			return SpeedMode.MediumSpeed;
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		// 受到干扰波后一段时间不响应碰撞事件
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
			if (movementMode == MovementMode.MovingAround) {
				movementMode = MovementMode.Following;
				isInTransition = false;
			}
			exitCircleElapse = -1;
		}
	}

	void OnTriggerExit2D(Collider2D other) {
		// 受到干扰波后一段时间不响应碰撞事件
		if (interferenceRippleEffectElapse >= 0 && interferenceRippleEffectElapse < interferenceRippleLastsSecond) {
			return;
		}

		if (other.GetComponent<MagicCircle>() != null && followingElapse > 0.5f) {
			exitCircleElapse = 0f;
			followingElapse = 0f;
			movementMode = MovementMode.Following;
			this.speedMode =  SpeedMode.HighSpeed;
			UpdateCurrentSpeed ();
			isInTransition = false;
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
