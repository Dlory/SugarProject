using UnityEngine;
using System.Collections;

public class GameGUI : MonoBehaviour {
	public static readonly string FragmenCollectEvent = "FragmenCollectEvent";
	public uint CollectCount = 0;
	public uint TotalCount = 0;
	public Texture2D EnergyBarTexture;
	public Texture2D EnergyBarBGTexture;
	public Rect EnergyBarRect = new Rect(20, 20, 200, 50);

	BoatEnergy boatEnergy;

	void Start() {
		BroadcastSystem.defaultBoardcast.AddListener (FragmenCollectEvent, new BroadcastSystem.Callback (UpdateFragmentCount));
		boatEnergy = GetComponent<BoatEnergy> ();
	}

	void UpdateFragmentCount (BroadcastInfo arg) {
		CollectCount += 1;
	}

	void OnGUI() {
		GUI.Label (new Rect (250, 20, 200, 50), "碎片收集：" + CollectCount + "/" + TotalCount);

		if (boatEnergy && EnergyBarTexture && EnergyBarBGTexture) {
			Rect rect = new Rect (EnergyBarRect.x, EnergyBarRect.y, boatEnergy.Energy/boatEnergy.MaxEnergy * EnergyBarRect.width, EnergyBarRect.height);
			GUI.DrawTexture (EnergyBarRect, EnergyBarBGTexture);
			GUI.DrawTexture (rect, EnergyBarTexture);
		}
	}
}