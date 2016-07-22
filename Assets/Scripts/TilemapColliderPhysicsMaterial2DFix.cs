using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CreativeSpore.SuperTilemapEditor;

public class TilemapColliderPhysicsMaterial2DFix : MonoBehaviour {
	public PhysicsMaterial2D PhysicsMaterial2D;

	void Start () {
		Tilemap tilemap = GetComponent<Tilemap> ();
		if (tilemap != null && PhysicsMaterial2D != null) {
			System.Type collider2DType = tilemap.Collider2DType == e2DColliderType.EdgeCollider2D ? typeof(EdgeCollider2D) : typeof(PolygonCollider2D);
			List<TilemapChunk> chunkList = new List<TilemapChunk>(transform.childCount);

			for (int i = 0; i < transform.childCount; ++i) {
				TilemapChunk chunk = transform.GetChild (i).GetComponent<TilemapChunk> ();
				Component[] aColliders2D = chunk.GetComponents(collider2DType); 
				foreach(Collider2D collider2D in aColliders2D) {
					collider2D.sharedMaterial = PhysicsMaterial2D;
				}
			}
		}
	}
}
