using UnityEngine;
using System.Collections;

public enum CrocodileStatus {
	Idle = 0,
	Appear,
	Disappear,
	Wink,
	Interfere,
	Crush,
}

public class Crocodile : MonoBehaviour {
	public Animator animator;
	public GameObject Fragment;
	public GameObject InterferenceRippleSprite;
	public CrocodileStatus currentStatus {
		get {
			if(!animator.IsInTransition(0)) {
				string[] actions = {"CrocodileIdle", "CrocodileAppear", "CrocodileDisappear", "CrocodileWink", "CrocodileInterfere", "CrocodileCrush"};
				for (int i = 0; i < actions.Length; i++) {
					if (animator.GetCurrentAnimatorStateInfo (0).IsName (actions[i])) {
						return (CrocodileStatus)i;
					}
				}
			}
			return CrocodileStatus.Idle;
		}
	}

	string ActionForStatus(CrocodileStatus status) {
		string[] actions = {null, "Appear", "Disappear", "Wink", "Interfere", "Crush"};
		return actions [(int)status];
	}

	public void PlayAnimation(CrocodileStatus status) {
		animator.SetTrigger (ActionForStatus (status));
	}

	void Start () {
		if (Fragment != null) {
			Fragment.SetActive (false);
		}
		StartCoroutine (FreeTimeLoop ());
	}

	IEnumerator FreeTimeLoop() {
		while (true) {
			yield return new WaitForSeconds (Random.Range(3f, 10f));

			if (this.currentStatus == CrocodileStatus.Idle) {
				CrocodileStatus toStatus = Random.Range(-1, 1) >= 0 ? CrocodileStatus.Disappear : CrocodileStatus.Wink;
				PlayAnimation (toStatus);
			} else if (this.currentStatus == CrocodileStatus.Disappear && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.98f) {
				PlayAnimation (CrocodileStatus.Appear);
			}
		}
	}

	public void OnCollisionEnter2D(Collision2D col){
		if (col.gameObject.tag == "Player") {
			if (this.currentStatus != CrocodileStatus.Interfere) { //播放鳄鱼被干扰的动画，还需添加产生干扰波的逻辑
				PlayAnimation (CrocodileStatus.Interfere);

				Vector2 contactPoint = col.contacts [0].point;
				GenerateInterferenceRipple(contactPoint, Random.Range(0.8f, 1.3f));
				GenerateInterferenceRipple(new Vector2(contactPoint.x + Random.Range(0.5f, 1f) * (Random.Range(-1, 1) >= 0 ? 1 : -1),
					contactPoint.y + Random.Range(0.5f, 1f) * (Random.Range(-1, 1) >= 0 ? 1 : -1)), Random.Range(0.8f, 1.3f) * 0.8f);
			}


			/*            仓鼠飞起来后落地砸到鳄鱼后，鳄鱼播放被砸动画并吐出拼图。还需添加触发此逻辑的条件，和产生相应的干扰波
			animator.SetTrigger("Crush");
			if (!Fragment.active) {
				Fragment.SetActive (true);
				FragmentAnimator.SetTrigger ("Spit");
			}
			*/
		}
	}

	void GenerateInterferenceRipple(Vector2 point, float scale) {
		GameObject obj = Instantiate (InterferenceRippleSprite, new Vector3(point.x, point.y, this.transform.position.z), transform.rotation) as GameObject;
		RippleScript interferenceRippleScript = obj.GetComponent<RippleScript> ();
		interferenceRippleScript.RippleScale = scale;
		interferenceRippleScript.isInterferenceRipple = true;
	}

}
