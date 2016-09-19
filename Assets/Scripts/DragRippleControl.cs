using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragRippleControl : MonoBehaviour {
	public GameObject dragRippleAnimatorObj;
	public float segmentThreadHoldDistance = 1.5f;
	const float size = 3;
	List<GameObject> segments = new List<GameObject>();

	public void DargToPoint(Vector2 toPoint, Vector2 fromPoint, float force) {
		Vector2 p1 = fromPoint;
		Vector2 p2 = toPoint;

		if (segments.Count > 0) {
			for (int i = segments.Count - 1; i >= 0; i--) {
				GameObject seg = segments [i];
				if (Vector2.Distance (seg.transform.position, toPoint) >= segmentThreadHoldDistance) {
					for (int j = 0; j <= i; j++) {
						GameObject s = segments [j];
						DragRippleScript ripple = s.GetComponent<DragRippleScript> ();
						ripple.force = force;
						ripple.HideRipple ();
					}
					segments.RemoveRange (0, i+1);
					break;
				}
			}
		}
		if (segments.Count > 0) {
			GameObject seg = segments [segments.Count-1];
			Animator anim = seg.transform.GetChild(0).gameObject.GetComponentInChildren<Animator> ();
			if (anim != null) {
				anim.SetTrigger (Animator.StringToHash ("Fade"));
			}
		}

		GameObject obj = Instantiate(dragRippleAnimatorObj);
		obj.transform.parent = this.transform;
		obj.transform.position = new Vector3 (p1.x+(p2.x-p1.x)/2, p1.y+(p2.y-p1.y)/2, transform.position.z);
		obj.transform.eulerAngles = new Vector3(obj.transform.eulerAngles.x, obj.transform.eulerAngles.y, p1.CoordinateAngleTo (p2));
		segments.Add (obj);

		DragRippleScript script = obj.GetComponent<DragRippleScript> ();
		script.force = force;

		BoxCollider2D c = obj.AddComponent<BoxCollider2D> ();
		c.isTrigger = true;
		c.size = new Vector2 (Vector2.Distance (p1, p2), size);

		StartCoroutine(DelayDestoryFirstRipple(obj));
	}

	IEnumerator DelayDestoryFirstRipple(GameObject obj)
	{
		yield return new WaitForSeconds(1.5f);

		if (segments.Count > 0) {
			segments.Remove (obj);
			Destroy (obj);
		}
	}
}
