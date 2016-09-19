using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(NPCScript))]
public class NPCScriptEditor : Editor 
{
	NPCScript m_target;
	SerializedObject m_object;

	public void OnEnable() {
		m_target = (NPCScript)target;
		if (target != null) {
			m_object = new SerializedObject (target);
		}
	}

	public override void OnInspectorGUI() {
		if (m_target == null || m_object == null) {
			return;
		}

		m_object.Update ();

		//EditorGUILayout.LabelField (new GUIContent("状态 ->	" + m_target.status.ToString(), "NPC状态"));

		EditorGUILayout.HelpBox ("NPC动画", MessageType.None, true);
		m_target.animationEnabled = EditorGUILayout.Toggle ("开关", m_target.animationEnabled);

		if (m_target.animationEnabled) {
			m_target.animationFPS = EditorGUILayout.IntField (
				"FPS",
				m_target.animationFPS);
			m_target.animationLoop = EditorGUILayout.Toggle (
				"循环播放",
				m_target.animationLoop);

			EditorGUILayout.PropertyField (
				m_object.FindProperty ("animationSprites"), 
				new GUIContent ("动画帧", "动画帧"),
				true);
		}

		EditorGUILayout.HelpBox ("对话", MessageType.None, true);
		EditorGUILayout.PropertyField (
			m_object.FindProperty ("tipDialogue"), 
			new GUIContent ("提示动画", "TipDialogue"),
			false);
		if (m_target.tipDialogue != null) {
			m_target.reTipble = EditorGUILayout.Toggle ("对话完后继续提示", m_target.reTipble);
		}

		m_object.ApplyModifiedProperties ();
	}
}
