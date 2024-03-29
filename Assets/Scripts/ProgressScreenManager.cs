using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProgressScreenManager : MonoBehaviour {

	public Text headerText;
	public Text messageText;
	public GameObject betterContainer;
	public GameObject worseContainer;
	public GameObject neverWonContainer;
	public Image [] worseStarImages;

	public Image [] starImages;
	public Button nextLevelButton;

	private StaticManager.LevelData results, oldResults;

	void Awake() {
		//SoundManager.Create ();
	}

	void Start () {
		results = StaticManager.GetPlayResults ();
		oldResults = StaticManager.GetOldPlayResults ();

		string levelString = StaticManager.GetLevelString ();

		if (results.stars > 0) {
			headerText.text = levelString + " " + "YOU ROO0lZ!#";

		} else {
			headerText.text = levelString + " " + "YOU... SUCK!!";
		}

		for (int i = 0; i < 3; i++) {
			starImages [i].color = i < results.stars ? Color.white : Color.black;
		}

		StaticManager.LevelData savedLevelData = StaticManager.GetCurrentLevelData ();
		nextLevelButton.gameObject.SetActive (savedLevelData.stars > 0);

		betterContainer.SetActive (false);
		worseContainer.SetActive (false);
		neverWonContainer.SetActive (false);

		if (results.stars > oldResults.stars) {
			betterContainer.SetActive (true);
		} else {
			if (savedLevelData.stars > 0)
				worseContainer.SetActive (true);
			else
				neverWonContainer.SetActive (true);
		}

		for (int i = 0; i < 3; i++) {
			worseStarImages [i].color = i < oldResults.stars ? Color.white : Color.black;
		}

		if (StaticManager.IsLastLevelOfAllWorlds())
			nextLevelButton.gameObject.SetActive (false);
		if (StaticManager.IsLastLevelOfCurrentWorld () && StaticManager.IsNextWorldEnabled() == false)
			nextLevelButton.gameObject.SetActive (false);
	}
	
	void Update () {}


	public void StartNextLevel() {
		StaticManager.StartNextLevel ();
	}

	public void RestartSameLevel() {
		StaticManager.RestartSameLevel ();
	}

	public void GotoMainScreen() {
		SceneManager.LoadScene ("Main");
	}

	public void GotoLevelScreen() {
		SceneManager.LoadScene ("LevelSelect");
	}

	public void SelectCharacter() {
		// test   SceneManager.LoadScene ("NewCharacter", LoadSceneMode.Additive);  // if we want to use additive solution with scene on scene
		StaticManager.PushScene();
		SceneManager.LoadScene ("CharSelect");
	}

}
