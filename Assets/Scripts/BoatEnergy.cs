using UnityEngine;
using System.Collections;

public class BoatEnergy : MonoBehaviour {
	public float Energy = 0;
	public float MaxEnergy = 100;
	public float EnergyRecoverDelay = 1;
	public float EnergyRecoverPerSecond = 10;

	float LastCastTime = 0;

	// Use this for initialization
	void Start () {
		Energy = MaxEnergy;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (Energy < MaxEnergy && Time.realtimeSinceStartup - LastCastTime >= EnergyRecoverDelay) { // 恢复能量
			Energy += EnergyRecoverPerSecond * Time.deltaTime;
			Energy = Mathf.Clamp (Energy, 0, MaxEnergy);
		}
	}

	public bool CastEnergy(float value) {
		if (value < 0 || Energy <= 0) {
			return false;
		}
		Energy -= value;
		Energy = Mathf.Max (Energy, 0);
		LastCastTime = Time.realtimeSinceStartup;

		return true;
	}
}
