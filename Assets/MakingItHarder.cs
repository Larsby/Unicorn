using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DentedPixel;

public class MakingItHarder : MonoBehaviour
{
	private Vector3 frometh;
	private Vector3 untoeth;
	private float secondsForOneLengthX = 2f;
	private float secondsForOneLengthY = 1f;
	public bool restricMovement = false;
	public Vector2 upRestriction;
	public Vector2 downRestriction; 
	TargetManager targetManager;
	private bowAndArrow bow;
	TargetLevelManager levelManager;
	bool isDancing = true;

	float timeToremove = 0;
	float startTime;
	float danceSpeed = 1.0f;
	enum ClipIndex {
	IDLE =0,
	BOUNCE=1,
	BOUNCE_HARD=2
	}

	// Use this for initialization

	public void SetUpDifficulty(int characterIndex, int currentscore) {

		//GenerateTick.OnTick += Dance;
	
		int hardIndex = (int)ClipIndex.IDLE;

	
		int level = 0;
		if (bow != null) {

			currentscore = (int)(bow.getScore ());

			level = targetManager.GetCurrentHardLevel (currentscore); 
			int randomDirection = Random.Range (0, 2);
 			Vector2 startPos = Vector2.zero;	 
			if (level >7) {

				if (restricMovement) {
					startPos = upRestriction;
				} else {
					startPos = new Vector2 (4.448886f, 0.2837411f);
				}


				transform.position = new Vector3 (startPos.x,startPos.y, transform.position.z);


				frometh = transform.position;

				untoeth = new Vector3 (frometh.x - Random.Range (2, 3), frometh.y - Random.Range (4, 5), frometh.z);
				secondsForOneLengthX = Random.Range (.5f, 1);
				secondsForOneLengthY = Random.Range (.5f, 2);
				transform.localScale *= Random.Range (0.3f, 1.0f);

			} else if (level > 6) {


				if (restricMovement) {
					startPos = upRestriction;
				} else {
					startPos = new Vector2 (3.448886f,0.2837411f);
				}


				transform.position = new Vector3 (startPos.x,startPos.y, transform.position.z);


				frometh = transform.position;

				secondsForOneLengthX = Random.Range (1f, 2);
				secondsForOneLengthY = Random.Range (1, 2);
				untoeth = new Vector3 (frometh.x - Random.Range (1, 2), frometh.y - Random.Range (3, 4), frometh.z);
				transform.localScale *= Random.Range (0.4f, 0.8f);
			} else if (level > 5) {

				if (restricMovement) {
					startPos = upRestriction;
				} else {
					startPos = new Vector2 (3.448886f,0.2837411f);
				}


				transform.position = new Vector3 (startPos.x,startPos.y, transform.position.z);




				frometh = transform.position;
				transform.localScale *= Random.Range (0.6f, 1.0f);

				if (randomDirection == 0)
					untoeth = new Vector3 (frometh.x, frometh.y - Random.Range (2, 3), frometh.z);
				else if (randomDirection == 1)
					untoeth = new Vector3 (frometh.x - Random.Range (2, 3), frometh.y, frometh.z);



			} else if (level > 4) {
				Vector3 startVPos;
				Vector3 endVPos;
				if (restricMovement) {
					startVPos = new Vector3 (upRestriction.x, upRestriction.y, transform.position.z);
					endVPos = new Vector3 (downRestriction.x, downRestriction.y, transform.position.z);
				} else {
					startVPos = new Vector3 (1.448886f, 2.848886f, transform.position.z);
					endVPos = new Vector3(1.3837411f, 0.2837411f,transform.position.z);
				}


				transform.position = new Vector3 (Random.Range (startVPos.x, startVPos.y), Random.Range (endVPos.x, endVPos.y), transform.position.z);


				frometh = transform.position;
				transform.localScale *= Random.Range (0.8f, 1.0f);

				if (randomDirection == 0 && restricMovement == false) {
					untoeth = new Vector3 (frometh.x, frometh.y - Random.Range (1, 2), frometh.z);
				} else if (randomDirection == 1 || restricMovement) {
					untoeth = new Vector3 (frometh.x - Random.Range (1, 2), frometh.y, frometh.z);
				}

			} else {

				transform.position = new Vector3 (1.448886f - Random.Range (-.5f, .5f), 0 - Random.Range (-.5f, .5f), transform.position.z);
				frometh = transform.position;

				untoeth = frometh;

			}


			transform.localEulerAngles = new Vector3 (transform.localEulerAngles.x, transform.localEulerAngles.y, 0);

		}
		else {
			isDancing = false;
		}
		if (level > 3) {
			hardIndex = (int)ClipIndex.BOUNCE;
		}
		if (level > 4) {
			hardIndex = (int)ClipIndex.BOUNCE_HARD;
		}
		Animator animator = GetComponent<Animator> ();
		animator.enabled = true;
		AnimationClip [] clips = animator.runtimeAnimatorController.animationClips;
		int index = hardIndex;
		if (index >= clips.Length - 1)
			index = clips.Length - 1;

		if(index>=0)
			animator.Play (clips [index].name);
	}
	void Start ()
	{
		GameObject obj = GameObject.Find ("bow");
		bow = obj.GetComponent<bowAndArrow> ();
		targetManager = GameObject.Find ("TargetManager").GetComponent<TargetManager> ();
		levelManager = GameObject.FindWithTag ("TargetLevelManager").GetComponent<TargetLevelManager> ();
	}
	public void SlowlyStopDancing() {
		
	}
	public void stopDancing ()
	{
		Animator animator = GetComponent<Animator> ();
		//animator.enabled = false;
		isDancing = false;
		startTime = Time.time;

	}

	public void startDancing ()
	{

		isDancing = true;

		timeToremove = Time.time - startTime;

		Animator animator = GetComponent<Animator> ();
		animator.speed =1.0f;
		animator.enabled = true;
	}

	void Dance ()
	{
 
		if (!isDancing)
			return;
		if (!targetManager.AllowDance ())
			return;
		if (transform.localEulerAngles.z == 0) {
			//transform.localEulerAngles = new Vector3 (transform.localEulerAngles.x, transform.localEulerAngles.y, Random.Range (5.0f, 10.0f));
			//LeanTween.moveX (gameObject, 1f, 1f).setEase (LeanTweenType.easeInQuad).setDelay (1f);
			LeanTween.rotateZ (gameObject, Random.Range (5.0f, 10.0f), 0.2f).setEaseInOutCubic ();

	 
		} else {
			//transform.localEulerAngles = new Vector3 (transform.localEulerAngles.x, transform.localEulerAngles.y, 0);
			LeanTween.rotateZ (gameObject, 0, 0.2f).setEaseInOutCubic ();
		}
		 
	}

	void OnDestroy ()
	{
		//GenerateTick.OnTick -= Dance;
	 
	}
	// Update is called once per frame
	void Update ()
	{
		/*	if (!isDancing)
			return;
			*/
		return;
		if (isDancing) {
			float x = Mathf.Lerp (frometh.x, untoeth.x,
				          Mathf.SmoothStep (0f, 1f,
					          Mathf.PingPong ((Time.time - timeToremove) / secondsForOneLengthX, 1f)
				          ));

			float y = Mathf.Lerp (frometh.y, untoeth.y,
				          Mathf.SmoothStep (0f, 1f,
					          Mathf.PingPong ((Time.time - timeToremove) / secondsForOneLengthY, 1f)
				          ));

			transform.position = new Vector3 (x, y, transform.position.z);
		} else {
			Animator animator = GetComponent<Animator> ();
			float speed = animator.speed - 0.05f;
			if (speed < 0)
				speed = 0;
			animator.speed = speed;
		}
	}
}
