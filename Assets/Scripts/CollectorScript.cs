using UnityEngine;
using System.Collections;

public class CollectorScript : MonoBehaviour {
	public static readonly string CollectCollectionEvent = "CollectCollectionEvent";
	MagicCircle magicCircle;
	BoatScript boat;

	void Start () {
		magicCircle = this.transform.parent.GetComponentInChildren<MagicCircle> ();
		boat = this.transform.parent.GetComponent<BoatScript> ();
	}
	
	void OnTriggerStay2D(Collider2D other) {
		CollectionScript collection = other.GetComponent<CollectionScript> ();

		if (collection != null && !collection.collected) {
			float altitude = boat.flayingAltitude;
			bool collectable = (Mathf.Abs(collection.altitude - altitude) < 0.5f && collection.altitude > 0 && altitude > 0) || (altitude == collection.altitude);

			if (collectable) {
				float score = collection.score;
				if (collection.useScoreToExpendMagicCircle && magicCircle.scale < magicCircle.maxScale) {
					magicCircle.scale = Mathf.Min (magicCircle.scale + score * magicCircle.expandScalePerScore, magicCircle.maxScale);
					collection.score = 0;
				}
				collection.collected = true;

				BroadcastSystem.defaultBoardcast.SendMessage (CollectCollectionEvent, collection, null);
				Destroy (other.gameObject);
			}
		}
	}

}
