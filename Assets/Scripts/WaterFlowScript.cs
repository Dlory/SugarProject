using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterFlowScript : MonoBehaviour {
	public float StartFlowDelay = 0f;
	public float FlowSpeed = 20f;
	public float FlowInterval = 0f;

	private PolygonCollider2D polygonCollider;
	private SplineTrailRenderer trailReference;
	private iTweenPath itwPath;
	private Mesh mesh;

	private float defaultTrailMaxLength;
	private Vector3 defaultPosition;

	private float lastFlowInterval = 0;
	private bool isFading = false;
	private bool isFlowing = false;

	void Start () {
		trailReference = GetComponentInChildren<SplineTrailRenderer> ();
		defaultTrailMaxLength = trailReference.maxLength;
		defaultPosition = trailReference.gameObject.transform.position;
		mesh = trailReference.GetComponent<MeshFilter> ().mesh;
		itwPath = GetComponent<iTweenPath> ();
	}

	void TrailFlow () {
		Vector3[] paths = itwPath.nodes.ToArray();
		GameObject gameObject = trailReference.gameObject;

		Hashtable args = new Hashtable();
		args.Add("path", paths);
		args.Add("easeType", iTween.EaseType.linear);
		args.Add("speed", FlowSpeed);
		args.Add("movetopath", true);
		args.Add("oncomplete", "OnAnimationEnd");
		args.Add("oncompletetarget", this.gameObject);

		isFading = false;
		isFlowing = true;
		gameObject.transform.position = defaultPosition;
		trailReference.maxLength = defaultTrailMaxLength;
		trailReference.Clear ();
		iTween.MoveTo(gameObject, args);
	}

	void OnAnimationEnd() {
		isFading = true;
	}

	void Update() {
		if (isFading && trailReference.maxLength > 0) {
			trailReference.maxLength -= FlowSpeed * Time.deltaTime;
			if (trailReference.maxLength <= 0) {
				isFlowing = false;
				isFading = false;
				lastFlowInterval = 0;
			}
		}
		if (!isFlowing){
			lastFlowInterval += Time.deltaTime;
			if (lastFlowInterval > FlowInterval) {
				TrailFlow ();
			}
		}
	}

	void LateUpdate() {
		
	}
}
