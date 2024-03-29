using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayAnimOnTouch : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void PlayAnim() {
		Animator anim = GetComponent<Animator> ();
		AnimationClip [] clips = anim.runtimeAnimatorController.animationClips;
		anim.Play (clips [0].name, -1, 0);
	}
}
