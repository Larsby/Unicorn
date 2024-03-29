using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyMaker : MonoBehaviour {

	public int nofCredits = 1;
	public bool repeatedReward = false;
	public float repeatedDelay = 1f;

	private bool wasHit = false;
	private float repeatTimer = 0;

	void Start () {
	}
	
	void Update () {
		if (repeatTimer > 0)
			repeatTimer -= Time.deltaTime;
	}

	void OnCollisionEnter(Collision collision)
	{
		OnTriggerEnter (collision.collider);
	}

	void OnTriggerEnter(Collider collider) {

		if (wasHit && repeatedReward == false)
			return;
		if (repeatTimer > 0)
			return;

		GameObject findMe = GameUtil.FindParentWithTag (collider.gameObject, "Player");

		if (findMe != null && !wasHit) {

			repeatTimer = repeatedDelay;
			wasHit = true;

			StaticManager.AddTemporaryCredits (nofCredits);
		}
	}

}
