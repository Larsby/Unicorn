using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardNotifyer : MonoBehaviour {
	public bowAndArrow manager;
	// Use this for initialization
	void Start () {
		manager = 	GameObject.FindGameObjectWithTag ("Bow").GetComponent<bowAndArrow> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag.Equals( "Arrow")) {
			manager.SetReward ();
			PlayRandomSound player = GetComponent<PlayRandomSound> ();
			if (player != null) {
				player.Play ();
			}
			Destroy (gameObject,2.0f);
			GetComponent<SpriteRenderer> ().enabled = false;
		}
	}
}
