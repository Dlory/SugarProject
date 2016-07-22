using UnityEngine;
using System.Collections;

public class SplashZone : MonoBehaviour {
	public GameObject[] Ripples;

	public bool IsRaining = true;
	public float DropsPerSecond = 3;
	public float DropsArea = 5f;
	public float ForceMin = 0.3f;
	public float ForceMax = 0.8f;

	private bool _isRaining = false;
	private BoxCollider _collider;


	private void StartRain() {
		if (_isRaining == false) {
			StopRain();
			StartCoroutine("DoMakeSplash");
			_isRaining = true;
		}
	}

	private void StopRain() {
		if (_isRaining) {
			StopCoroutine("DoMakeSplash");
			_isRaining = false;
		}
	}

	void Update() {
		if (!IsRaining && _isRaining) {
			StopRain ();
		} else if (IsRaining && !_isRaining) {
			StartRain ();
		}
	}

	private IEnumerator DoMakeSplash() {
		while (true) {
			// Selecting a random point within bounds
			Vector2 point = new Vector2(Random.Range(-0.5f * DropsArea, 0.5f * DropsArea), 
										Random.Range(-0.5f * DropsArea, 0.5f * DropsArea));
			point = transform.TransformPoint(point);

			if (Ripples.Length > 0) {
				GameObject ripple = Ripples [Random.Range (0, Ripples.Length - 1)];
				GameObject rippleWave = Instantiate(ripple, point, transform.rotation) as GameObject;
				RippleScript script = rippleWave.GetComponent<RippleScript> ();
				script.RippleScale = Random.Range (ForceMin, ForceMax);
				script.Impact = script.RippleScale * 0.5f;
			}

			// Wait for next splash
			yield return new WaitForSeconds(1f / Mathf.Clamp(DropsPerSecond, 0f, 100f));
		}
	}
}
