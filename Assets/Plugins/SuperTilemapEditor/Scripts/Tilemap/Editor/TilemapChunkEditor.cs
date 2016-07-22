using UnityEngine;
using System.Collections;
using UnityEditor;
using CreativeSpore.SuperTilemapEditor;
using RailGun;

[CustomEditor(typeof(TilemapChunk))]
public class TilemapChunkEditor : Editor
{
//	private TilemapChunk targetChunk;
//	private Tilemap targetMap;
//
//	public override void OnInspectorGUI()
//	{
//		targetChunk = target as TilemapChunk;
//
//		targetMap = targetChunk.GetComponentInParent<Tilemap>();
//
//		base.OnInspectorGUI();
//
//		if(targetMap != null && targetMap.ColliderType == eColliderType._2D && targetMap.Collider2DType == e2DColliderType.PolygonCollider2D)
//		{
//			EditorGUILayout.BeginVertical("box");
//			float angle = EditorPrefs.GetFloat("FlatternPolygonColliderAngle",180.0f);
//			float distance = EditorPrefs.GetFloat("FlatternPolygonColliderDistance",0.1f);
//
//			angle = EditorGUILayout.Slider("Angle",angle,0,180);
//			distance = EditorGUILayout.FloatField("Distance",distance);
//
//			EditorPrefs.SetFloat("FlatternPolygonColliderAngle",angle);
//			EditorPrefs.SetFloat("FlatternPolygonColliderDistance",distance);
//
//			if(GUILayout.Button("Flattern Polygon Collider"))
//			{
//				FlatternPolygons(angle,distance);
//			}
//
//			EditorGUILayout.EndVertical();
//
//			if(GUILayout.Button("Combine Polygon Colliders"))
//			{
//				CombinePolygons();
//			}
//		}
//	}
//
//	private void FlatternPolygons(float angle,float distance)
//	{
//		PolygonCollider2D[] polygons = targetChunk.GetComponents<PolygonCollider2D>();
//		for(int i = 0;i < polygons.Length;i++)
//		{
//			Vector2[] flattern = Polygon2DCombiner.FlattenPoints(polygons[i].points,angle,distance);
//			polygons[i].points = flattern;
//		}
//	}
//
//	private void CombinePolygons()
//	{
//		PolygonCollider2D[] polygons = targetChunk.GetComponents<PolygonCollider2D>();
//		Vector2[][] v2 = new Vector2[polygons.Length][];
//		for(int i = 0;i < polygons.Length;i++)
//		{
//			v2[i] = polygons[i].points;
//		}
//
//		Vector2[][] after = Polygon2DCombiner.CombinePolygons(v2);
//		targetChunk.OutlinePoints = after;
//		GameObject obj = new GameObject("polygon_after");
//		for(int i = 0;i < after.Length;i++)
//		{
//			PolygonCollider2D collider = obj.AddComponent<PolygonCollider2D>();
//			collider.points = after[i];
//		}
//	}
}
