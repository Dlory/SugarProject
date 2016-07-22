using UnityEngine;
using System.Collections;

public class GameGUI : MonoBehaviour {
	public static readonly string FragmenCollectEvent = "FragmenCollectEvent";

	public uint CollectCount = 0;
	public uint TotalCount = 0;

	void Start() {
		BroadcastSystem.defaultBoardcast.AddListener (FragmenCollectEvent, new BroadcastSystem.Callback (UpdateFragmentCount));
	}

	void UpdateFragmentCount (BroadcastInfo arg) {
		CollectCount += 1;
	}

	void OnGUI() {
		GUI.Label (new Rect (20, 20, 200, 50), "碎片收集：" + CollectCount + "/" + TotalCount);
	}
}