using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectManager : MonoBehaviour {

	public GameObject levelItemPrefab;
	public GameObject levelContainer;

	void Awake() {
	//	SoundManager.Create ();
	}

	void Start () {
		StaticManager.LevelData [] levels = StaticManager.GetAllLevelsForWorld ();

		for (int i = 0; i < levels.Length; i++) {
			GameObject levelItem = Instantiate (levelItemPrefab, levelContainer.transform, false);

			Transform levelTextTransform = levelItem.transform.Find ("LevelText");
			if (levelTextTransform != null) {
				Text levelText = levelTextTransform.gameObject.GetComponent<Text> ();
				if (levelText != null) {
					levelText.text = "" + (i + 1);
				}
			}

			for (int j = 0; j < 3; j++) {
				Transform star = GameUtil.FindDeepChild(levelItem.transform, "Star" + (j + 1));
				if (star != null) {
					Image starImage = star.gameObject.GetComponent<Image> ();
					if (starImage != null) {
						starImage.color = (j < levels[i].stars)? Color.white : Color.black;
					}
				}
			}

			Button button = levelItem.gameObject.GetComponent<Button> ();
			if (i == 0 || levels [i-1].stars > 0) {
				if (button != null) {
					int index = i;
					button.onClick.AddListener (() =>
						{
							StartLevel (index);
						}); // Have to create index from i, otherwise it always thinks "i" is 3 (i.e. at end of loop), because the listener gets the int Object, not the value
				}
			} else {
				if (button != null) {
					button.interactable = false;
				}
			}

		}
	}
	
	void Update () {
	}

	public void GotoMainScreen() {
		SceneManager.LoadScene ("Main");
	}

	public void GotoWorldScreen() {
		SceneManager.LoadScene ("WorldSelect");
	}

	public void StartLevel(int index) {
		if (StaticManager.usePlayLevelScreen) {
			StaticManager.SetLevel (index);
			SceneManager.LoadScene ("PlayLevel");
		} else
			StaticManager.StartLevel (index);
	}

}
