using UnityEngine;
using System.Collections;

public class CollectionScript : MonoBehaviour {
	public int score = 0;
	public bool useScoreToExpendMagicCircle = false;
	public string objName;
	public GameObject shadowObject;

	[HideInInspector]
	public float altitude;
	public bool collected = false;

	void Start () {
		if (shadowObject != null) {

			//			Vector3 pos = paraPos + Quaternion.AngleAxis (-45, Vector3.right) * new Vector3 (0, -flyingHeight, 0);
			altitude = Mathf.Max(this.transform.position.y - shadowObject.transform.position.y, 0);
		}
	}
}
