using UnityEngine;
using System.Collections;

public class PlayerFollowScript : MonoBehaviour {
	private Transform player;
	private Camera mainCamera;
	private Vector2 boundsSize;

	// Use this for initialization
	void Start () {
		mainCamera = Camera.main;
		player = gameObject.transform;

		float orthographicSize = mainCamera.orthographicSize;
		float aspectRatio = Screen.width / Screen.height;
		float cameraHeight = orthographicSize * 2f;
		float cameraWidth = cameraHeight * aspectRatio;
		boundsSize = new Vector2 (cameraWidth/5, cameraHeight/5);
		boundsSize = Vector2.zero;
	}

	// Update is called once per frame
	void Update () {
		if (boundsSize == null || boundsSize == Vector2.zero) {
			float x = player.position.x;
			float y = player.position.y;
			mainCamera.transform.position = new Vector3 (x, y, mainCamera.transform.position.z);
		} else {
			float cameraX = mainCamera.transform.position.x;
			float cameraY = mainCamera.transform.position.y;
			float playerX = player.position.x;
			float playerY = player.position.y;

			if (cameraX - playerX > boundsSize.x / 2) { // 左边
				cameraX = playerX + boundsSize.x / 2;
			} else if (playerX - cameraX > boundsSize.x / 2) { // 右边
				cameraX = playerX - boundsSize.x / 2;
			}
			if (cameraY - playerY > boundsSize.y / 2) { // 上边
				cameraY = playerY + boundsSize.y / 2;
			} else if (playerY - cameraY > boundsSize.y / 2) { // 下边
				cameraY = playerY - boundsSize.y / 2;
			}
			mainCamera.transform.position = new Vector3 (cameraX, cameraY, mainCamera.transform.position.z);
		}
	}
}
