using UnityEngine;
using System.Collections;

public class RippleScript : MonoBehaviour {
	public float FinalRadius = 1f;
	public float Impact = 1f;
	public float RippleScale = 1f;

	private float _initImpact;
	private Transform _colliderTransform;
	private Animator _animator;

	// Use this for initialization
	void Start () {
		_animator = GetComponent<Animator> ();
		_initImpact = Impact;

		float Scale = RippleScale;
		transform.localScale = new Vector3 (Scale, Scale, Scale);
		_colliderTransform = gameObject.transform.GetChild (0);
		_colliderTransform.gameObject.tag = this.tag;
	}
	
	// Update is called once per frame
	void Update () {
		float time = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		float percent = Mathf.Abs (time - Mathf.Floor(time));
		Impact = _initImpact * (1.0f - percent);

		percent *= 1.45f;
		_colliderTransform.localScale = new Vector3 (percent, percent, 1);
	}

	public void PlayOver() {
		Destroy (gameObject);
	}
}
