using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace RailGun
{
	public class RailGunEditorUtility  
	{
		/// <summary>
		/// 生成asset文件,路径从“Assets/”开始算
		/// </summary>
		public static ScriptableObject CreateAsset(System.Type type, string path)// where T : ScriptableObject 
		{
			if(!type.IsSubclassOf(typeof(ScriptableObject)))
			{
				Debug.LogError("You want to create a scriptable asset,but the type you give is not SciptableObject!");
				return null;
			}

			ScriptableObject asset = ScriptableObject.CreateInstance(type);
			AssetDatabase.CreateAsset(asset, path);
			AssetDatabase.SaveAssets();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;

			return asset;
		}

		public static T CreateAsset<T>(string path) where T : ScriptableObject 
		{
			return CreateAsset(typeof(T),path) as T;
		}

		public static void CreateAssetAtCurrentDir<T>(String name) where T : ScriptableObject
		{
			string dir = "Assets/";

			UnityEngine.Object selected = Selection.activeObject;
			if (selected != null)
			{
				string assetDir = AssetDatabase.GetAssetPath(selected.GetInstanceID());
				if (assetDir.Length > 0 && Directory.Exists(assetDir))
					dir = assetDir + "/";
			}

			CreateAsset<T>(dir + name);
		}

		/// <summary>
		/// Unity 4.5+ makes it possible to hide the move tool.
		/// </summary>
		public static void HideMoveTool (bool hide)
		{
			#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3
			UnityEditor.Tools.hidden = hide && (UnityEditor.Tools.current == UnityEditor.Tool.Move);
			#endif
		}

		/// <summary>
		/// 给定坐标，画出对应的边框,世界坐标
		/// </summary>
		public static void DrawBounds(Rect rect,Color color)
		{
			Handles.color = color;

			Handles.DrawLine(new Vector3(rect.xMin,rect.yMin),new Vector3(rect.xMin,rect.yMax));
			Handles.DrawLine(new Vector3(rect.xMin,rect.yMin),new Vector3(rect.xMax,rect.yMin));
			Handles.DrawLine(new Vector3(rect.xMax,rect.yMax),new Vector3(rect.xMax,rect.yMin));
			Handles.DrawLine(new Vector3(rect.xMax,rect.yMax),new Vector3(rect.xMin,rect.yMax));
		}

		/// <summary>
		/// 给定鼠标screen point，返回在scene view里指到的世界坐标点
		/// </summary>
		public static Vector3 GetSceneMousePosInWorld(Vector2 mousePosition)
		{
			Vector3 v = HandleUtility.GUIPointToWorldRay(mousePosition).origin;//camera.ScreenToWorldPoint(new Vector2(mousePosition.x,camera.pixelHeight - mousePosition.y));
			//	Vector3 v = camera.ScreenPointToRay(new Vector2(mousePosition.x,camera.pixelHeight - mousePosition.y)).origin;

			return v;
		}

		/// <summary>
		/// 通过给定世界坐标及大小，返回正方形的四个顶点，左下、左上、右上、右下
		/// </summary>
		private static Vector3[] GetRect(Vector2 center, Vector2 size)
		{
			Vector3[] mouseRectCorner = new Vector3[4];

			float halfX = size.x / 2;
			float halfY = size.y / 2;

			//bottom left
			mouseRectCorner[0] = center + new Vector2(-halfX, -halfY);
			//top left
			mouseRectCorner[1] = center + new Vector2(-halfX, halfY );
			//top right
			mouseRectCorner[2] = center + new Vector2(halfX,halfY);
			//bottom right
			mouseRectCorner[3] = center + new Vector2(halfX, -halfY);

			return mouseRectCorner;
		}

		/// <summary>
		/// Unity 4.3 changed the way LookLikeControls works.
		/// </summary>
		static public void SetLabelWidth (float width)
		{
			#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			EditorGUIUtility.LookLikeControls(width);
			#else
			EditorGUIUtility.labelWidth = width;
			#endif
		}
		
		/// <summary>
		/// Create an undo point for the specified objects.
		/// </summary>
		static public void RegisterUndo (string name, params UnityEngine.Object[] objects)
		{
			if (objects != null && objects.Length > 0)
			{
				#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
				UnityEditor.Undo.RegisterUndo(objects, name);
				#else
				UnityEditor.Undo.RecordObjects(objects, name);
				#endif
				foreach (UnityEngine.Object obj in objects)
				{
					if (obj == null) continue;
					EditorUtility.SetDirty(obj);
				}
			}
		}

		/// <summary>
		/// Draw a distinctly different looking header label
		/// </summary>
		
		static public bool DrawHeader (string text) { return DrawHeader(text, text, false); }
		
		/// <summary>
		/// Draw a distinctly different looking header label
		/// </summary>
		
		static public bool DrawHeader (string text, string key) { return DrawHeader(text, key, false); }
		
		/// <summary>
		/// Draw a distinctly different looking header label
		/// </summary>
		
		static public bool DrawHeader (string text, bool forceOn) { return DrawHeader(text, text, forceOn); }
		
		/// <summary>
		/// Draw a distinctly different looking header label
		/// </summary>
		
		static public bool DrawHeader (string text, string key, bool forceOn)
		{
			bool state = EditorPrefs.GetBool(key, false);
			
			GUILayout.Space(3f);
			if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
			GUILayout.BeginHorizontal();
			GUILayout.Space(3f);
			
			GUI.changed = false;
			#if UNITY_3_5
			if (state) text = "\u25B2 " + text;
			else text = "\u25BC " + text;
			if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
			#else
			text = "<b><size=11>" + text + "</size></b>";
			if (state) text = "\u25B2 " + text;
			else text = "\u25BC " + text;
			if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
			#endif
			if (GUI.changed) EditorPrefs.SetBool(key, state);
			
			GUILayout.Space(2f);
			GUILayout.EndHorizontal();
			GUI.backgroundColor = Color.white;
			if (!forceOn && !state) GUILayout.Space(3f);
			return state;
		}

		static public bool DrawHeader (ref Rect position,string text, string key, bool forceOn,float height)
		{
			bool state = EditorPrefs.GetBool(key, false);

			if (!forceOn && !state) 
				GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);

			GUI.changed = false;
			text = "<b><size=11>" + text + "</size></b>";
			if (state) 
				text = "\u25B2 " + text;
			else 
				text = "\u25BC " + text;
			if (!GUI.Toggle(new Rect(position.x,position.y,position.width,height),true,text,"dragtab")) 
				state = !state;
			
			if (GUI.changed) 
				EditorPrefs.SetBool(key, state);

			GUI.backgroundColor = Color.white;

			position.y += height;
			position.height -= height;

			return state;
		}

		static public void DrawTitle (string text)
		{
//			GUILayout.Space(3f);
//
//			GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
//
//			GUILayout.BeginHorizontal();
//			GUILayout.Space(3f);
//			text = "<b><size=11>" + text + "</size></b>";
////			GUILayout.Label(text);
//			GUILayout.Toggle(true,text,"dragtab",GUILayout.MinWidth(20f));
//
//			
//
//			GUILayout.Space(2f);
//			GUILayout.EndHorizontal();
//
//			GUI.backgroundColor = Color.white;
			EditorGUILayout.LabelField(text,EditorStyles.boldLabel);
		}

		//
		/// <summary>
		/// Begin drawing the content area.
		/// </summary>
		
		static public void BeginContents ()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space(4f);
			EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
			GUILayout.BeginVertical();
			GUILayout.Space(2f);
		}
		
		/// <summary>
		/// End drawing the content area.
		/// </summary>
		
		static public void EndContents ()
		{
			GUILayout.Space(3f);
			GUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(3f);
			GUILayout.EndHorizontal();
			GUILayout.Space(3f);
		}

		/// <summary>
		/// AssetDatabase获得的路径是Assets/xxx/xx
		/// Application.dataPath获得的是xxx/xxx/Assets
		/// 需要转换一下才能拼在一起
		/// </summary>
		public static string GetDiskFullPathFromAssetPath(string path) 
		{
			int subLen = Application.dataPath.Length - 6;//把Asset去掉
			string fullPath = Application.dataPath.Substring(0,subLen) + path;
			return fullPath;
		}

		public static string GetAssetPathFromDiskPath(string path)
		{
			int subLen = Application.dataPath.Length - 6;
			return path.Substring(subLen);
		}

		public static string GetAssetDirectory(string assetPath)
		{
			string[] split = assetPath.Split('/');
			return assetPath.Remove(assetPath.Length - split[split.Length - 1].Length - 1);
		}

		public static TextAsset WriteTextAsset(string assetPath,string text)
		{
			string diskPath = GetDiskFullPathFromAssetPath(assetPath);
			File.WriteAllText(diskPath,text);
//			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();//一定要刷新，否则用LoadAsset加载不到
			TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
			return textAsset;
		}

		public static T GetSerializedPropertyObjectValue<T>(SerializedProperty property)
		{
			UnityEngine.Object targetObject = property.serializedObject.targetObject;
			System.Type targetObjectClassType = targetObject.GetType();
			System.Reflection.FieldInfo field = targetObjectClassType.GetField(property.propertyPath);
			if (field != null)
			{
				T value = (T)field.GetValue(targetObject);
				return value;
			}

			return default(T);
		}
	}
}

