
using UnityEngine;
using System.Collections;

public class LookAt : MonoBehaviour
{

	Transform target;
	public float verify = 0.0f;
	public float speed = 10.0f;
	 
	// Use this for initialization
	void Start ()
	{

	}

	// Update is called once per frame
	void Update ()
	{
		if (target == null) {
			if (GameObject.Find ("arrow")) {
				target = GameObject.Find ("arrow").transform;
			}
		} else if (target) {
			Vector3 vectorToTarget = target.position - transform.position;
			float angle = Mathf.Atan2 (vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg;
			Quaternion q = Quaternion.AngleAxis (angle, Vector3.forward);
			transform.rotation = Quaternion.Slerp (transform.rotation, q, Time.deltaTime * speed);
		}
		 
	}
}