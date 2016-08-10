﻿using UnityEngine;
using System.Collections;

public class GameControl : MonoBehaviour {
	public GameObject Ripple;
	public float Impact = 3f;
	public float SpawnMinInterval = 0.5f;
	public float SpawnMinDistanceX = 2f;
	public float SpawnMinDistanceY = 1f;
	public float EnergyPerRippleCost = 5;

	BoatEnergy boatEnergy;
	BoatScript boat;
	Camera mainCamera;

	int waterLayer;
	int playerLayer;
	int bankLayer;
	int LayerMaskWater;
	int LayerMaskBank;

	float lastSpawnTime;
	Vector2 prevPoint;
	bool detectWater;
	bool detectPlayer;

	void Start() {
		mainCamera = Camera.main ?? GetComponent<Camera>();
		waterLayer = LayerMask.NameToLayer ("Water");
		playerLayer = LayerMask.NameToLayer ("Player");
		bankLayer = LayerMask.NameToLayer ("Bank");
		LayerMaskWater = LayerMask.GetMask ("Water");
		LayerMaskBank = LayerMask.GetMask ("Bank");

		boat = GameObject.FindGameObjectWithTag (Constant.TagPlayer).GetComponent<BoatScript> ();
		boatEnergy = GetComponent<BoatEnergy> ();
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


	void ResetVariables(){
		detectWater = false;
		detectPlayer = false;
		prevPoint = Vector2.zero;
		lastSpawnTime = 0;
	}

	void TrySplashWater (Vector2 position) {
		if (detectWater && !detectPlayer) { //滑动水面
			Vector2 pos = mainCamera.ScreenToWorldPoint (position);
			RaycastHit2D hitWater = Physics2D.Raycast (pos, Vector2.zero, Mathf.Infinity, LayerMaskWater);
			RaycastHit2D hitBank = Physics2D.Raycast (pos, Vector2.zero, Mathf.Infinity, LayerMaskBank);

			if (hitWater.collider != null && hitBank.collider == null) {
				Vector2 hitPoint = hitWater.point;
				if (prevPoint != Vector2.zero && (Mathf.Abs (prevPoint.x - hitPoint.x) > SpawnMinDistanceX || Mathf.Abs (prevPoint.y - hitPoint.y) > SpawnMinDistanceY)) {
					if (boatEnergy.CastEnergy (EnergyPerRippleCost)) {
						SplashRipple (hitPoint);
						lastSpawnTime = Time.timeSinceLevelLoad;
						prevPoint = hitPoint;
					}
				} else if (Time.timeSinceLevelLoad - lastSpawnTime > SpawnMinInterval) {
					if (boatEnergy.CastEnergy (EnergyPerRippleCost)) {
						SplashRipple (hitPoint);
						lastSpawnTime = Time.timeSinceLevelLoad;
						prevPoint = hitPoint;
					}
				}
			}
		}
	}

//  Finger Call backs
	void OnFingerDown(FingerDownEvent e ){
		ResetVariables ();

		Vector2 pos = mainCamera.ScreenToWorldPoint (e.Position);
		Collider2D[] colliders = Physics2D.OverlapPointAll (pos);
		foreach (Collider2D c in colliders) {
			if (c.gameObject.layer == waterLayer) {
				detectWater = true;
			} else if (c.gameObject.layer == playerLayer) {
				detectPlayer = true;
			}
		}
		prevPoint = pos;

		if (detectPlayer) {
			boat.Status = BoatStatus.ReadyToFly;
		}
	}

	void OnFingerUp(FingerUpEvent e) {
		ResetVariables ();
	}

	void OnFingerMove(FingerMotionEvent e) {
		TrySplashWater (e.Position);

		if (detectPlayer && boat.Status != BoatStatus.Flying) {
			Vector2 hitPoint = mainCamera.ScreenToWorldPoint (e.Position);
			if (Vector2.Distance (prevPoint, hitPoint) > 2f) {
				boat.FlyToDirection (hitPoint - prevPoint);
			}
		}
	}

	void OnFingerStationary(FingerMotionEvent e) {
		TrySplashWater (e.Position);
	}
}