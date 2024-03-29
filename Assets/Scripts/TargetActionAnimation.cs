using System;
using UnityEngine;

public class TargetActionAnimation:MonoBehaviour,ITargetAction
{

	enum ClipIndex {
		IDLE =0,
		BOUNCE=1,
		BOUNCE_HARD=2
	}
	ClipIndex clip;
	private GameObject target;
	LevelDifficulty level;
	private bool setup = false;
	[Range(0.0f, 3.0f)]
	public int index;
	public bool useRandom = false;
	private int oldIndex;
	private bool play = true;
	public TargetActionAnimation ()
	{
		
	}
	LevelDifficulty GetLevelDifficulty (int idx)
	{
		switch (idx) {
		case 0:
			return LevelDifficulty.BEGINNER;
		case 1:
			return LevelDifficulty.INTERMEDIATE;
		case 2:
			return LevelDifficulty.HARD;
		case 3:
			return LevelDifficulty.INSANE;

		}
		return  LevelDifficulty.BEGINNER;
		
	}

	ClipIndex GetClip(LevelDifficulty level) {
		switch(level){
		case LevelDifficulty.BEGINNER:
		case LevelDifficulty.INTERMEDIATE:
			return ClipIndex.IDLE;
		case LevelDifficulty.HARD:
			return ClipIndex.BOUNCE;
		case LevelDifficulty.INSANE:
			return ClipIndex.BOUNCE_HARD;
		default:
			return  ClipIndex.IDLE;
		}
	}
	public ITargetAction EditorSetup(GameObject editor, GameObject t) {
		if (editor != null) {
			TargetActionAnimation move = editor.GetComponent<TargetActionAnimation> ();
			if (move == null) {
				
				move = editor.AddComponent<TargetActionAnimation> ()  as TargetActionAnimation;
				move.target = t;
				setup = true;
				target = t;
				oldIndex = -1;
				return move as ITargetAction;
			}
		}
		setup = true;
		target = t;
		oldIndex = -1;
		return this as ITargetAction;
	}
	public void Pause() {
		play = false;
		Animator animator = target.GetComponent<Animator> ();
		if (animator != null) {
			animator.enabled = true;
			animator.speed = 0;
		}
	}
	private void Play(int speed) {
		
	}

	public void Play() {
		Animator animator = target.GetComponent<Animator> ();
		if (animator != null) {
			animator.enabled = true;
			animator.speed = 1;
		}
	}
	public void Load(TargetActionData data) {
		index = data.state;
		useRandom = data.useRandom;
		if (useRandom) {
			index =  UnityEngine.Random.Range (0, 4);
		 
		}
		SetupAction (target, GetLevelDifficulty (index));
	}
	public TargetActionData Save () {
		TargetActionData data = new TargetActionData ();
		data.state = index;
		data.useRandom = useRandom;
		return data;
	}
	public void SetupAction(GameObject t, LevelDifficulty level){
		if (setup == false) {
			this.target = t;
			this.level = level;
			setup = true;
			if (t == null)
				return;
			Animator animator = target.GetComponent<Animator> ();
			if (animator != null) {
				animator.enabled = true;
				AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
				int idx = (int)GetClip (level);
				if (idx >= clips.Length - 1)
					idx = clips.Length - 1;

				if (idx >= 0) {
					animator.StopPlayback ();
				
						animator.Play (clips [idx].name, -1, 0);
					}
				}
			
			
			} else {
				//Debug.Log ("No animator on object" + target.name + " Aborting");
			}
	}


	void Start(){
		if (target == null) {
			target = gameObject;
		}
		setup = false;

	}

	void Update() {
		if (setup == false) {
			//remove this. its for testing only
		//	SetupAction (target, LevelDifficulty.INSANE);
		}
		if (oldIndex != index) {
			oldIndex = index;
			setup = false;
			SetupAction (target, GetLevelDifficulty (index));
			setup = true;
		}

	}
}

