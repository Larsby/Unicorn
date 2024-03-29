using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewCharacterManager : MonoBehaviour {

	public Text nameText;

	private bool firstRun = true;
	// private AsyncOperation closeOperation = null;

	private CharacterManager characterManager = null;

	void Awake() {
	//	SoundManager.Create ();

		characterManager = CharacterManager.instance;
	}

	void Start () {
	}

	void Update () {
		
		if (firstRun) {

			int index = StaticManager.GetUnlockedAvatarIndex ();
			if (index < 0)
				index = 0;

			GameObject character = Instantiate (characterManager.characterPrefabs[index]);
			ToonDollHelper player = character.GetComponentInChildren<ToonDollHelper> ();

			character.transform.localPosition = new Vector3 (0, 0.25f, 0); // 0.37f for *1.0f below
			character.transform.localScale = character.transform.localScale * 1.2f;
			character.transform.localRotation = Quaternion.Euler (0, 0, 0);

			if (player != null) {
				player.disableRagdoll ();
				player.SetKinematic ();
				player.PlayAnim ("Unlocked");
			}

			nameText.text = characterManager.characterPrefabs[index].name;

			firstRun = false;
		}

		// if (closeOperation != null) { /* supposed to do sth? // closeOperation.isDone ? */ }
	}

	public void BackButton() {
		/*
		if (closeOperation != null)
			return;

		// Debug.Log ("Go back to previous scene somehow here");
		// Debug.Log ("https://gamedev.stackexchange.com/questions/116698/resuming-a-previous-scene-from-a-current-scene-using-the-unity3d");
		closeOperation = SceneManager.UnloadSceneAsync (SceneManager.GetActiveScene ().name);
		if (closeOperation != null)
			closeOperation.allowSceneActivation = true;
		*/

		StaticManager.PopScene ();
	}
}
