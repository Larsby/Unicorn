using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMusicManager : MonoBehaviour {

	// Use this for initialization
	void Start () {

		if (PlayerPrefs.HasKey ("Music") == false) {
			PlayerPrefs.SetInt ("Music", 1);
			PlayerPrefs.Save ();
		}
		bool enable = PlayerPrefs.GetInt ("Music") == 1 ? true : false;
		GetComponent<AudioSource> ().enabled = enable;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
