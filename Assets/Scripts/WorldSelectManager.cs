using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldSelectManager : MonoBehaviour {

	public GameObject worldItemPrefab;
	public GameObject worldContainer;
	public Text acquiredStarsText;

	void Awake() {
	//	SoundManager.Create ();
	}

	void Start () {

		acquiredStarsText.text = "" + StaticManager.GetNofStars ();

		StaticManager.WorldData[] worlds = StaticManager.GetWorlds ();

		for (int i = 0; i < worlds.Length; i++) {

			GameObject worldItem = Instantiate (worldItemPrefab, worldContainer.transform, false);

			Transform levelTextTransform = worldItem.transform.Find ("WorldText");
			if (levelTextTransform != null) {
				Text worldText = levelTextTransform.gameObject.GetComponent<Text> ();
				if (worldText != null) {
					worldText.text = "" + (i + 1);
				}
			}

			Transform requiredStarsTranform = worldItem.transform.Find ("RequiredStars");

			Button button = worldItem.gameObject.GetComponent<Button> ();
			if (worlds[i].bIsEnabled) {
				if (button != null) {
					int index = i;
					button.onClick.AddListener (() =>
						{
							GotoLevelSelect (index);
						});
				}
				if (requiredStarsTranform != null)
					requiredStarsTranform.gameObject.SetActive(false);
				
			} else {
				if (button != null) {
					button.interactable = false;
				}
				if (requiredStarsTranform != null) {
					Transform t = GameUtil.FindDeepChild (requiredStarsTranform, "NofStars");
					if (t != null) {
						Text nofStarsText = t.gameObject.GetComponent<Text> ();
						if (nofStarsText != null)
							nofStarsText.text = "" + worlds [i].nofStarsToUnlock;
					}
				}
			}
		}
	}
	
	void Update () {
	}

	public void GotoMainScreen() {
		SceneManager.LoadScene ("Main");
	}

	public void GotoLevelSelect(int index) {
		StaticManager.GotoLevelSelectScreen (index);
	}

}
