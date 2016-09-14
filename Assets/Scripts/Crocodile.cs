using UnityEngine;
using System.Collections;

public class Crocodile : MonoBehaviour {
	public Animator animator;
	public Animator FragmentAnimator;
	public GameObject Fragment;
	private float percent;
	int type = 0;
	// Use this for initialization
	void Start () {
		Fragment.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	public void Type(){
		type = Random.Range (1, 3);
		Invoke ("ChangeState", Random.Range (0f, 1.5f));
	}

	private void ChangeState(){
		animator.SetTrigger(GetTriggerName());
	}


	public void StopAndRepeat(){
		type = 0;
		gameObject.SetActive (false);
		Invoke ("Again",Random.Range(0.5f,2f));
	}

	private void Again(){
		gameObject.SetActive (true);

	}
	private string GetTriggerName()
	{
		switch (type) {
		case 1:
			return "Wink";
		case 2:
			return "Disappear";
		default:
			return "";
		}
	}

	public void OnCollisionEnter2D(Collision2D col){
		if (col.gameObject.tag == "Player") {     //鳄鱼被干扰产生干扰波
			animator.SetTrigger("Interfere");
//			animator.SetTrigger("Crush");        //鳄鱼被砸（未完成）
//			if (Fragment.active) {
//				Fragment.SetActive (true);
//				FragmentAnimator.SetTrigger ("Spit");
//			}
		}
	}


}
