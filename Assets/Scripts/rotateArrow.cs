using UnityEngine;
using System.Collections;

// this class steers the arrow and its behaviour


public class rotateArrow : MonoBehaviour
{

	// register collision
	bool collisionOccurred;

	// References to GameObjects gset in the inspector

	public GameObject bow;

 
	public SoundFXManager mySoundFXManager;

	// the vars realize the fading out of the arrow when target is hit
	float alpha;
	float life_loss;
	public Color color = Color.white;

	Transform start;

    float lerpValue = 0.01f;
    float lerpIncrease=0.01f;

	public bool isHoming= false;

	// Use this for initialization
	void Start ()
	{
		// set the initialization values for fading out
		float duration = 1.5f;
		life_loss = 1f / duration;
		alpha = 1f;
		start = transform;

	}



	// Update is called once per frame
	void Update ()
	{
		//this part of update is only executed, if a rigidbody is present
		// the rigidbody is added when the arrow is shot (released from the bowstring)
		if (transform.GetComponent<Rigidbody2D> () != null) {
			// do we fly actually?
			if (GetComponent<Rigidbody2D> ().velocity != Vector2.zero) {
				// get the actual velocity
				Vector2 vel = GetComponent<Rigidbody2D> ().velocity;
				// calc the rotation from x and y velocity via a simple atan2
				float angleZ = Mathf.Atan2 (vel.y, vel.x) * Mathf.Rad2Deg;
				//float angleY = Mathf.Atan2 (-1.0f, vel.x) * Mathf.Rad2Deg;
				// rotate the arrow according to the trajectory
				transform.eulerAngles = new Vector3 (0, 15, angleZ);
			}
			if (bow.GetComponent<bowAndArrow> ().isHoming || isHoming ) {
				if( !collisionOccurred)
				{
                    lerpValue += lerpIncrease;
				GameObject Target = GameObject.FindGameObjectWithTag("Target");
				GetComponent<Rigidbody2D> ().velocity = Vector2.zero;

				Collider2D mycollider = Target.GetComponentInChildren<Collider2D>();
					Debug.Log ("**" + 1);
				if (mycollider) {
						//start.position = Vector3.Lerp(transform.position, mycollider.transform.position, 0.05f);
                        start.position = Vector3.Lerp(start.position, mycollider.bounds.center,lerpValue);
					 

						Vector3 targetDir = mycollider.bounds.center - transform.position;
						//float step = 0.1f * Time.deltaTime;
						//Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
						//Debug.DrawRay(transform.position, newDir, Color.red);
						//transform.rotation = Quaternion.LookRotation(newDir);
						//Debug.Log ("**" + 2);


                        var dir = targetDir - transform.position;
                        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                       // angle -= 180;
                       
                    //    transform.rotation =  Quaternion.Lerp(transform.rotation ,Quaternion.AngleAxis(angle, Vector3.forward),lerpValue );
                      transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);


				}
                    else
                    {
                        Vector3 targetDir = mycollider.bounds.center - transform.position;
                
                        var dir = targetDir - transform.position;
                         var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                        transform.rotation =  Quaternion.AngleAxis(angle, Vector3.forward);
                   

                    }

				}

			}
		}

 
		// if the arrow hit something...
		if (collisionOccurred) {
			// fade the arrow out
			alpha -= Time.deltaTime * life_loss;
			//GetComponent<Renderer> ().material.color = new Color (color.r, color.g, color.b, alpha);

			// if completely faded out, die:
			if (alpha <= 0f) {
				// create new arrow
				bow.GetComponent<bowAndArrow> ().createArrow (true);

				bow.GetComponent<bowAndArrow> ().arrows = 3;
				// and destroy the current one
				Destroy (gameObject);
			}
		}
	}


 


	void	OnTriggerEnter2D (Collider2D other)
	{
		if (other.transform.name == "Miss") {

			 
			bow.GetComponent<bowAndArrow> ().setMissed ();
		}
		 
	}
	Animator getAnimatorOnParent(Transform t) {
		if (t.parent == null) {
			Animator anim = t.gameObject.GetComponent<Animator> ();
			return anim;
		} else {
			return getAnimatorOnParent (t.parent);
		}
	}
	GameObject GetParentByTag(GameObject child, string tag) {
		if (child == null) {
			return null;
		}
		if(child.CompareTag(tag) == true) {
			return child;
		}
		Transform parent = child.transform.parent;
		if (parent == null)
			return null;
		return GetParentByTag (parent.gameObject, tag);
	}
	void collision (Transform trans, Collision2D other)
	{
		// we must determine where the other object has been hit
		float y;
		// we have to determine a score
		int actScore = 0;

		GameObject parent = GetParentByTag (trans.gameObject, "MainTarg");
		//so, did a collision occur already?
		if (collisionOccurred) {
			// fix the arrow and let it not move anymore
			//transform.position = new Vector3 (trans.position.x, transform.position.y, transform.position.z);

			// the rest of the method hasn't to be calculated
			return;
		}

		// I installed cubes as border collider outside the screen
		// If the arrow hits these objects, the player lost an arrow
		if (trans.name == "Cube") {
			bow.GetComponent<bowAndArrow> ().createArrow (false);
			Destroy (gameObject);
			bow.GetComponent<bowAndArrow> ().setMiss ();
		}



		// Ok - 
		// we hit the target
		if (trans.name.Contains ("target") || trans.tag.Contains("Target")) {
			
			Animator anim = getAnimatorOnParent (trans);
			if (anim != null) {
				anim.enabled = true;
			}

			float xMod = 0;
			//print(trans.name);
			if (trans.name.Contains("snail"))
				xMod = 0.66f;
			else if (trans.parent != null && trans.parent.name.Contains("Chicken"))
				xMod = 0.35f;
			else if (trans.parent != null && trans.parent.parent != null && trans.parent.parent.name.Contains("Giraffe"))
				xMod = 0.45f;

			// tries to set the arrow to the center. Not perfect.
			transform.localPosition = new Vector3(other.collider.bounds.center.x + xMod,(transform.localPosition.y+other.collider.bounds.center.y)/2,0.0f);

			bow.GetComponent<bowAndArrow> ().startThePopCorn ();


			transform.parent = trans;

			/*
			Transform manualSetTarget = GameUtil.FindDeepChild(trans, "HitPosCenter");
			if (manualSetTarget)
			{
				transform.parent = manualSetTarget;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
			}
			*/

			// play the audio file ("trrrrr")
			mySoundFXManager.playEffect ("hit");
			if (parent == null) {
				Debug.Log ("!!!!");
			}
			CharacterInfo targetInfo = parent.gameObject.GetComponent<CharacterInfo>();
			if (targetInfo != null && targetInfo.hitClip != null) {
			mySoundFXManager.playClip (targetInfo.hitClip);
			}
				
			 
			// set velocity to zero
			GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
			// disable the rigidbody
			GetComponent<Rigidbody2D> ().isKinematic = true;
			transform.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;
			// and a collision occurred
			collisionOccurred = true;
			// disable the arrow head to create optical illusion
			// that arrow hit the target
			// though there may be more than one contact point, we take 
			// the first one in order
			y =  other.contacts [0].point.y;
			// y is the absolute coordinate on the screen, not on the collider, 
			// so we subtract the collider's position
			y = y - trans.position.y;



			actScore = 10;

	/*		if (y < 1.36906f && y > -1.45483f)
				actScore = 2;

			if (y < 0.9470826f && y > -1.021649f)
				actScore = 3;

			if (y < 0.6095f && y > -0.760f)
				actScore = 4;
*/
			if (y < 0.34f && y > -0.53f)
				actScore = 50;

			//int multiplier = targetInfo.maxHardLevel / 10;
			//actScore = actScore * multiplier;
			bow.GetComponent<bowAndArrow> ().setPoints (actScore);

			GetComponent<wiggle> ().setIsAnimating (true);

		}	
	}

	void OnCollisionEnter2D (Collision2D other)
	{
		collision (other.transform, other);

	
	}

	//
	// public void setBow
	//
	// set a reference to the main game object

	public void setBow (GameObject _bow)
	{
		bow = _bow;
	}

	public void setSoundFXManager (SoundFXManager _fxmanager)
	{
		mySoundFXManager = _fxmanager;

	}
}
