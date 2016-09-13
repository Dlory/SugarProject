using UnityEngine;
using System.Collections;

public class RippleScript : MonoBehaviour {
	public static string RippleDestroyEvent = "RippleDestroyEvent";
	[HideInInspector]
	public float Impact = 1f;
	[HideInInspector]
	public bool isInterferenceRipple = false;

	/// <summary>
	/// 衰减程度 0-1, 0表示在末端力不衰减，1表示在末端力衰减为0
	/// </summary>
	public float ImpactDamping = 0.3f;
	public float RippleScale = 1f;

	private float _initImpact;
	private Transform _colliderTransform;
	private Animator _animator;
	private Material _material;

	// Use this for initialization
	void Start () {
		_animator = GetComponent<Animator> ();
		_initImpact = Impact;

		float Scale = RippleScale;
		transform.localScale = new Vector3 (Scale, Scale, Scale);
		_colliderTransform = gameObject.transform.GetChild (0);
		_colliderTransform.gameObject.tag = this.tag;
		_colliderTransform.localScale = new Vector3 (0, 0, 1f);
	}
	
	// Update is called once per frame
	void LateUpdate () {
		float time = _animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		float percent = Mathf.Abs (time - Mathf.Floor(time));
		float edgeImpact = _initImpact * (1f - Mathf.Clamp (ImpactDamping, 0, 1));
		Impact = (1-percent) * (_initImpact - edgeImpact) + edgeImpact ;

		// 线性放大Collider
		float scale =  (- percent * percent + 2 * percent) * 1.1f;
		_colliderTransform.localScale = new Vector3 (scale, scale, 1);
	}

	public void SplashRotated(float rotation) {
		if (!_material) {
			_material = GetComponent<SpriteRenderer> ().material;
		}
		if (_material == null) {
			return;
		}
		_material.SetFloat ("_MaskRotation", rotation);
		_material.SetInt ("_MaskEnable", 1);
	}

	public void PlayOver() {
		BroadcastSystem.defaultBoardcast.SendMessage (RippleDestroyEvent, this, null);
		Destroy (gameObject);
	}

}
