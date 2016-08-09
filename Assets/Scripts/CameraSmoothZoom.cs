using UnityEngine;
using System.Collections;

public class CameraSmoothZoom : MonoBehaviour {
	bool isZooming = false;
	float startUpdateTimeStamp = 0;
	float zoomDuration = 0;
	float zoomToSize;
	float zoomFromSize;
	float zoomDelay;
	Camera TheCamera;

	public void ZoomCameraOrthographicSize(float size, float delay, float duration) {
		TheCamera = GetComponent<Camera>() ?? Camera.main;
		zoomDelay = delay;
		zoomToSize = size;
		zoomFromSize = TheCamera.orthographicSize;
		zoomDuration = duration;
		startUpdateTimeStamp = Time.realtimeSinceStartup;
		isZooming = true;
	}
	
	void Update () {
		if (isZooming) {
			if (Time.realtimeSinceStartup - startUpdateTimeStamp >= zoomDelay) {
				float progress = (Time.realtimeSinceStartup - zoomDelay - startUpdateTimeStamp) / zoomDuration;
				TheCamera.orthographicSize = zoomFromSize + (zoomToSize - zoomFromSize) * progress;

				if (progress >= 1) {
					isZooming = false;
					TheCamera.orthographicSize = zoomToSize;
				}
			}
		}
	}
}
