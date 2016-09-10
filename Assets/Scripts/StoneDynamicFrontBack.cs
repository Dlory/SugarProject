using UnityEngine;
using System.Collections;

public class StoneDynamicFrontBack : MonoBehaviour {
	GameObject player;
	GameObject[] stones;
	SpriteRenderer[] spriteRenderers;
	BoatScript boatScript;
	SpriteRenderer myRender;
	int breakFPS = 5;  //每100毫秒检测一次
	int m_breakFPS = 0;

	void Start () {
		player = GameObject.Find ("Player");
		stones = GameObject.FindGameObjectsWithTag ("Stone");

		boatScript = player.GetComponent<BoatScript> ();

		spriteRenderers = new SpriteRenderer[stones.Length];
		for (int i = 0; i < stones.Length; i++) {
			SpriteRenderer sr = stones[i].GetComponent<SpriteRenderer> ();
			if (sr != null) {
				spriteRenderers [i] = sr;
			}
		}
	}

	void FixedUpdate () {
		m_breakFPS ++;
		if (m_breakFPS > breakFPS) {
			foreach (SpriteRenderer sr in spriteRenderers) {
				if (sr != null) {
					if (sr.gameObject.transform.position.y < player.transform.position.y && boatScript.Status != BoatStatus.Flying) {
						sr.sortingOrder = 2;
					} else {
						sr.sortingOrder = 0;
					}
				}
			}
			m_breakFPS = 0;
		}
	}
}
