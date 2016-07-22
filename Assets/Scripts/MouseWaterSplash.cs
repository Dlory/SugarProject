using UnityEngine;
using System.Collections;

public class MouseWaterSplash : MonoBehaviour {
	public GameObject Ripple;
	public GameObject RippleMove;
	public float SpawnInterval = 0.25f;

	private Camera Camera;
	private int layerMask;
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
		layerMask = LayerMask.GetMask ("Water");
	}

	// Updating the splash generation
	void FixedUpdate() {
		// Creating a splash line between previous position and current
		if (GUIUtility.hotControl == 0 && (Input.GetMouseButton (0) || Input.touchCount > 0)) {
			if (lastSpawnTime > 0 && Time.timeSinceLevelLoad - lastSpawnTime < SpawnInterval) {
				return;
			}
			lastSpawnTime = Time.timeSinceLevelLoad;

			// Creating a ray from camera to world
			Vector2 point;
			if (Input.GetMouseButton (0) == false) {
				point = Camera.ScreenToWorldPoint (Input.touches [0].position);
			} else {
				point = Camera.ScreenToWorldPoint (Input.mousePosition);
			}

			RaycastHit2D hit = Physics2D.Raycast (point, Vector2.zero, Mathf.Infinity, layerMask);
			if (hit.collider != null) {
				if (prevPoint == Vector2.zero || Vector2.Distance(prevPoint, hit.point) < 0.2f) {
					Instantiate (Ripple, hit.point, transform.rotation);
				} else {
					Instantiate (RippleMove, hit.point, transform.rotation);
				}
				prevPoint = hit.point;
			}
		} else {
			prevPoint = Vector2.zero;
		}
	}
}