using UnityEngine;
using System.Collections;

public class Bubble : MonoBehaviour {
	public Animator animator;
	// Use this for initialization
	void Start () {
		Invoke("RandomAnimation",Random.Range(0,0.5f));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void RandomAnimation(){
		int i = Random.Range (1, 5);
		switch (i) {
		case 1:
			animator.PlayInFixedTime ("BubblePath1", 0, 0);
			break;
		case 2:
			animator.PlayInFixedTime ("BubblePath2", 0, 0);
			break;
		case 3:
			animator.PlayInFixedTime ("BubblePath3", 0, 0);
			break;
		case 4:
			animator.PlayInFixedTime ("BubblePath4", 0, 0);
			break;
		}

	}
}
