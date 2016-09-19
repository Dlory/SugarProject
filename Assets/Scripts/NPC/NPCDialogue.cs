using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCDialogue : MonoBehaviour {
	public static readonly string NPCDialugeNextEvent = "NPCDialugeNextEvent";
	public static readonly string NPCDialugeFinishEvent = "NPCDialugeFinishEvent";

	public List<Sprite> dialogues = new List<Sprite>();

	[HideInInspector]
	public NPCScript talkerNPC = null;

	SpriteRenderer spriteRenderer;
	GameControl gameControl;
	bool isDialoguing = false;
	int dialogueIndex = -1;

	public void TalkDialogues() {		
		if (dialogues.Count > 0) {
			if (dialogueIndex < 0) {
				gameControl.dialogueTalkMode = true;
				dialogueIndex = 0;
			} else {
				dialogueIndex++;
				if (dialogueIndex >= dialogues.Count) {
					gameControl.dialogueTalkMode = false;
					dialogueIndex = -1;
					spriteRenderer.sprite = null;
					isDialoguing = false;

					BroadcastSystem.defaultBoardcast.SendMessage (NPCDialugeFinishEvent, this, null);
				}
			}
		}

		if (dialogueIndex >= 0) {
			isDialoguing = true;
			spriteRenderer.sprite = dialogues[dialogueIndex];
		}
	}

	void NotifyTalkDialogues(BroadcastInfo arg) {
		if (isDialoguing) {
			TalkDialogues ();
		}
	}

	// Use this for initialization
	void Start () {
		spriteRenderer = GetComponent<SpriteRenderer> ();
		spriteRenderer.sprite = null;
		GameObject gestureObject = GameObject.Find ("GameGUI");
		gameControl = gestureObject.GetComponent<GameControl> ();

		BroadcastSystem.defaultBoardcast.AddListener (NPCDialugeNextEvent, new BroadcastSystem.Callback (NotifyTalkDialogues));
	}
}
