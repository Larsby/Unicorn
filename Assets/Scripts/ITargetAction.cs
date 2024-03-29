using System;
using UnityEngine;

	public interface ITargetAction
	{
	void SetupAction(GameObject target, LevelDifficulty level);
	ITargetAction EditorSetup(GameObject editor, GameObject t);
	void Pause ();
	void Play ();
	void Load(TargetActionData data);
	TargetActionData Save ();
	}





