using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCScript : MonoBehaviour {
	// Animation properties
	[SerializeField]
	public bool animationEnabled = false;
	[SerializeField]
	public int animationFPS = 10;
	[SerializeField]
	public bool animationLoop = true;
	[SerializeField]
	public List<Sprite> animationSprites = new List<Sprite>();
	[SerializeField]
	public SpriteAnimation2D tipDialogue;
	[SerializeField]
	public bool reTipble = false;

	new Collider2D collider;
	NPCAnimation npcAnimation;

	List<NPCTask> tasks;
	List<NPCTask> sendTasks;
	List<NPCTask> completerTasks;
	List<NPCTask> reactTaskChain = new List<NPCTask> ();

	private static int SortTasksByPriority(NPCTask task1, NPCTask task2) {
		if (task1.priority < task2.priority) {
			return -1;
		}
		return 1;
	}

	// Use this for initialization
	void Start () {
		collider = GetComponent<Collider2D> ();
		if (collider != null) {
			collider.isTrigger = true;
		}
		BroadcastSystem.defaultBoardcast.AddListener (NPCTask.NPCTaskStatusUpdateEvent, new BroadcastSystem.Callback (NotifyUpdateTaskStatus));
		BroadcastSystem.defaultBoardcast.AddListener (NPCDialogue.NPCDialugeFinishEvent, new BroadcastSystem.Callback (NotifyDialugeFinished));

		NPCTask[] npcTasks = GetComponentsInChildren<NPCTask> ();
		tasks = new List<NPCTask> (npcTasks);
		tasks.Sort (SortTasksByPriority);

		sendTasks = new List<NPCTask> ();
		completerTasks = new List<NPCTask> ();

		foreach (NPCTask t in tasks) {
			if (t.npcTaskMode == NPCTaskMode.Sender) {
				sendTasks.Add (t);
			} else {
				completerTasks.Add (t);
			}

			Dictionary<string, NPCTask> taskDict = NPCTaskData.sharedInstance.taskDict;
			taskDict.Add (t.taskName, t);
		}

		npcAnimation = new NPCAnimation();
		npcAnimation.spriteRenderer = GetComponent<SpriteRenderer> ();
		npcAnimation.enabled = true;

		PlayNPCAnimation (null);
	}

	void Update() {
		npcAnimation.Update ();
	}

	void PlayNPCAnimation(NPCTask task) {
		int fps = 0;
		bool loop = false;
		List<Sprite> sprites = null;

		if (task == null && animationEnabled) {
			fps = animationFPS;
			sprites = animationSprites;
			loop = animationLoop;
			//print ("播放默认动画!");
		} else if (task != null && task.animationEnabled == true) {
			fps = task.animationFPS;
			sprites = task.animationSprites;
			loop = task.animationLoop;
			//print ("播放动画:" + task.taskName);
		}

		if (sprites != null && sprites.Count > 0 && ((npcAnimation.IsPlaying && !sprites.Equals (npcAnimation.frames)) || !npcAnimation.IsPlaying)) {
			npcAnimation.fps = fps;
			npcAnimation.PlayAnimation (sprites, loop);
		}
	}

	NPCTaskStatus FindTaskStatus(string taskName) {
		Dictionary<string, NPCTask> taskDict = NPCTaskData.sharedInstance.taskDict;
		if (taskDict.ContainsKey (taskName)) {
			return  taskDict [taskName].status;
		}
		return NPCTaskStatus.NotStarted;
	}

	void NotifyUpdateTaskStatus (BroadcastInfo arg) {
		NPCTaskStatus status = (NPCTaskStatus)arg.info ["status"];
		string taskName = arg.info ["taskName"] as string;
	
		if (status == NPCTaskStatus.Done) {
			foreach (NPCTask t in sendTasks) {
				if (t.enabled && t.taskName == taskName) {
					//m_status = NPCStatus.MyTaskAlreadyFinished;
				}
			}
		}
	}

	void NotifyDialugeFinished (BroadcastInfo arg) {
		NPCDialogue dialogue = (NPCDialogue)arg.from;
		if (dialogue != null && dialogue.talkerNPC == this) {
			PlayNPCAnimation (null);

			Time.timeScale = 1;
		}

		if (reTipble && tipDialogue != null) {
			tipDialogue.enabled = true;
			tipDialogue.Reset ();
		}
	}

	// 跟NPC对话
	public void TalkTo() {
		Dictionary<string, NPCTask> taskDict = NPCTaskData.sharedInstance.taskDict;

		NPCTask reactTask = null;
		// 遍历是否有能完成的任务

		foreach (NPCTask t in completerTasks) {
			NPCTask toCompleteTask = null;
			if (taskDict.ContainsKey (t.receiveCompletionTask)) {
				toCompleteTask = taskDict[t.receiveCompletionTask];
			}

			if (toCompleteTask != null && toCompleteTask.status == NPCTaskStatus.Process) {
				toCompleteTask.status = NPCTaskStatus.Done;
				reactTask = t;
				break;
			}
		}

		// 遍历是否有能派发的任务
		if (reactTask == null) {
			foreach (NPCTask t in sendTasks) {
				if (t.status == NPCTaskStatus.NotStarted && (t.taskCondition.Length == 0 || FindTaskStatus(t.taskCondition) == NPCTaskStatus.Done)) {
					t.status = NPCTaskStatus.Process;
					reactTask = t;
					break;
				}
			}
		}

		if (reactTask != null) {
			reactTaskChain.Add (reactTask);
		}

		NPCTask interactTask = reactTask;
		int i = reactTaskChain.Count - 1;
		while (i >= 0 && interactTask == null) {
			NPCTask t = reactTaskChain [i];
			if (t.npcDialogue != null && t.npcTaskMode == NPCTaskMode.Sender) {
				interactTask = t;
			}
			i--;
		}

		// 对话
		if (interactTask != null) {
			//print ("talk about:" + interactTask.taskName);

			if (interactTask.npcDialogue != null) {
				interactTask.npcDialogue.talkerNPC = this;
				interactTask.npcDialogue.TalkDialogues ();
				Time.timeScale = 0f;
			}
			if (interactTask.animationEnabled) {
				PlayNPCAnimation (interactTask);
			}
		}


		if (tipDialogue != null) {
			if (tipDialogue.enabled) {
				tipDialogue.enabled = false;
				SpriteRenderer sr = tipDialogue.GetComponent<SpriteRenderer> ();
				sr.sprite = null;
			}
		}
	}
}
