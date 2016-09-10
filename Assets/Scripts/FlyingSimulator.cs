using UnityEngine;
using System.Collections;

public class FlyingSimulator : MonoBehaviour {
	public bool flying = false;
	Rigidbody RB;

	// Use this for initialization
	void Start () {
		RB = GetComponent<Rigidbody> ();
	}

	// Update is called once per frame
	void Update () {
		if (transform.position.y < 0) {
			flying = false;
			RB.useGravity = false;
			RB.velocity = Vector3.zero;
			transform.position = new Vector3(transform.position.x, 0, transform.position.z);
		}
	}

	/// <summary>
	/// 模拟向右上抛，然后boat根据抛物线坐标做旋转来模拟任意方向的抛物线样子
	/// </summary>
	/// <param name="position">Position.</param>
	/// <param name="force">Force.</param>
	public void SimulateParabola(Vector2 ParabolaSimulateForce) {
		Vector3 force = new Vector3(ParabolaSimulateForce.x, ParabolaSimulateForce.y, 0);
		flying = true;

		gameObject.transform.position = Vector2.zero;
		RB.velocity = Vector3.zero;
		RB.useGravity = true;
		RB.AddForce (force, ForceMode.Impulse);
	}

	public float TopmostHeightForParabola(float forceY) {
		return 11f / 10f * forceY;
	}
}
