using UnityEngine;
using System.Collections;
using DG.Tweening;



public class NewFishScript : MonoBehaviour {
	// Use this for initialization
	Rigidbody2D rigidFish;
	float speed;
	public float MaxRadius;
	public float MinRadius;

	public GameObject boat;
	bool enterCircle;
	bool hasChangeDirection;
	bool outCircle;
	float distance;
	Vector3 nextTarget;
	public float forceScale;
	public float minForce;
	public float maxForce;
	bool exitCircle;
	Animator anim;
	float exitTime;
	float animSpeed;

	float time;
	public float TargetValue;

	void Start () {
		InitData ();
		//ChangeDirection ();
		RandomMove ();
	}

	// Update is called once per frame
	void Update () {
		rigidFish.velocity = transform.up * speed;
		anim.SetFloat ("speed",animSpeed);
//		if (Physics2D.CircleCast (transform.position, 1.5f,transform.up,0.5f,(1<<LayerMask.NameToLayer("Stone")|1<<LayerMask.NameToLayer("Bank")))  && !hasChangeDirection) {
//
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
//
//			}
//		}
//
//		if (enterCircle) {
//			distance = Vector3.Distance (transform.position, boat.transform.position);
//			//			outline.enabled = true;
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

	public void InitData(){
		rigidFish = gameObject.GetComponent<Rigidbody2D> ();
		speed = 1;
		animSpeed = 1;
		hasChangeDirection = false;
		enterCircle = false;
		outCircle = true;
		distance = 0;
		anim = gameObject.GetComponent<Animator> ();
		anim.SetFloat ("speed",1);
		exitCircle = false;
		exitTime = 0;
		time = 0;
	}

	public void RandomMove(){
		float duration = Random.Range (0.8f, 2);
		float speed1 = Random.Range (0.5f, 4);
		float speed2 = Random.Range (0.5f, 4);
		int type = Random.Range (1, 4);
		switch (type) {
		case 1:
			if (speed1 < speed2) {
				iTween.ValueTo (gameObject, iTween.Hash ("from", speed1, "to", speed2, "time", duration, "easetype", iTween.EaseType.easeInCubic, "oncomplete", "RandomMove", "oncompletetarget", gameObject));
			}
			else
				iTween.ValueTo (gameObject, iTween.Hash ("from", speed2, "to", speed1,"time",duration,"easetype",iTween.EaseType.easeInOutCubic,"oncomplete","RandomMove","oncompletetarget",gameObject));
			break;
		case 2:
			speed = speed1;
			animSpeed = 1 / speed;
			Invoke ("RandomMove", duration);
			break;
		case 3:
			speed = 0;
			animSpeed = 1;
			Invoke ("RandomMove", duration);
			break;
		}
	}

	public void ChangeDirection(){
		Vector3 nowRotation = transform.rotation.eulerAngles;
		Vector3 randomAngle = new Vector3 (0, 0, nowRotation.z + Random.Range(-180,180));
		iTween.RotateTo (gameObject, iTween.Hash ("rotation", randomAngle, "time", Mathf.Abs (nowRotation.z - randomAngle.z) / 60, "easeType", iTween.EaseType.linear, "oncomplete", "ChangeDirection", "oncompletetarget", gameObject));
	}

//	public void ChangeDirection(){
//		Vector3 nowRotation = transform.rotation.eulerAngles;
//		Vector3 turnBackAngle;
//		if (hasChangeDirection) {
//			turnBackAngle = new Vector3 (0, 0, nowRotation.z + 180);
//		} 
//		else {
//			turnBackAngle = new Vector3 (0, 0, nowRotation.z + Random.Range(-90,90));
//		}
//		iTween.RotateTo (gameObject, iTween.Hash ("rotation", turnBackAngle, "time", Mathf.Abs (nowRotation.z - turnBackAngle.z) / 60, "easeType", iTween.EaseType.linear, "oncomplete", "Move", "oncompletetarget", gameObject));
//	}

//	public void Move(){
//		float duration = Random.Range (0.8f, 3);
//		if (duration < 1.5f) {
//			speed = 1;
//		} 
//		else if (duration < 2.2) {
//			speed = 2;
//		} 
//		else {
//			speed = 3;
//
//		}
//		hasChangeDirection = false;
//		rigidFish.velocity = transform.up*speed;
//		Invoke ("ChangeDirection", duration);
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
//			float impact = script.Impact;
//			Vector3 force = impact * (transform.position - col.gameObject.transform.position).normalized;
//			UpdateVelocityByCombineNewForce (new Vector2(force.x, force.y));
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
//
//
//	void UpdateVelocityByCombineNewForce(Vector2 force) {
//		Vector2 currentForce = rigidFish.velocity;
//
//		Vector2 newForce = new Vector2 ();
//
//		if (force.x * currentForce.x > 0) {
//			newForce.x = MaxForce (force.x, currentForce.x) + MinForce(force.x, currentForce.x) * 0;
//		} else {
//			newForce.x = currentForce.x + force.x;
//		}
//		if (force.y * currentForce.y > 0) {
//			newForce.y = MaxForce (force.y, currentForce.y);
//		} else {
//			newForce.y = currentForce.y + force.y + MinForce(force.y, currentForce.y) * 0;
//		}
//		rigidFish.velocity = newForce*forceScale;
//		print("force:" + force*forceScale);
//		rigidFish.AddForce (force * forceScale, ForceMode2D.Force);
//
//
//
//	}
//
//	float MaxForce(float force1, float force2) {
//		if (Mathf.Abs (force1) > Mathf.Abs (force2)) {
//			return force1;
//		}
//		return force2;
//	}
//
//	float MinForce(float force1, float force2) {
//		if (Mathf.Abs (force1) > Mathf.Abs (force2)) {
//			return force2;
//		}
//		return force1;
//	}
}
