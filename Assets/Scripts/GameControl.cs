using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour {
	public GameObject Ripple;
	public float Impact = 3f;
	public float SpawnMinInterval = 0.5f;
	public float SpawnMinDistanceX = 2f;
	public float SpawnMinDistanceY = 1f;
	public float EnergyPerRippleCost = 5;

	[HideInInspector]
	public bool dialogueTalkMode = false;

	BoatEnergy boatEnergy;
	BoatScript boat;
	Camera mainCamera;

	int layerWater;
	int layerPlayer;
	int layerBank;

	int detectLayer;
	Collider2D detectCollider;

	float lastSpawnTime;
	Vector2 prevPoint;

	protected virtual void OnEnable()
	{
		// Hook events
		Lean.LeanTouch.OnFingerDown     += OnFingerDown;
		Lean.LeanTouch.OnFingerSet      += OnFingerSet;
		Lean.LeanTouch.OnFingerUp       += OnFingerUp;
		Lean.LeanTouch.OnFingerDrag     += OnFingerDrag;
	}

	protected virtual void OnDisable()
	{
		// Unhook events
		Lean.LeanTouch.OnFingerDown     -= OnFingerDown;
		Lean.LeanTouch.OnFingerSet      -= OnFingerSet;
		Lean.LeanTouch.OnFingerUp       -= OnFingerUp;
		Lean.LeanTouch.OnFingerDrag     -= OnFingerDrag;
	}

	void Start() {
		mainCamera = Camera.main ?? GetComponent<Camera>();

		layerPlayer = LayerMask.NameToLayer ("Player");
		layerWater = LayerMask.NameToLayer ("WaterTap");
		layerBank = LayerMask.NameToLayer ("Bank");

		boat = GameObject.FindGameObjectWithTag ("Player").GetComponent<BoatScript> ();
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
		detectLayer = -1;
		detectCollider = null;
		prevPoint = Vector2.zero;
	}

	void TrySplashWater (Vector2 position) {
		if (detectLayer == layerWater) { //滑动水面
			Vector2 pos = mainCamera.ScreenToWorldPoint (position);
			RaycastHit2D hitWater = Physics2D.Raycast (pos, Vector2.zero, Mathf.Infinity, 1 << layerWater);
			RaycastHit2D hitBank = Physics2D.Raycast (pos, Vector2.zero, Mathf.Infinity, 1 << layerBank);

			if (hitWater.collider != null && hitBank.collider == null) {
				Vector2 hitPoint = hitWater.point;
				if (prevPoint != Vector2.zero && (Mathf.Abs (prevPoint.x - hitPoint.x) > SpawnMinDistanceX || Mathf.Abs (prevPoint.y - hitPoint.y) > SpawnMinDistanceY)) {
					if (boatEnergy.CastEnergy (EnergyPerRippleCost)) {
						SplashRipple (hitPoint);
						lastSpawnTime = Time.timeSinceLevelLoad;
						prevPoint = hitPoint;
					}
				} else if (Time.timeSinceLevelLoad - lastSpawnTime > SpawnMinInterval) {
					if (boatEnergy.CastEnergy (EnergyPerRippleCost * 0.5f)) {
						SplashRipple (hitPoint);
						lastSpawnTime = Time.timeSinceLevelLoad;
						prevPoint = hitPoint;
					}
				}
			}
		}
	}

//  Finger Call backs
	public void OnFingerDown(Lean.LeanFinger finger) {
		if (dialogueTalkMode) {
			BroadcastSystem.defaultBoardcast.SendMessage (NPCDialogue.NPCDialugeNextEvent, null, null);
			return;
		}

		ResetVariables ();

		bool detectNPC = false;
		Vector2 pos = mainCamera.ScreenToWorldPoint (finger.ScreenPosition);
		Collider2D[] colliders = Physics2D.OverlapPointAll (pos);
		int[] layerOrders = new int[]{layerPlayer, layerBank, layerWater };

		foreach (int layer in layerOrders) {
			foreach (Collider2D c in colliders) {
				if (c.GetComponent<NPCScript> () != null) {
					detectCollider = c;
					detectNPC = true;
					break;
				}
				int objLayer = c.gameObject.layer;
				if (objLayer == layer) {
					detectLayer = objLayer;
					detectCollider = c;
					break;
				}
			}
			if (detectLayer != -1 || detectNPC) {
				break;
			}
		}
		prevPoint = pos;

		if (detectNPC) {
			NPCScript npc = detectCollider.gameObject.GetComponent<NPCScript> ();
			if (npc != null) {
				npc.TalkTo (); // 跟NPC对话
			}
		}
	}

	public void OnFingerUp(Lean.LeanFinger finger) {
		if (dialogueTalkMode) {
			return;
		}

		Vector2 hitPoint = mainCamera.ScreenToWorldPoint (finger.ScreenPosition);
		if (detectLayer == layerPlayer && (boat.Status == BoatStatus.Normal) && Vector2.Distance (prevPoint, hitPoint) < 0.5f) {
			boat.AnnoyingAnimation ();
		}

		ResetVariables ();
	}

	public void OnFingerDrag(Lean.LeanFinger finger) {
		if (dialogueTalkMode) return;

		if (detectLayer == layerPlayer && boat.Status != BoatStatus.Flying) {
			Vector2 hitPoint = mainCamera.ScreenToWorldPoint (finger.ScreenPosition);
			if (Vector2.Distance (prevPoint, hitPoint) > 2f && boat.flyable) {
				boat.FlyToDirection (hitPoint - prevPoint);
			}
		} else {
			TrySplashWater (finger.ScreenPosition);
		}
	}

	public void OnFingerSet(Lean.LeanFinger finger) {
		if (dialogueTalkMode) return;

		TrySplashWater (finger.ScreenPosition);
	}
}