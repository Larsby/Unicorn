using System;
using UnityEngine;

[ExecuteInEditMode]

public class TargetActionMove:MonoBehaviour,ITargetAction, ITargetInfoDifficulty
	{
		public Vector3 startFromVector;
		public Vector3 stopToVector;
	public bool stillUseAsRange;
	public bool useSameValue = true;
	public Vector3 fromRandom = new Vector3(0,0,0);
	public Vector3 stopRandom = new Vector3(0,0,0);

	 Vector3 startFromVectorTemp;
	 Vector3 stopToVectorTemp;

    public bool useShadow = true;
    public int difficulty = 0;

		private PingPongVectorUtility util;
		private GameObject target;
	private bool play = true;
	public int GetDifficulty() {
		return difficulty;
	}
	public TargetActionMove ()
		{
			util = new PingPongVectorUtility ();
		}
	public ITargetAction EditorSetup(GameObject editor, GameObject t) {
		if (editor != null) {
			TargetActionMove move = editor.GetComponent<TargetActionMove> ();
			if (move == null) {
				move = editor.AddComponent<TargetActionMove> ()  as TargetActionMove;
			}
			move.target = t;
			move.stillUseAsRange = stillUseAsRange;
			move.useSameValue = useSameValue;
			return move as ITargetAction;
		}
		target = t;
		return this as ITargetAction;
	}
	public void Load(TargetActionData data) {
		startFromVector = LevelDataClasses.PastilleVectorToVec3( data.from);
		stopToVector = LevelDataClasses.PastilleVectorToVec3( data.to);
		stillUseAsRange = data.state == 1 ? true : false;
		useSameValue =  data.useSameValue == 1 ? true : false;
		fromRandom = LevelDataClasses.PastilleVectorToVec3( data.fromRandom);
		stopRandom = LevelDataClasses.PastilleVectorToVec3( data.stopRandom);

		startFromVectorTemp.x = startFromVector.x -  UnityEngine.Random.Range (-fromRandom.x, fromRandom.x);
		startFromVectorTemp.y = startFromVector.y - UnityEngine.Random.Range (-fromRandom.y, fromRandom.y);

		stopToVectorTemp.x = stopToVector.x -  UnityEngine.Random.Range (-stopRandom.x, stopRandom.x);
		stopToVectorTemp.y = stopToVector.y - UnityEngine.Random.Range (-stopRandom.y, stopRandom.y);
     
        useShadow = data.useShadow;// == 1 ? true : false;
        difficulty = data.difficulty;

        if(useShadow)
        {
            if(target.transform.Find("shadow") != null)
            {
                target.transform.Find("shadow").gameObject.SetActive(true) ;

            }
        }
        else
        {
                target.transform.Find("shadow").gameObject.SetActive(false);
        }



		play = true;
	}
	public TargetActionData Save () {
		TargetActionData data = new TargetActionData ();
		data.from = LevelDataClasses.Vector3ToPastilleVec3(startFromVector);
		data.to = LevelDataClasses.Vector3ToPastilleVec3(stopToVector);
		data.state = stillUseAsRange == true ? 1 : 0;
		data.useSameValue = useSameValue == true ? 1 : 0;

		data.fromRandom =LevelDataClasses.Vector3ToPastilleVec3(fromRandom);
		data.stopRandom =LevelDataClasses.Vector3ToPastilleVec3(stopRandom);
        data.difficulty = difficulty;
	 
			
		return data;
	}
	public void Pause() {
		play = false;
	}
	public void Play() {
		play = true;
	}
	public void  SetupAction(GameObject t, LevelDifficulty level){
		if (t == null)
			return;
			target = t;

		 ;
	 


	


		target.transform.position = startFromVectorTemp;
		if (stillUseAsRange) {
			target.transform.position = LevelDataClasses.GetRandomRange (startFromVectorTemp, stopToVectorTemp);


		}
		}

	void Start(){


	}

		void Update() {
			if (target == null) {
				target = gameObject;
			}

	 
		if (useSameValue) {
			stopToVector = startFromVector;
		}
		if (play && stillUseAsRange == false) {
			target.transform.position = util.PingPong (startFromVectorTemp, stopToVectorTemp);
		}
		if (play && stillUseAsRange) {
			play = false;

			target.transform.position = LevelDataClasses.GetRandomRange (startFromVectorTemp, stopToVectorTemp);
		}
		if (stillUseAsRange == false) {
			play = true;
		}
		}
	}



