using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFromSpawnPoint : MonoBehaviour
{
	private Vector3 start;
	private Vector3 stop;
	// Use this for initialization
	bool reverse = false;
	public bool update = false;
	private float startTime;
	private float duration = 25f;
	void Start() {
		startTime = Time.time;
		if (transform.parent != null) {
			start = transform.parent.transform.position;
		
			start.y = start.y - Random.Range (1, 2);

		} else {
			start = transform.position;
		}

			stop = new Vector3 (start.x*-1.0f, start.y, start.z);
		duration = Random.Range (4, 12);
		if (start.x > 0) {
			reverse = true;
		
		} else {
			transform.Rotate (new Vector3 (0, 180, 0));
		}
	}

	void Update() {
		float t = (Time.time - startTime) / duration;
	//	if (reverse) {
	//		transform.position = new Vector3 (Mathf.SmoothStep (stop.x, start.x, t), 0, 0);
	//	} else {
		transform.position = new Vector3 (Mathf.SmoothStep (start.x, stop.x, t), stop.y, 0);
	//	}

		if (transform.position.x  == stop.x) {
			Destroy (gameObject);
		}
	}

	/*
	void Start ()
	{
		
		//update = true;
		//SetTargetPos ();
	}
	public	void SetTargetPos(bool r) {
		rotation = false;
		frometh = transform.position;
		//	untoeth = new Vector3 (frometh.x - Random.Range (2, 3), frometh.y - Random.Range (4, 5), frometh.z);

		untoeth = new Vector3 (transform.position.x*-1.0f, 3, frometh.z);
		rotation = r;
		update = true;
	}
	public void Spawn() {
		update = true;
	}
	
	// Update is called once per frame
	void Update ()
	{
		float smoothstep = Mathf.SmoothStep (0f, 1f,
			                   Mathf.PingPong ((Time.time ) / 8.0f, 1f));
		if (update) {
			
			float x = Mathf.Lerp (frometh.x, untoeth.x,smoothstep);

			float y = Mathf.Lerp (frometh.y, untoeth.y,
				         Mathf.SmoothStep (0f, 1f,
					         Mathf.PingPong ((Time.time) / 8.0f, 1f)
				         ));
			transform.position = new Vector3 (x, y, transform.position.z);
			if (x <= -14 && rotation == false) {
					Destroy (gameObject);
			//	update = false;
			//	SetTargetPos ();


			}
			if (x > 14 && rotation == true) {
					Destroy (gameObject);
				//update = false;
				//SetTargetPos ();
			

			}

		
		}
	}*/
}
