using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shadow : MonoBehaviour {

	 
	public Transform followTransform;
	// Use this for initialization
 
	Vector3 oldpos;
	void Start () {
 


	}
	void Awake()

	{
		//Awake verkar kallas på även om man inte har skriptet igång.
//		Debug.Log ("whaaa");
//		transform.SetSiblingIndex(0);
//		transform.SetParent(transform.parent.parent); //säg att skuggan inte skall ligga som barn till prefaben, då hoppar den längst upp och följer inte med transformen


	}


	// Update is called once per frame
	void LateUpdate () {

		if (followTransform != null) {
			oldpos = followTransform.position;
				
		} else {
			gameObject.SetActive(false);
		}
		//följ bara med vågrätt.

	}

	void Update()
	{
 

		if (transform) {
			transform.position = new Vector3(oldpos.x, transform.position.y, transform.position.z);

		}


	//	Debug.Log ("oldpos.x:" + oldpos.x + " y:" + oldpos.y);

	}
}
