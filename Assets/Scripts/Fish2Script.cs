using UnityEngine;
using System.Collections;
using DG.Tweening;

public enum MovementMode {
	MovingAround = 0,
	Following = 1,
};

public enum SpeedMode {
	Idle = 0,
	MediumSpeed = 1,
	HighSpeed = 2,
};

public class Fish2Script : MonoBehaviour {
	/// <summary>
	/// 高速移动速度
	/// </summary>
	public float HighSpeed = 2f;

	/// <summary>
	/// 中速移动速度
	/// </summary>
	public float MediumSpeed = 1f;

	/// <summary>
	/// 移动速度模式改变时间间隔
	/// </summary>
	public float SpeedModeChangeInterval = 10f;

	/// <summary>
	/// 移动方向改变时间间隔
	/// </summary>
	public float DirectionChangeInterval = 10f;

	/// <summary>
	/// 移动速度模式
	/// </summary>
	SpeedMode SpeedMode;

	/// <summary>
	/// 跟随状态
	/// </summary>
	MovementMode MovementMode;

	/// <summary>
	/// 获取当前移动速度
	/// </summary>
	/// <value>The current speed.</value>
	float CurrentSpeed {
		get {
			switch (SpeedMode) {
			case SpeedMode.Idle:
				return 0;
			case SpeedMode.MediumSpeed:
				return MediumSpeed;
			case SpeedMode.HighSpeed:
				return HighSpeed;
			}
			return 0;
		}
	}

	/// <summary>
	/// 正在动画过渡中
	/// </summary>
	bool IsInTransition = false;

	Rigidbody2D RigidFish;
	GameObject Boat;
	Animator Anim;

	void InitData(){
		RigidFish = gameObject.GetComponent<Rigidbody2D> ();
		Boat = GameObject.FindGameObjectWithTag (Constant.TagPlayer);
		SpeedMode = SpeedMode.MediumSpeed;
		MovementMode = MovementMode.MovingAround;
		Anim = gameObject.GetComponent<Animator> ();
		Anim.SetFloat ("speed", 1);
	}

	void Start () {
		InitData ();
	}

	void Update () {
		NextStep ();

//		if (Physics2D.CircleCast (transform.position, 1.5f,transform.up,0.5f,(1<<LayerMask.NameToLayer("Stone")|1<<LayerMask.NameToLayer("Bank")))) {
//			GameObject hit = Physics2D.CircleCast (transform.position, 1.5f,transform.up,0.5f,(1<<LayerMask.NameToLayer("Stone")|1<<LayerMask.NameToLayer("Bank"))).collider.gameObject;
//			Debug.Log (hit.layer);
//			rigidFish.velocity = Vector3.zero;
//			CancelInvoke ();
//			iTween.Stop (gameObject);
//			DOTween.Kill (gameObject);
//			if (enterCircle) {
//				hasChangeDirection = true;
//
//				RandomMoveInCircle ();
//			} 
//			else {
//				hasChangeDirection = true;
//
//				ChangeDirection ();
//			}
//		}
//
//		if (enterCircle) {
//			distance = Vector3.Distance (transform.position, boat.transform.position);
////			outline.enabled = true;
//			//Light (2.0f);
//		}
//
//		if(exitTime >= 3 && exitCircle){
//			enterCircle = false;
//			iTween.Stop (gameObject);
//			rigidFish.velocity = Vector3.zero;
//			DOTween.Kill(gameObject);
//			CancelInvoke ("ChangeDirection");
//			ChangeDirection ();
//			exitTime = 0;
//		}
	}

	void StepDone() {
		IsInTransition = false;
		NextStep ();
	}

	void NextStep() {
		if (IsInTransition)
			return;
		
		switch (MovementMode) {
		case MovementMode.MovingAround:
			MoveAround ();
			break;
		case MovementMode.Following:
			MoveToTarget (Boat.transform.position);
			break;
		}
	}

	void MoveAround() {
		int loopCount = 0;
		bool loopToFind = true;
		Vector2 availablePos = Vector2.zero;
		while (loopToFind) {
			Vector2 target = transform.position + transform.up * 1f;
			float angle = Random.Range (-45, 45) * Mathf.Deg2Rad;
			angle = -45 * Mathf.Deg2Rad;

			float x = Mathf.Cos(angle) * (target.x - transform.position.x) - Mathf.Sin(angle) * (target.y - transform.position.y);
			float y = Mathf.Cos(angle) * (target.y - transform.position.y) + Mathf.Sin(angle) * (target.x - transform.position.x);
			target = new Vector2 (x + transform.position.x, y + transform.position.y);

			loopCount += 1;
			RaycastHit2D hit = Physics2D.CircleCast (target, 1.5f, transform.up, 0.5f, 1 << LayerMask.NameToLayer ("Stone") | 1 << LayerMask.NameToLayer ("Bank"));
			if (!hit) {
				availablePos = target ;
				loopToFind = false;
			} else if (loopCount > 10) {
				loopToFind = false;
			}
		}

		if (availablePos != Vector2.zero) {
			MoveToTarget (availablePos);
		} else {
			// 没有发现可行进路线，准备掉头
		}
	}

	void MoveToTarget(Vector2 target) {
		if (CurrentSpeed > 0) {
			IsInTransition = true;

			float duration = Vector3.Distance (target, transform.position) / CurrentSpeed;
		
			Vector3 finalAngle = new Vector3 (0, 0, -transform.localEulerAngles.z + Mathf.Rad2Deg * Mathf.Atan((target.y - transform.position.y) / (target.x - transform.position.x)));

			transform.DOMove (target, duration, false).OnComplete (StepDone).SetId (gameObject);
			transform.DORotate (finalAngle, duration);
		}
	}

//
//	public void Move(){
//		hasChangeDirection = false;
//		rigidFish.velocity = transform.up*speed;
//		Invoke ("ChangeDirection", Random.Range (0.8f, 3));
//	}
//
//	void OnTriggerEnter2D(Collider2D col){
//		if (col.gameObject.tag == "MagicCircle" && !enterCircle) {
//			enterCircle = true;
//			Debug.Log (gameObject.name + " Enter");
//			iTween.Stop (gameObject);
//			rigidFish.velocity = Vector3.zero;
//			DOTween.Kill(gameObject);
//			CancelInvoke ("ChangeDirection");
//			//gameObject.GetComponent<SpriteRenderer>().material.color = Color.Lerp(Color.white, Color.red, 2f);
//			//Light(2);
//			RandomMoveInCircle ();
//
//		}
//		if (col.gameObject.tag == "Ripple" && !enterCircle) {
//			Debug.Log ("碰到涟漪了");
//			RippleScript script = col.gameObject.GetComponentInParent<RippleScript> ();
//		} 
//	}
//	void OnTriggerExit2D(Collider2D col){
//		if (col.gameObject.tag == "MagicCircle") {
//			exitCircle = true;
//			exitTime += Time.deltaTime;
//		}
//	}
//
//
//	void RandomMoveInCircle()
//	{
//		
//		anim.SetFloat ("speed",2f);
//		float angle = Random.Range (0, 2 * Mathf.PI);
//		float r = Random.Range (MinRadius, MaxRadius);
//		if(distance > 5.5)
//			nextTarget = boat.transform.position;
//		else
//			nextTarget = new Vector3 (Mathf.Cos (angle), Mathf.Sin (angle), 0) * r + boat.transform.position;
//		//		Debug.DrawLine(transform.position)
//
//		Vector3 horizontalDir = nextTarget - transform.position;
//		float rotateTowardsAngle = GetSignAngle (horizontalDir.normalized, transform.up.normalized);
//		transform.DORotate(new Vector3(0, 0, rotateTowardsAngle + transform.localEulerAngles.z), Mathf.Abs(rotateTowardsAngle)/120).OnComplete(moveToTarget);
//
//
//	}
//
//	private void moveToTarget(){
//		if (distance > 5.5 && outCircle) {
//			nextTarget = boat.transform.position;
//			RandomMoveInCircle ();
//			outCircle = false;
//		} 
//		else {
//			hasChangeDirection = false;
//			transform.DOMove (nextTarget, Vector3.Distance (nextTarget, transform.position) / speed / 3, false).OnComplete (RandomMoveInCircle).SetId (gameObject);
//		}
//	}
//
//	private float GetSignAngle(Vector3 a,Vector3 b)
//	{
//		return -Vector3.Angle (a, b) * Mathf.Sign (Vector3.Cross(a,b).z);
//
//	}


}
