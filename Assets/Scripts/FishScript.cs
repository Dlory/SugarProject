using UnityEngine;
using System.Collections;
using DG.Tweening;



public class FishScript : MonoBehaviour {
	// Use this for initialization
	Rigidbody2D rigidFish;
	float speed;
	public float MaxRadius;
	public float MinRadius;

	GameObject boat;
	bool enterCircle;
	bool hasChangeDirection;
	float distance;
	Vector3 nextTarget;
	public float forceScale;
	public float minForce;
	public float maxForce;
	Animator anim;


	GameObject fishLight;
	Animator fishLightAnim;
	bool tooFar;
	float time;
	public float TargetValue;

	void Start () {
		InitData ();
		ChangeDirection ();
	}

	// Update is called once per frame
	void Update () {

		if (Physics2D.CircleCast (transform.position, 1.5f,transform.up,0.5f,(1<<LayerMask.NameToLayer("Stone")|1<<LayerMask.NameToLayer("Bank")))  && !hasChangeDirection) {

			GameObject hit = Physics2D.CircleCast (transform.position, 1.5f,transform.up,0.5f,(1<<LayerMask.NameToLayer("Stone")|1<<LayerMask.NameToLayer("Bank"))).collider.gameObject;

			Debug.Log (hit.name);
			rigidFish.velocity = Vector3.zero;
			CancelInvoke ();
			iTween.Stop (gameObject);
			DOTween.Kill (gameObject);
			if (enterCircle) {
				if (tooFar) {
					rigidFish.velocity = Vector3.zero;
					enterCircle = false;
					DOTween.Kill (fishLight);
					DOTween.Kill (gameObject);
					tooFar = false;
					ChangeDirection ();
					time = 0;
					FishLight (new Color32(255,255,255,0));
					Debug.Log ("fish escape");
				} 
				else {
					Debug.Log ("碰到东西了");
					hasChangeDirection = true;

					RandomMoveInCircle ();
				}
			} 
			else {
				hasChangeDirection = true;
				ChangeDirection ();

			}
		}

		if (enterCircle) {
			distance = Vector3.Distance (transform.position, boat.transform.position);
			if (tooFar) {
				time += Time.deltaTime;
				if (time >= 3) {
					rigidFish.velocity = Vector3.zero;
					enterCircle = false;
					DOTween.Kill (fishLight);
					DOTween.Kill (gameObject);
					tooFar = false;
					ChangeDirection ();
					time = 0;
					FishLight (new Color32(255,255,255,0));
					Debug.Log ("fish escape");
				}
			} 
		} 
		else {
			distance = 0;
		}

	}

	public void InitData(){
		boat = GameObject.FindWithTag ("Player");
		rigidFish = gameObject.GetComponent<Rigidbody2D> ();
		fishLight = gameObject.transform.GetChild (0).gameObject;

		speed = 1;
		hasChangeDirection = false;
		enterCircle = false;
		//outCircle = true;
		distance = 0;
		anim = gameObject.GetComponent<Animator> ();
		fishLightAnim = fishLight.GetComponent<Animator> ();
		anim.SetFloat ("speed",1);


		fishLightAnim.SetFloat ("speed", 1);

		time = 0;
		tooFar = false;
		Color a = fishLight.transform.GetComponent<SpriteRenderer> ().color;
		a.a = 0;
		fishLight.transform.GetComponent<SpriteRenderer> ().color = a;
	}

	public void ChangeDirection(){
		Vector3 nowRotation = transform.rotation.eulerAngles;
		Vector3 turnBackAngle;
		if (hasChangeDirection) {
			turnBackAngle = new Vector3 (0, 0, nowRotation.z + 180);
		} 
		else {
			turnBackAngle = new Vector3 (0, 0, nowRotation.z + Random.Range(-90,90));
		}
		iTween.RotateTo (gameObject, iTween.Hash ("rotation", turnBackAngle, "time", Mathf.Abs (nowRotation.z - turnBackAngle.z) / 60, "easetype", iTween.EaseType.linear, "oncomplete", "Move", "oncompletetarget", gameObject));
	}

	public void Move(){
		float duration = Random.Range (0.8f, 3);
		if (duration < 1.5f) {
			speed = 1;
		} 
		else if (duration < 2.2) {
			speed = 2;
		} 
		else {
			speed = 3;
		}
		hasChangeDirection = false;
		rigidFish.velocity = transform.up*speed;
		Invoke ("ChangeDirection", duration);
	}

	void OnTriggerEnter2D(Collider2D col){
		if (col.gameObject.tag == "MagicCircle") {
			tooFar = false;
			enterCircle = true;
			Debug.Log (gameObject.name + " Enter2");
			iTween.Stop (gameObject);
			rigidFish.velocity = Vector3.zero;
			DOTween.Kill (gameObject);
			DOTween.Kill (fishLight);
			CancelInvoke ("ChangeDirection");
			FishLight (new Color32(255,248,0,255));
			RandomMoveInCircle ();

		}


		if (col.gameObject.tag == "Ripple" && !enterCircle) {
			RippleScript script = col.gameObject.GetComponentInParent<RippleScript> ();
			float impact = script.Impact;
			Vector3 force = impact * (transform.position - col.gameObject.transform.position).normalized;
			UpdateVelocityByCombineNewForce (new Vector2(force.x, force.y));
		} 
	}
	void OnTriggerExit2D(Collider2D col){
		FishLight (new Color32(255,255,255,0));
	}


	void RandomMoveInCircle()
	{
		float angle = Random.Range (0, 2 * Mathf.PI);
		float r = Random.Range (MinRadius, MaxRadius);
		if (distance > 5.5) {
			tooFar = true;
			Debug.Log (" Rotatet Too far : " + Vector3.Distance (boat.transform.position, transform.position));
			nextTarget = boat.transform.position;
			anim.SetFloat ("speed",2);
			fishLightAnim.SetFloat ("speed",2);
		}
		else {
			nextTarget = new Vector3 (Mathf.Cos (angle), Mathf.Sin (angle), 0) * r + boat.transform.position;
			anim.SetFloat ("speed",0.8f);
			fishLightAnim.SetFloat ("speed",0.8f);
			Debug.DrawLine (transform.position, nextTarget, Color.red, 10);
		}



		Vector3 horizontalDir = nextTarget - transform.position;
		float rotateTowardsAngle = GetSignAngle (horizontalDir.normalized, transform.up.normalized);
		transform.DORotate(new Vector3(0, 0, rotateTowardsAngle + transform.localEulerAngles.z), Mathf.Abs(rotateTowardsAngle)/120).SetId (gameObject).OnComplete(moveToTarget);


	}


	private void moveToTarget(){
		if (distance > 5.5 && !tooFar) {

			Debug.Log (" Move Too far : " + Vector3.Distance (boat.transform.position, transform.position));
			RandomMoveInCircle ();
		} 
		else {
			if (tooFar) {
				transform.DOMove (nextTarget, Vector3.Distance (nextTarget, transform.position) / (speed * 2), false).OnComplete (RandomMoveInCircle).SetId (gameObject);
			} 
			else {
				transform.DOMove (nextTarget, Vector3.Distance (nextTarget, transform.position) / (speed * 0.8f), false).OnComplete (RandomMoveInCircle).SetId (gameObject);
			}
		}
	}

	private float GetSignAngle(Vector3 a,Vector3 b)
	{
		return -Vector3.Angle (a, b) * Mathf.Sign (Vector3.Cross(a,b).z);

	}


	void UpdateVelocityByCombineNewForce(Vector2 force) {
		Vector2 currentForce = rigidFish.velocity;

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
		rigidFish.velocity = newForce*forceScale;
		print("force:" + force*forceScale);
		rigidFish.AddForce (force * forceScale, ForceMode2D.Force);



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


	public void FishLight(Color color){
		if (fishLight.transform.GetComponent<SpriteRenderer> ().color.a < color.a) {
			fishLight.transform.GetComponent<SpriteRenderer> ().DOColor (color, 3 * (color.a - fishLight.transform.GetComponent<SpriteRenderer> ().color.a));
		}
		else{
			fishLight.transform.GetComponent<SpriteRenderer> ().DOColor (color, 3 * (fishLight.transform.GetComponent<SpriteRenderer> ().color.a - color.a)).SetId(fishLight);
		}
	}

}
