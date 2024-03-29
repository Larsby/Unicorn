using System;
using UnityEngine;
[ExecuteInEditMode]
public class TargetActionScale:MonoBehaviour,ITargetAction
	{
	public Vector3 scaleFromVector;
	public Vector3 scaleToVector;
	private PingPongVectorUtility util;
	private GameObject target;
	private bool play = true;
	public bool stillUseAsRange;
	public bool useSameValue = true;
	public Vector3 fromRandom = new Vector3(0,0,0);
	public Vector3 stopRandom = new Vector3(0,0,0);

	Vector3 startFromVectorTemp;
	Vector3 stopToVectorTemp;


		public TargetActionScale ()
		{
		util = new PingPongVectorUtility ();
		}

	public ITargetAction EditorSetup(GameObject editor, GameObject t) {
		if (editor != null) {
			TargetActionScale move = editor.GetComponent<TargetActionScale> ();
			if (move == null) {
				move = editor.AddComponent<TargetActionScale> ()  as TargetActionScale;
			}
			move.target = t;
			move.scaleToVector = scaleToVector;
			move.scaleFromVector = scaleFromVector;
			move.stillUseAsRange = stillUseAsRange;
			move.useSameValue = useSameValue;
			return move as ITargetAction;
		}
		target = t;
		return this as ITargetAction;
	}

	public void Load(TargetActionData data) {
		scaleFromVector = LevelDataClasses.PastilleVectorToVec3( data.from);
		scaleToVector = LevelDataClasses.PastilleVectorToVec3( data.to);
		stillUseAsRange = data.state == 1 ? true : false;
		useSameValue =  data.useSameValue == 1 ? true : false;

		if (scaleFromVector == Vector3.zero) {
			scaleFromVector = new Vector3 (1.0f, 1.0f, 1.0f);
		}
		if (scaleToVector == Vector3.zero) {
			scaleToVector = new Vector3 (1.0f, 1.0f, 1.0f);
		}
		fromRandom = LevelDataClasses.PastilleVectorToVec3( data.fromRandom);
		stopRandom = LevelDataClasses.PastilleVectorToVec3( data.stopRandom);

	/*	scaleFromVector.x -=  UnityEngine.Random.Range (-fromRandom.x, fromRandom.x);
		scaleFromVector.y -= UnityEngine.Random.Range (-fromRandom.y, fromRandom.y);

		scaleToVector.x -=  UnityEngine.Random.Range (-stopRandom.x, stopRandom.x);
		scaleToVector.y -= UnityEngine.Random.Range (-stopRandom.y, stopRandom.y);
		*/
		startFromVectorTemp.x = scaleFromVector.x -  UnityEngine.Random.Range (-fromRandom.x, fromRandom.x);
		startFromVectorTemp.y = scaleFromVector.y - UnityEngine.Random.Range (-fromRandom.y, fromRandom.y);

		stopToVectorTemp.x = scaleToVector.x -  UnityEngine.Random.Range (-stopRandom.x, stopRandom.x);
		stopToVectorTemp.y = scaleToVector.y - UnityEngine.Random.Range (-stopRandom.y, stopRandom.y);


		if (useSameValue) {
			scaleToVector.x = scaleFromVector.x;
			scaleToVector.y = scaleFromVector.x;
			scaleFromVector.y = scaleFromVector.x;



			stopToVectorTemp.x = startFromVectorTemp.x;
			stopToVectorTemp.y = startFromVectorTemp.x;
			startFromVectorTemp.y = startFromVectorTemp.x;

		}

	
	}
	public TargetActionData Save () {
		TargetActionData data = new TargetActionData ();
		data.from = LevelDataClasses.Vector3ToPastilleVec3(scaleFromVector);
		data.to = LevelDataClasses.Vector3ToPastilleVec3(scaleToVector);
		data.state = stillUseAsRange == true ? 1 : 0;
		data.useSameValue = useSameValue == true ? 1 : 0;

		data.fromRandom =LevelDataClasses.Vector3ToPastilleVec3(fromRandom);
		data.stopRandom =LevelDataClasses.Vector3ToPastilleVec3(stopRandom);

		return data;
	}
	public void Pause() {
		play = false;
	}
	public void Play() {
		play = true;
	}


	public void SetupAction(GameObject t, LevelDifficulty level){
		if (t == null)
			return;
		target = t;
		target.transform.localScale = startFromVectorTemp;
		if (stillUseAsRange) {
			target.transform.localScale = LevelDataClasses.GetRandomRange (startFromVectorTemp, stopToVectorTemp);
		}
		
	}
		
	void Start(){
		
	}

	void Update() {
		if (target == null) {
			target = gameObject;
		}

	
		if (play && stillUseAsRange == false) {
			target.transform.localScale = util.PingPong (startFromVectorTemp, stopToVectorTemp);
		}
		if (play && stillUseAsRange) {
			play = false;

			target.transform.localScale = LevelDataClasses.GetRandomRange (startFromVectorTemp, stopToVectorTemp);
		}
		if (stillUseAsRange == false) {
			play = true;
		}
	}
}

