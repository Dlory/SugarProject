using UnityEngine;
using System.Collections;

public class CameraSmoothZoom : MonoBehaviour {
	public static CameraSmoothZoom instance;

	[HideInInspector]
	public bool isZoomingIn = false;
	[HideInInspector]
	public bool isZoomingOut = false;
	public float cameraSize {
		get {
			return TheCamera != null ? TheCamera.orthographicSize : 0;
		}
	}

	float startUpdateTimeStamp = 0;
	float zoomDuration = 0;
	float zoomToSize;
	float zoomFromSize;
	float zoomDelay;
	Camera TheCamera;

	void Awake() {
		if (instance == null) {
			CameraSmoothZoom.instance = this;
		}
	}

	public void ZoomCameraOrthographicSize(float size, float delay, float duration) {
		TheCamera = Camera.main;
		zoomDelay = delay;
		zoomToSize = size;
		zoomFromSize = TheCamera.orthographicSize;
		zoomDuration = duration;
		startUpdateTimeStamp = Time.realtimeSinceStartup;
		isZoomingIn = size >= TheCamera.orthographicSize;
		isZoomingOut = size < TheCamera.orthographicSize;
	}
	
	void LateUpdate () {
		if (isZoomingIn || isZoomingOut) {
			if (Time.realtimeSinceStartup - startUpdateTimeStamp >= zoomDelay) {
				float progress = (Time.realtimeSinceStartup - zoomDelay - startUpdateTimeStamp) / zoomDuration;
				TheCamera.orthographicSize = zoomFromSize + (zoomToSize - zoomFromSize) * progress;

				if (progress >= 1) {
					isZoomingIn = false;
					isZoomingOut = false;
					TheCamera.orthographicSize = zoomToSize;
				}
			}
		}
	}
}
