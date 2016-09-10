using UnityEngine;
using System.Collections;

public class GameGUI : MonoBehaviour {
	public uint CollectCount = 0;
	public uint TotalCount = 0;
	public Texture2D EnergyBarTexture;
	public Texture2D EnergyBarBGTexture;
	public Rect EnergyBarRect = new Rect(20, 20, 200, 50);
	public float scorePerPower = 100f;
	public float score = 0f;
	BoatEnergy boatEnergy;

	void Start() {
		BroadcastSystem.defaultBoardcast.AddListener (CollectorScript.CollectCollectionEvent, new BroadcastSystem.Callback (CollectionEvent));
		boatEnergy = GetComponent<BoatEnergy> ();
	}

	void CollectionEvent(BroadcastInfo arg) {
		CollectionScript collection = arg.from as CollectionScript;
		score += collection.score;

		if (collection.useScoreToExpendMagicCircle == false) {
			CollectCount += 1;
		}
	}

	void OnGUI() {
		GUI.Label (new Rect (250, 20, 200, 50), "碎片收集：" + CollectCount + "/" + TotalCount);
		GUI.Label (new Rect (500, 20, 200, 50), "分数：" + score);

		if (boatEnergy && EnergyBarTexture && EnergyBarBGTexture) {
			Rect rect = new Rect (EnergyBarRect.x, EnergyBarRect.y, boatEnergy.Energy/boatEnergy.MaxEnergy * EnergyBarRect.width, EnergyBarRect.height);
			GUI.DrawTexture (EnergyBarRect, EnergyBarBGTexture);
			GUI.DrawTexture (rect, EnergyBarTexture);
		}
	}
}