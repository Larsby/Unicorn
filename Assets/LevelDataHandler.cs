using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDataHandler : MonoBehaviour {
	public GameObject loadManagerPrefab;
	// Use this for initialization
	void Start () {
		GameObject loadManagerObject = GameObject.FindGameObjectWithTag ("LevelData");
		if (loadManagerObject == null) {
			loadManagerObject = Instantiate (loadManagerPrefab);
			loadManagerObject.transform.parent = null;
			LevelDataLoader loadManager = loadManagerObject.GetComponent<LevelDataLoader> ();
			loadManager.Load ();
		}
	}
	

}
