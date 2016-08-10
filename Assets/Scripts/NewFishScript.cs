using UnityEngine;
using System.Collections;
using DG.Tweening;



public class NewFishScript : MonoBehaviour {
	// Use this for initialization
	Rigidbody2D rigidFish;
	GameObject fishLight;

	float speed;             //鱼的速度
	float animSpeed;         //鱼动画播放速度
	float randomSpeed;       //随机鱼的速度

	Animator anim;
	Animator fishLightAnim;
	int type;            //鱼运动的模式
	int lastType;        //上一次的运动模式

	void Start () {
		InitData ();
		RandomRotate ();
		RandomMove ();
	}

	// Update is called once per frame
	void Update () {
//		Debug.Log ("模式为" + type  + ",动画速度为" + animSpeed);
		rigidFish.velocity = transform.up * speed;
		anim.SetFloat ("speed",animSpeed);
		fishLightAnim.SetFloat ("speed",animSpeed);
	}

	public void InitData(){
		rigidFish = gameObject.GetComponent<Rigidbody2D> ();
		fishLight = gameObject.transform.GetChild (0).gameObject;
		type = 0;
		speed = 1;
		animSpeed = 1;
		anim = gameObject.GetComponent<Animator> ();
		fishLightAnim = fishLight.GetComponent<Animator> ();
		anim.SetFloat ("speed",1);
		fishLightAnim.SetFloat ("speed", 1);

	}


	public void RandomRotate(){
		Invoke ("ChangeDirection", Random.Range (0.7f, 3));
	}
	public void RandomMove(){
		float duration = Random.Range (0.8f, 2);      //随机此次模式持续的时间
		randomSpeed = Random.Range (1f, 3f);      //随机此次模式的鱼的速度
		lastType = type;                              //将上一次的模式进行保存
		type = Random.Range (1, 4);                   //随机一个模式
		Debug.Log("上次模式为" + lastType + ",这次模式为" + type + "，这次速度为" + randomSpeed);
		switch (type) {
		case 1:       //加速、减速运动
			iTween.ValueTo (gameObject, iTween.Hash ("from", speed, "to", randomSpeed, "time", duration, "onupdate", "UpdateSpeed", "onupdatetarget", gameObject, "easetype", iTween.EaseType.easeInCubic, "oncomplete", "RandomMove", "oncompletetarget", gameObject));     
			break;
		case 2:      //匀速运动
			iTween.ValueTo (gameObject, iTween.Hash ("from", speed, "to", randomSpeed, "time", 0.5f, "onupdate", "UpdateSpeed", "onupdatetarget", gameObject, "easetype", iTween.EaseType.linear));
			Invoke ("RandomMove", duration);
			break;
		case 3:        //静止
			iTween.ValueTo (gameObject, iTween.Hash ("from", speed, "to", 0, "time", 0.5f, "onupdate", "UpdateSpeed", "onupdatetarget", gameObject, "easetype", iTween.EaseType.linear));
			Invoke ("RandomMove", duration);
			break;
		}
	}

	public void ChangeDirection(){
		Vector3 nowRotation = transform.rotation.eulerAngles;
		Vector3 randomAngle = new Vector3 (0, 0, nowRotation.z + Random.Range(-180,180));
		iTween.RotateTo (gameObject, iTween.Hash ("rotation", randomAngle, "time", Random.Range(0.7f,1.2f), "easeType", iTween.EaseType.linear, "oncomplete", "RandomRotate", "oncompletetarget", gameObject));
	}

	private void UpdateSpeed(float value){
		speed = value;
		if (lastType == 3) {
			animSpeed = 1 + value - value / randomSpeed;
		} 
		else {
			if (type == 3) {
				animSpeed = 1;
			} 
			else {
				animSpeed = value;
			}
		}
	}
}
