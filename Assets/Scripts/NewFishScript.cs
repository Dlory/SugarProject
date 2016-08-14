using UnityEngine;
using System.Collections;
using DG.Tweening;



public class NewFishScript : MonoBehaviour {
	// Use this for initialization
	Rigidbody2D rigidFish;
	GameObject fishLight;
	GameObject player;

	float speed;             //鱼的速度
	float animSpeed;         //鱼动画播放速度
	float randomSpeed;       //随机鱼的速度

	Animator anim;
	Animator fishLightAnim;
	int type;            //鱼运动的模式
	int lastType;        //上一次的运动模式

	public float MaxRadius;        //魔法圈内随机点生成的范围最大半径
	public float MinRadius;        //魔法圈内随机点生成的范围最小半径

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
//		Debug.Log("上次模式为" + lastType + ",这次模式为" + type + "，此时一共有" + iTween.Count(gameObject) + "个iTween");
	}

	public void InitData(){
		rigidFish = gameObject.GetComponent<Rigidbody2D> ();
		fishLight = gameObject.transform.GetChild (0).gameObject;
		player = GameObject.FindWithTag ("Player");

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
//		Debug.Log("上次模式为" + lastType + ",这次模式为" + type + "，这次速度为" + randomSpeed);
		switch (type) {
		case 1:       //加速、减速运动
			//iTween.ValueTo (gameObject, iTween.Hash ("from", speed, "to", randomSpeed, "time", duration, "onupdate", "UpdateSpeed", "onupdatetarget", gameObject, "easetype", iTween.EaseType.easeInCubic, "oncomplete", "RandomMove", "oncompletetarget", gameObject));     
			DOTween.To(UpdateSpeed,speed,randomSpeed,duration).SetEase(Ease.InCubic).OnComplete(RandomMove).SetId(gameObject);
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
        if (stopRotate)
            return;
		Vector3 nowRotation = transform.rotation.eulerAngles;
		Vector3 randomAngle = new Vector3 (0, 0, nowRotation.z + Random.Range(-180,180));
		//iTween.RotateTo (gameObject, iTween.Hash ("rotation", randomAngle, "time", Random.Range(0.7f,1.2f), "easeType", iTween.EaseType.linear, "oncomplete", "RandomRotate", "oncompletetarget", gameObject));
		transform.DORotate(randomAngle,Random.Range(0.7f,1.2f)).OnComplete(RandomRotate).SetId(gameObject);
	}

	private void UpdateSpeed(float value){
		speed = value;
		if (lastType == 3) {         //如果上一个模式是静止，则速度由0变成x，动画播放速度要由1变成x
			animSpeed = 1 + value - value / randomSpeed;
		} 
		else {                  //如果上一个模式是非静止，而当前模式是静止，则速度由x变成0，动画播放速度由x变为1
			if (type == 3) {
				animSpeed = 1;
			} 
			else {                //其余情况下，动画播放速度等于鱼的速度
				animSpeed = value;
			}
		}
	}

    private bool stopRotate = false;
	public void RandomMoveInCircle(){             //鱼进入魔法圈后的随机运动的逻辑实现
		CancelInvoke();
		Debug.Log("test");
		iTween.Stop(gameObject);
		Debug.Log (iTween.Count (gameObject));
		speed = 1;
		animSpeed = 1;
		Invoke ("Test", 0.5f);
		Debug.Log (iTween.Count (gameObject));
        stopRotate = true;
        DOTween.Kill (gameObject);

        float angle = Random.Range(0, 2 * Mathf.PI);       //随机一个角度
        float r = Random.Range(MinRadius, MaxRadius);       //随机一个半径
        Vector3 nextTarget = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * r + player.transform.position;        //利用随机的角度与半径算出此随机点的位置
        Debug.DrawLine(player.transform.position, nextTarget, Color.green, 1);
        Vector3 horizontalDir = nextTarget - transform.position;
        float rotateTowardsAngle = GetSignAngle(horizontalDir.normalized, transform.up.normalized);               //计算出随机点与船的连线，到鱼的y轴的夹角
        transform.DORotate(new Vector3(0, 0, rotateTowardsAngle + transform.localEulerAngles.z), Mathf.Abs(rotateTowardsAngle) / 30).SetId(gameObject);      //利用动画让鱼从当前角度朝向随机点的方向转动
        Invoke("RandomMoveInCircle", Random.Range(0.5f, 1.5f));               //随机时间后重新调用这个方法
    }

    private float GetSignAngle(Vector3 a,Vector3 b)        //计算从向量a到向量b的夹角的方法
	{
		return -Vector3.Angle (a, b) * Mathf.Sign (Vector3.Cross(a,b).z);

	}

	public void Test(){
		Debug.Log (iTween.Count (gameObject));
	}
}
