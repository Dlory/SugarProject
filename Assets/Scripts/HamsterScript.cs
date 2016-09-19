using UnityEngine;
using System.Collections;

public class HamsterScript : MonoBehaviour {
	public static readonly string CollectCollectionEvent = "CollectCollectionEvent";
	MagicCircle magicCircle;
	BoatScript boat;
	Rigidbody2D m_rigidbody;
	public float rotation;
	public float rotationMultiplier = 1f;

	void Start () {
		magicCircle = this.transform.parent.GetComponentInChildren<MagicCircle> ();
		boat = this.transform.parent.GetComponent<BoatScript> ();
		m_rigidbody = GetComponentInParent<Rigidbody2D> ();
	}
	
	void OnTriggerStay2D(Collider2D other) {
		CollectionScript collection = other.GetComponent<CollectionScript> ();

		if (collection != null && !collection.collected) {
			float altitude = boat.flyingAltitude;
			bool collectable = (Mathf.Abs(collection.altitude - altitude) < 0.5f && collection.altitude > 0 && altitude > 0) || (altitude == collection.altitude);

			if (collectable) {
				float score = collection.score;
				if (collection.useScoreToExpendMagicCircle && magicCircle.scale < magicCircle.maxScale) {
					magicCircle.scale = Mathf.Min (magicCircle.scale + score * magicCircle.expandScalePerScore, magicCircle.maxScale);
					collection.score = 0;
				}
				collection.collected = true;

				BroadcastSystem.defaultBoardcast.SendMessage (CollectCollectionEvent, collection, null);
				//Destroy (other.gameObject);
			}
		}
	}

	void Update() {
		if (Vector2.Distance (m_rigidbody.velocity, Vector2.zero) > 0.1f) {
			this.transform.eulerAngles = new Vector3 (0, 0, rotation * rotationMultiplier);
		} else {
			this.transform.eulerAngles = Vector3.zero;
		}
	}
}
