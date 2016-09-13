using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MagicCircle : MonoBehaviour {
	public int collectFish {
		get {
			int count = 0;
			foreach (FishBehaviourScript fish in fishes) {
				if (fish.isFlllowInMagicCircle) count += 1;
			}
			return count;}
	}
	public float expandScalePerScore = 0.01f;
	public float maxScale = 4f;
	public float scale {
		set {
			m_preScale = m_scale;
			m_scale = value;
		}
		get {
			return m_scale;
		}
	}

	float duration = 0.2f;
	float m_preScale = 0f;
	float m_scale = 1f;
	float m_sprtieSize;
	List<FishBehaviourScript> fishes = new List<FishBehaviourScript>();

	void Start() {
		SpriteRenderer renderer = GetComponent<SpriteRenderer> ();
		m_sprtieSize = Mathf.Max(renderer.sprite.textureRect.width, renderer.sprite.textureRect.height) / renderer.sprite.pixelsPerUnit;
		this.scale = scale;
	}

	void Update() {
		if (gameObject.transform.localScale.x != scale) {
			bool increase = scale > gameObject.transform.localScale.x;
			float step = (scale - m_preScale) / duration;
			float value = gameObject.transform.localScale.x + step * Time.deltaTime;
			value = increase ? Mathf.Clamp (value, m_preScale, scale) : Mathf.Clamp (value, scale, m_preScale);

			gameObject.transform.localScale = new Vector3 (value, value, 1f);
		}
	}

	public float Radius() {
		return m_sprtieSize / 2f * this.scale;
	}

	public void EnterFish(FishBehaviourScript fish) {
		if (!fishes.Contains (fish)) {
			fishes.Add (fish);
		}
	}

	public void ExitFish(FishBehaviourScript fish) {
		if (fishes.Contains (fish)) {
			fishes.Remove (fish);
		}
	}
}