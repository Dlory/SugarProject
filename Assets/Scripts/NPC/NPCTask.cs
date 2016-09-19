using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NPCTaskStatus {
	NotStarted = 0,
	Process = 1,
	Done = 2,
};

public enum NPCTaskMode {
	Sender = 0,
	Completer  = 1,
};

public class NPCTaskData : Object {
	public static readonly NPCTaskData sharedInstance = new NPCTaskData();
	public Dictionary<string, NPCTask> taskDict = new Dictionary<string, NPCTask>();
	private NPCTaskData(){}
}

public class NPCTask : MonoBehaviour {
	public static readonly string NPCTaskStatusUpdateEvent = "NPCTaskStatusUpdateEvent";
	public NPCTaskMode npcTaskMode = NPCTaskMode.Sender;
	public string taskName = "";
	public string taskCondition = "";
	public string receiveCompletionTask = "";
	public int priority = 0;
	public int reward = 0;

	[SerializeField]
	public NPCDialogue npcDialogue;

	// Animation properties
	public bool animationEnabled = false;
	public int animationFPS = 10;
	public bool animationLoop = true;
	[SerializeField]
	public List<Sprite> animationSprites = new List<Sprite>();

	NPCTaskStatus m_status = NPCTaskStatus.NotStarted;

	public NPCTaskStatus status {
		set {
			if (m_status != value) {
				m_status = value;
				BroadCastParam[] broadParams = new BroadCastParam[2];
				broadParams [0] = new BroadCastParam ("taskName", taskName);
				broadParams [1] = new BroadCastParam ("status", m_status);

				BroadcastSystem.defaultBoardcast.SendMessage (NPCTaskStatusUpdateEvent, null, broadParams);
			}
		}
		get {
			return m_status;
		}
	}

	void Start() {
	}
}
