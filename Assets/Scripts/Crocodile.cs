using UnityEngine;
using System.Collections;

public class Crocodile : MonoBehaviour {
	public Animator animator;
	bool wait = false;
	private float percent;
	// Use this for initialization
	void Start () {
		animator.PlayInFixedTime("CrocodileIdle", 0,0);
	}
	
	// Update is called once per frame
	void Update () {
		if (wait) {
			Debug.Log ("teat");
			animator.PlayInFixedTime ("CrocodileIdle", 0, percent);
		}
	}
	public void Wait(){
		percent = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		wait = true;
		Invoke ("ContinuePlay",Random.Range(2,3));

	}
	private void ContinuePlay(){
		wait = false;
		animator.PlayInFixedTime("CrocodileIdle", 0, percent);
	}

	public void StopAndRepeat(){
		gameObject.SetActive (false);
		Invoke ("Again",Random.Range(0.5f,1.5f));
	}
	private void Again(){
		gameObject.SetActive (true);
		animator.PlayInFixedTime("CrocodileIdle", 0,0);
	}
}
