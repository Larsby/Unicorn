using System;

using UnityEngine;
[ExecuteInEditMode]
public class TargetActionRotate:MonoBehaviour,ITargetAction
{
	public Vector3 rotateFromVector;
	public Vector3 rotateToVector;
	private PingPongVectorUtility util;
	private GameObject target;
	private bool play = true;
	public Vector3 fromRandom = new Vector3(0,0,0);
	public Vector3 stopRandom = new Vector3(0,0,0);

	Vector3 startFromVectorTemp;
	Vector3 stopToVectorTemp;


	public TargetActionRotate ()
	{
		util = new PingPongVectorUtility ();
	}

	public void SetupAction(GameObject t, LevelDifficulty level){
		target = t;

	
	}

	public void Load(TargetActionData data) {
		rotateFromVector = LevelDataClasses.PastilleVectorToVec3( data.from);
		rotateToVector = LevelDataClasses.PastilleVectorToVec3( data.to);

		fromRandom = LevelDataClasses.PastilleVectorToVec3( data.fromRandom);
		stopRandom = LevelDataClasses.PastilleVectorToVec3( data.stopRandom);


		startFromVectorTemp.z = rotateFromVector.z -  UnityEngine.Random.Range (-fromRandom.z, fromRandom.z);
		 
		stopToVectorTemp.z = rotateToVector.z -  UnityEngine.Random.Range (-stopRandom.z, stopRandom.z);
	 

		 
 
	}
	public TargetActionData Save () {
		TargetActionData data = new TargetActionData ();
		data.from = LevelDataClasses.Vector3ToPastilleVec3(rotateFromVector);
		data.to = LevelDataClasses.Vector3ToPastilleVec3(rotateToVector);
		data.fromRandom =LevelDataClasses.Vector3ToPastilleVec3(fromRandom);
		data.stopRandom =LevelDataClasses.Vector3ToPastilleVec3(stopRandom);

		return data;
	}

	public ITargetAction EditorSetup(GameObject editor, GameObject t) {
		if (editor != null) {
			TargetActionRotate move = editor.GetComponent<TargetActionRotate> ();
			if (move == null) {
				move = editor.AddComponent<TargetActionRotate> ()  as TargetActionRotate;
			}
			move.target = t;
			return move as ITargetAction;
		}
		target = t;
		return this as ITargetAction;
	}
	public void Pause() {
		play = false;
	}
	public void Play() {
		play = true;
	}

	void Start(){}

	void Update() {
		if (target == null) {
			return;
		}
		if (play) {
			target.transform.localEulerAngles = util.PingPong (startFromVectorTemp, stopToVectorTemp);
		}
	}
}


