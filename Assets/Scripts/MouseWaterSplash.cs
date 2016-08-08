using UnityEngine;
using System.Collections;

public class MouseWaterSplash : MonoBehaviour {
	public GameObject Ripple;
	public float Impact = 3f;
	public float SpawnMinInterval = 0.5f;
	public float SpawnMinDistanceX = 2f;
	public float SpawnMinDistanceY = 1f;

	private Camera Camera;
	private int waterLayer;
	private int playerLayer;
	private float lastSpawnTime;
	private Vector2 prevPoint;

	void Start() {
		try {
			Camera = Camera.main ?? GetComponent<Camera>();
		} catch {}
		if (Camera == null) {
			Debug.LogError("No Camera attached and no active Camera was found, please set the Camera property for DW_MouseSplash to work", this);
			gameObject.SetActive(false);
			return;
		}
		waterLayer = LayerMask.NameToLayer ("Water");
		playerLayer = LayerMask.NameToLayer ("Player");

	}

	// Updating the splash generation
	void FixedUpdate() {
		// Creating a splash line between previous position and current
		if (GUIUtility.hotControl == 0 && (Input.GetMouseButton (0) || Input.touchCount > 0)) {
			// Creating a ray from camera to world
			Vector2 point;
			if (Input.GetMouseButton (0) == false) {
				point = Camera.ScreenToWorldPoint (Input.touches [0].position);
			} else {
				point = Camera.ScreenToWorldPoint (Input.mousePosition);
			}

			bool touchWater = false;
			bool touchPlayer = false;
			Collider2D[] colliders = Physics2D.OverlapPointAll (point);
			foreach (Collider2D c in colliders) {
				if (c.gameObject.layer == waterLayer) {
					touchWater = true;
				} else if (c.gameObject.layer == playerLayer) {
					touchPlayer = true;
				}
			}

			if (touchWater && !touchPlayer) {
				Vector2 hitPoint = point;
				if (prevPoint != Vector2.zero && (Mathf.Abs(prevPoint.x - hitPoint.x) > SpawnMinDistanceX || Mathf.Abs(prevPoint.y - hitPoint.y) > SpawnMinDistanceY) ) {
					SplashRipple (hitPoint);
					lastSpawnTime = Time.timeSinceLevelLoad;
					prevPoint = hitPoint;

				} else if (Time.timeSinceLevelLoad - lastSpawnTime > SpawnMinInterval) {
					SplashRipple (hitPoint);
					lastSpawnTime = Time.timeSinceLevelLoad;
					prevPoint = hitPoint;
				}
			}
		} else {
			if (prevPoint != Vector2.zero) {
				//print ("放开了");
			}
			prevPoint = Vector2.zero;
		}
	}

	void SplashRipple(Vector2 hitPoint) {
		if (Ripple != null) {

			GameObject gameObj = Instantiate (Ripple, hitPoint, transform.rotation) as GameObject;
			RippleScript ripple = gameObj.GetComponent<RippleScript>();
			ripple.Impact = Impact;

			if (prevPoint != Vector2.zero && (Mathf.Abs(prevPoint.x - hitPoint.x) > SpawnMinDistanceX || Mathf.Abs(prevPoint.y - hitPoint.y) > SpawnMinDistanceY)) {
				float angle = AngleBetweenVector2 (prevPoint, hitPoint);
				ripple.SplashRotated (Mathf.Deg2Rad * angle);
			}
		}
	}

	float AngleBetweenVector2(Vector2 vec1, Vector2 vec2) {
		Vector2 diference = vec2 - vec1;
		float sign = (vec2.y < vec1.y)? 1.0f : -1.0f;
		return Vector2.Angle(Vector2.right, diference) * sign;
	}
}