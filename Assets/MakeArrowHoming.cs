using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeArrowHoming : MonoBehaviour {
	bowAndArrow bow = null;
	public bool useIdioticCinamaticHoming = false;
	// Use this for initialization
	void Start () {
		GameObject gameManagerObj = GameObject.FindGameObjectWithTag("Bow");
		bow = gameManagerObj.GetComponent<bowAndArrow>();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetMouseButtonUp(0)) {
			Vector2 mousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			ContactFilter2D cf = new ContactFilter2D();
			Collider2D[] results = new Collider2D[5];
			int count = Physics2D.OverlapPoint (mousePosition,cf,results);

			Collider2D hitCollider = null;

			if (count > 0)
			{
				hitCollider = results[0];
			}

			if (hitCollider) {
				if(useIdioticCinamaticHoming) {
					GameObject arrow = GameObject.FindGameObjectWithTag("Arrow");
					if (arrow != null)
						arrow.GetComponent<rotateArrow>().isHoming = true;
				} else {
					if (gameObject.GetComponent<Collider2D>() == hitCollider)
					{

						GameObject arrow = GameObject.FindGameObjectWithTag("Arrow");
						Destroy(arrow);
						bow.startThePopCorn();
						bow.setPoints(100);
						bow.createArrow(true);
						bow.arrows = 3;
						// WebCamFlags want homing
						//	AutoFade.LoadScene (SceneToLoad, fadeOutTime, fadeInTime, SceneToLoad);



					}

				}
			}

	}
	}
	}
	 
	 