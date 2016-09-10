using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor (typeof(NPCTask))]
public class NPCTaskEditor : Editor
{
	NPCTask m_target;
	SerializedObject m_object;

	public void OnEnable() {
		m_target = (NPCTask)target;
		if (target != null) {
			m_object = new SerializedObject (target);
		}
	}

	public override void OnInspectorGUI() {
		if (m_target == null || m_object == null) {
			return;
		}

		m_object.Update ();

		EditorGUILayout.HelpBox ("NPC任务", MessageType.None, true);

		m_target.priority = Mathf.Clamp (EditorGUILayout.IntField (
			new GUIContent ("优先级", "0为最高，数字越大优先级越低"),
			m_target.priority
		), 0, int.MaxValue);

		m_target.npcTaskMode = (NPCTaskMode)EditorGUILayout.EnumPopup (
			new GUIContent ("模式", "任务发送/任务完成"), 
			m_target.npcTaskMode);

		if (m_target.npcTaskMode == NPCTaskMode.Sender) {
			m_target.taskCondition = EditorGUILayout.TextField (
				new GUIContent ("前置任务", "该NPC发送任务需要你完成的前置任务"),
				m_target.taskCondition
			);
			m_target.taskName = EditorGUILayout.TextField (
				new GUIContent ("任务名", "该NPC发送的任务名"),
				m_target.taskName
			);
		} else {
			m_target.receiveCompletionTask = EditorGUILayout.TextField (
				new GUIContent ("完成任务名", "该NPC能完成的任务"),
				m_target.receiveCompletionTask
			);
		}

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
			m_object.FindProperty ("npcDialogue"), 
			new GUIContent ("NPCDialogue", "NPCDialogue"),
			false);

		m_object.ApplyModifiedProperties ();
	}
}