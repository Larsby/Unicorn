using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardSpawner : MonoBehaviour {
	public GameObject[] spawnPoints;
	public GameObject[] prefabs;
	public float minWaitInSec = 20.0f;
	public float maxWaitInSec = 100.0f;
	float time = 0;
	float delay = 0.0f;
	// Use this for initialization
	void Start () {
		time = Time.time;
		delay = Random.Range (minWaitInSec, maxWaitInSec);
		time = Time.time + delay;
		//StartCoroutine (Spawn (0.0f));
	}
	IEnumerator Spawn(float delay) {
		yield return new WaitForSeconds (delay);
		int spawnIndex = Random.Range (0, spawnPoints.Length);
		int prefabIndex = Random.Range (0, prefabs.Length );
	
		Transform spawnPosition = spawnPoints [spawnIndex].transform;

		GameObject spawn = Instantiate (prefabs [prefabIndex], spawnPosition);


	}

		// Update is called once per frame
	void Update () {
		
		if (time  < Time.time) {
			
			delay = Random.Range (minWaitInSec, maxWaitInSec);
//			Debug.Log ("Delay is " + delay);
			time = Time.time + delay;
			StartCoroutine (Spawn (0.0f));
		}
	}
}
