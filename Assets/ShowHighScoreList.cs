using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ShowHighScoreList : MonoBehaviour {
	dreamloLeaderBoard dl;
	public GameObject listItemPrefab;
	// Use this for initialization
	bool updated = false;
	bool listShown = false;
	void Start () {
		this.dl = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
		updated = false;
	}
	void ShowLeadbordList() {
		
		List<dreamloLeaderBoard.Score> scoreList = dl.ToListHighToLow();

			int maxToDisplay = 20;
			int count = 0;
		float yDir = -0.45f;
		float step = 0.0f;
		foreach (dreamloLeaderBoard.Score currentScore in scoreList) {
			GameObject listItem = Instantiate (listItemPrefab);
			//listItem.transform.parent = transform;

			listItem.transform.localScale = new Vector3 (-0.05f, 0.05f, 1.0f);
			//listItem.transform.position = Vector3.zero;
			Text name = listItem.transform.GetChild (0).GetComponent<Text> ();
			RectTransform rect = listItem.GetComponent<RectTransform> ();

			rect.position = new Vector3 (0.0f, 0.0f, 0.0f);
			Text score = listItem.transform.GetChild (1).GetComponent<Text> ();
			name.text = currentScore.playerName;
			score.text = ""+currentScore.score;
			rect.SetParent (transform.GetComponent<RectTransform>(),true);
			rect.position = transform.GetComponent<RectTransform> ().position;
			listItem.transform.localScale = new Vector3 (-0.05f, 0.05f, 1.0f);
			rect.position = new Vector3 (rect.position.x, rect.position.y-step, rect.position.z);
			step -= yDir;
		}
	}
	// Update is called once per frame
	void Update () {
		if (updated == false) {
			updated = true;
			dl.LoadScores ();
		}
		if (dl.ToListHighToLow ().Count > 0) {
			if (listShown == false) {

				ShowLeadbordList ();
				listShown = true;	
			}
		}
	}
}
