using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowerManager : MonoBehaviour {
	private static string THROWER_PREFIX = "thrower";
	private static string SELECTED = "selected_thrower";
	public GameObject[] throwers;
	// Use this for initialization
	void Start () {
		UnlockAvatar(0);
	}
	public bool IsAvatarUnlocked(int index) {
		
		if (PlayerPrefs.HasKey (THROWER_PREFIX + index)) {
			
			return true;
		}
		return false;
	}

	public void SetSelectedIndex(int index) {
		PlayerPrefs.SetInt (SELECTED, index);
		PlayerPrefs.Save ();
	}
	public GameObject GetSelected() {
		if (PlayerPrefs.HasKey (SELECTED) == false) {
			SetSelectedIndex (0);
		}
		int selected = PlayerPrefs.GetInt (SELECTED);
		return throwers [selected];
	}

	public void UnlockAvatar(int index) {
		if (PlayerPrefs.HasKey (THROWER_PREFIX + index) == false) {
			PlayerPrefs.SetInt (THROWER_PREFIX + index, 1);
			PlayerPrefs.Save ();
		}
	}
	

	public GameObject GetAvatar(int index) {
		if (index == 0) {
			UnlockAvatar (0);
		}
		if (IsAvatarUnlocked (index)) {
			if (index < 0 || index >= throwers.Length) {
				Debug.Log ("Invalid avatar index, returning default avatar");
				return throwers[0];
			}
			return throwers[index];
		}
		return null;
	}

	// Update is called once per frame
	void Update () {
		
	}
}
