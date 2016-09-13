using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameControl : MonoBehaviour {
	public GameObject Ripple;
	[Range(0,1)]
	public float minRippleScale = 0.35f;
	public float maxImpact = 6f;
	public float accumulateTime = 1f; //蓄到最大力的时间
	public float spawnPathPointBetween = 1f;
	public int flowLastRippleCount = 8; 
	public GameObject controlTrail;

	[HideInInspector]
	public bool dialogueTalkMode = false;
	BoatScript boat;
	Camera mainCamera;

	int layerWater;
	int layerPlayer;
	int layerBank;

	int detectLayer;
	Collider2D detectCollider;
	GameObject liveControlTrail;

	float touchDownSinceTime;
	List<Vector2> points = new List<Vector2>();

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
	}

	RippleScript SplashRipple(Vector2 hitPoint, float impact, float splashRotate) {
		if (Ripple != null) {
			GameObject gameObj = Instantiate (Ripple, hitPoint, transform.rotation) as GameObject;
			RippleScript ripple = gameObj.GetComponent<RippleScript>();
			ripple.Impact = impact;
			ripple.RippleScale = impact * 1.3f / maxImpact;

			if (splashRotate != float.MaxValue) {
				ripple.SplashRotated (Mathf.Deg2Rad * splashRotate);
			}
			return ripple;

//			if (prevPoint != Vector2.zero && (Mathf.Abs(prevPoint.x - hitPoint.x) > SpawnMinDistanceX || Mathf.Abs(prevPoint.y - hitPoint.y) > SpawnMinDistanceY)) {
//				float angle = AngleBetweenVector2 (prevPoint, hitPoint);
//				ripple.SplashRotated (Mathf.Deg2Rad * angle);
//			}
		}
		return null;
	}

	float AngleBetweenVector2(Vector2 vec1, Vector2 vec2) {
		Vector2 diference = vec2 - vec1;
		float sign = (vec2.y < vec1.y)? 1.0f : -1.0f;
		return Vector2.Angle(Vector2.right, diference) * sign;
	}

	void ResetVariables(){
		detectLayer = -1;
		detectCollider = null;
		points.Clear();
	}

	void SplashWater (Vector2 worldPosition, float impact, float splashRotate) {
		if (detectLayer != layerWater) return;

		//滑动水面
		Vector2 pos = worldPosition;
		RaycastHit2D hitWater = Physics2D.Raycast (pos, Vector2.zero, Mathf.Infinity, 1 << layerWater);
		RaycastHit2D hitBank = Physics2D.Raycast (pos, Vector2.zero, Mathf.Infinity, 1 << layerBank);

		if (hitWater.collider != null && hitBank.collider == null) {
			Vector2 hitPoint = hitWater.point;
			SplashRipple (hitPoint, impact, splashRotate);
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
		points.Add (finger.ScreenPosition);
		touchDownSinceTime = Time.timeSinceLevelLoad;

		if (detectNPC) {
			NPCScript npc = detectCollider.gameObject.GetComponent<NPCScript> ();
			if (npc != null) {
				npc.TalkTo (); // 跟NPC对话
			}
		} else {
			if (liveControlTrail != null) Destroy (liveControlTrail);
			liveControlTrail = SpawnControlTrail ();

			liveControlTrail.transform.position = ConvertControlTrailPosition(finger.ScreenPosition);
			liveControlTrail.SetActive(true);
		}
	}

	public void OnFingerUp(Lean.LeanFinger finger) {
		if (dialogueTalkMode) {
			return;
		}

		Vector2 hitPoint = mainCamera.ScreenToWorldPoint (finger.ScreenPosition);
		if (detectLayer == layerPlayer && (boat.Status == BoatStatus.Normal) && Vector2.Distance (points[points.Count - 1], hitPoint) < 0.5f) {
			boat.AnnoyingAnimation ();
		} else if (detectLayer == layerWater) {
			float impact = (Time.timeSinceLevelLoad - touchDownSinceTime) * 1f/accumulateTime * maxImpact;
			impact = Mathf.Clamp (impact, minRippleScale * maxImpact, maxImpact);

			Vector2 lastPoint = points [points.Count - 1];
			if (Vector2.Distance (mainCamera.ScreenToWorldPoint (lastPoint), mainCamera.ScreenToWorldPoint (hitPoint)) > spawnPathPointBetween) {
				points.Add (finger.ScreenPosition);
			}

			if (points.Count > flowLastRippleCount) {
				points.RemoveRange (0, points.Count - flowLastRippleCount);
			}
//			points.RemoveRange (0, points.Count - 1);

			for (int i = 0; i < points.Count; i++) {
				Vector2 p = points [i];
				float rotate = float.MaxValue;
				if (i > 0) {
					rotate = AngleBetweenVector2 (points [i - 1], p);
				}
				float scale = (float)(i) / points.Count * 0.5f + 0.5f;
				SplashWater (mainCamera.ScreenToWorldPoint(p), impact * 1, rotate);
			}
			FadeInControlTrail ();
		}
		ResetVariables ();
	}

	public void OnFingerDrag(Lean.LeanFinger finger) {
		if (dialogueTalkMode) return;

		Vector2 lastPoint = points [points.Count - 1];
		Vector2 point = finger.ScreenPosition;
		if (detectLayer == layerPlayer && boat.Status != BoatStatus.Flying) {
			if (Vector2.Distance (points[0], point) > 2f && boat.flyable) {
				boat.FlyToDirection (point - points[0]);
			}
		} else if (detectLayer == layerWater) {
			// 滑动水面
			if (Vector2.Distance (mainCamera.ScreenToWorldPoint (lastPoint), mainCamera.ScreenToWorldPoint (point)) > spawnPathPointBetween) {
				points.Add (point);
			}
			liveControlTrail.transform.position = ConvertControlTrailPosition(finger.ScreenPosition);
		}
	}

	public void OnFingerSet(Lean.LeanFinger finger) {
		if (dialogueTalkMode) return;

		if (detectLayer == layerWater) {
			Vector2 lastPoint = points [points.Count - 1];
			Vector2 point = finger.ScreenPosition;
			if (Vector2.Distance (mainCamera.ScreenToWorldPoint (lastPoint), mainCamera.ScreenToWorldPoint (point)) > spawnPathPointBetween) {
				points.Add (point);
			}
			liveControlTrail.transform.position = ConvertControlTrailPosition(finger.ScreenPosition);
		}
	}

	Vector3 ConvertControlTrailPosition(Vector2 pos) {
		Vector3 trailPos = mainCamera.ScreenToWorldPoint (pos);
		trailPos.z = liveControlTrail.transform.position.z;
		return trailPos;
	}

	private GameObject SpawnControlTrail() {
		if (controlTrail == null) return null;

		GameObject particles = (GameObject)Instantiate(controlTrail);
		particles.transform.position = new Vector3(0,particles.transform.position.y,0);

		ParticleSystem ps = particles.GetComponent<ParticleSystem>();
		if(ps != null)
		{
			ps.gameObject.AddComponent<CFX_AutoDestructShuriken>();
		}
		return particles;
	}

	private void FadeInControlTrail() {
		if (liveControlTrail == null) return;
		
		ParticleSystem ps = liveControlTrail.GetComponent<ParticleSystem>();
		ps.Stop ();
	}
}