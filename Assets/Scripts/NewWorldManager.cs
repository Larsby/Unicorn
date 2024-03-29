using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewWorldManager : MonoBehaviour {

	public Text nameText;
	// private AsyncOperation closeOperation = null;

	void Awake() {
	//	SoundManager.Create ();
	}

	void Start () {
		int world, level;
		StaticManager.GetLevel (out world, out level);
		nameText.text = "Wurld" + (world + 2) + "!"; // +2 because of 0-based-index AND worldIndex has not yet been increased here
	}

	void Update () { 
		// if (closeOperation != null) { /* supposed to do sth? // closeOperation.isDone ? */ }
	}

	public void BackButton() {
		/*
		if (closeOperation != null)
			return;

		closeOperation = SceneManager.UnloadSceneAsync (SceneManager.GetActiveScene ().name);
		if (closeOperation != null)
			closeOperation.allowSceneActivation = true;
		*/
		StaticManager.PopScene ();
	}
}
