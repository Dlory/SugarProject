using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StoneDynamicFrontBack : MonoBehaviour {
	GameObject player;
	GameObject[] objects;
	SpriteRenderer[] spriteRenderers;
	BoatScript boatScript;
	SpriteRenderer myRender;
	int breakFPS = 5;  //每100毫秒检测一次
	int m_breakFPS = 0;
	Dictionary<SpriteRenderer, int> spriteSrcOrderDict = new Dictionary<SpriteRenderer, int>();

	void Start () {
		player = GameObject.Find ("Player");
		objects = GameObject.FindGameObjectsWithTag ("AutoFixFrontBack");
		boatScript = player.GetComponent<BoatScript> ();

		spriteRenderers = new SpriteRenderer[objects.Length];
		for (int i = 0; i < objects.Length; i++) {
			SpriteRenderer sr = objects[i].GetComponent<SpriteRenderer> ();
			if (sr != null) {
				spriteRenderers [i] = sr;
				spriteSrcOrderDict.Add (sr, sr.sortingOrder);
			}
		}
	}

	void FixedUpdate () {
		m_breakFPS ++;
		if (m_breakFPS > breakFPS) {
			foreach (SpriteRenderer sr in spriteRenderers) {
				if (sr != null) {
					if (sr.gameObject.transform.position.y < player.transform.position.y && boatScript.Status != BoatStatus.Flying) {
						sr.sortingOrder = 100;
					} else {
						sr.sortingOrder = spriteSrcOrderDict[sr];
					}
				}
			}
			m_breakFPS = 0;
		}
	}
}
