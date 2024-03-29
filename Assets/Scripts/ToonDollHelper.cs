using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToonDollHelper : MonoBehaviour {

	Component[] boneRig;			// Contains the ragdoll bones
	float mass = .1f;				// Mass of each bone
	public Transform root;			// Assign the root bone to position the shadow projector
	public GameObject _model;
	public Mesh _bodyMesh;

	public Transform _headBone;

	private bool isRagDoll = false, wasTossed = false, readyForNext = false, isOutOfBounds = false, wasTossedOnce = false;
	private PhysicMaterial physMat;

	private Animator animator;

	private Vector3 endPos;
	private bool setEndPos = false;
	private bool gettingUp = false;

	private bool standUpAtEnd = false;
	private bool progressiveLevel = false;
	private bool turnStandingIntoRagdollOnPlayerHit = false;
	private bool allHitsTurnsRagdoll = false;
	private bool getUpAfterReenabledRagdoll = false;

	public bool isPlayerHelper = false;

	public int prize = 0;

	private Rigidbody rootRb;

	private int goalHit = -1;

	private bool _decapitated;
	private bool wasReset = false;

	private Vector3 tossForce = new Vector3(1, -3, 1);

	private int tossNumber = 0;

	private int timeStepAffectIndex;

	private bool checkAngularVelocityMove = false;

	private GravityMode gravityMode = GravityMode.GRAVITY_ON;

	private float stillStandTime = 0;
	private float stillStandTimeCounter = 0;

	private bool flipperPhysics = false;
	private float originalSleepTreshold;
	private float speedupTimer = 0;
	private float speedupForce = 10;

	//Blinking
	Color colorOriginal, color;
	float _R = 2500.0f, _G = 2500.0f, _B = 2500.0f;
	bool _randomColor;
	int _blinkCounter, _stopBlink;

	private bool isActive = true;

	private Vector3 drag_Angulardrag_Mass = new Vector3 (0.14f, 0.1f, 2.5f);
	private CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

	private ParticleSystem specialFxParticles;
	private float oldSpecialFxY;

	private Animator separateTestAnimator = null;


	public bool IsActive() {
		return isActive;
	}
	public void SetActive(bool active) {
		isActive = active;
	}

	public void SetRigidValues(Vector3 dam, CollisionDetectionMode cdm) {
		drag_Angulardrag_Mass = dam;
		collisionDetectionMode = cdm;
	}

	public void SetKinematic() {
		rootRb = GetComponent<Rigidbody> ();

		rootRb.isKinematic = true;
	}

	private void ReadyNext() {
		readyForNext = true;
		if (progressiveLevel)
			TrigAnim ("Idle");
		else
			PlayAnim ("Idle");

		Invoke ("SetKinematic", 2f);
		LeanTween.rotateY (gameObject, 0, 0.6f).setEaseInCubic ();
		StaticManager.RestoreTimeStep(timeStepAffectIndex);
	}

	public bool IsGettingUp() {
		return gettingUp;
	}

	public bool IsRagDoll() {
		return isRagDoll;
	}

	public bool IsPlayerHelper() {
		return isPlayerHelper;
	}

	public void SetTossForce(Vector3 tossForce) {
		this.tossForce = tossForce;
	}

	private void RotateToFrontFacing () {
		LeanTween.rotateY (gameObject, 0, 1.7f).setEaseInCubic ();
	}

	public void SetFlipperPhysics(bool state) {
		flipperPhysics = state;
	}
	public void SetSpeedupTimer(float time, float force) {
		speedupTimer = time;
		speedupForce = force;
	}

	private void ResetSleepTreshold() {
		if (flipperPhysics == false)
			return;

		foreach (Component c in boneRig)
			(c as Rigidbody).sleepThreshold = originalSleepTreshold;
	}


	public bool IsMoving() {
		bool isMoving = false;

		foreach (Component c in boneRig) {
			Rigidbody rb = (Rigidbody)c;

			if (Mathf.Abs (rb.velocity.x) > 0.2f || Mathf.Abs (rb.velocity.y) > 0.2f || Mathf.Abs (rb.velocity.z) > 0.2f)
				isMoving = true;
			if (checkAngularVelocityMove)
			if (Mathf.Abs (rb.angularVelocity.x) > 2f || Mathf.Abs (rb.angularVelocity.y) > 2f || Mathf.Abs (rb.angularVelocity.z) > 2f)
				isMoving = true;
		}
		return isMoving;
	}


	public void Update() {

		if (setEndPos) {
			setEndPos = false;
//			transform.position = endPos;
		}

		if (wasTossed && gettingUp == false) {

			bool isMoving = IsMoving();

			if (!isMoving) {

				stillStandTimeCounter -= Time.deltaTime;
				// Debug.Log (stillStandTimeCounter);

				if (stillStandTimeCounter <= 0) {
					isActive = false;

					ResetSleepTreshold ();

					if (!standUpAtEnd) {
						readyForNext = true;
					} else {
						disableRagdoll ();

						rootRb.isKinematic = false;
						rootRb.useGravity = true;

						timeStepAffectIndex = StaticManager.PushFixedTimeStep (0.002f);

						if (animator.GetBoneTransform (HumanBodyBones.Hips).forward.y > 0) //hip forward vector pointing upwards, initiate the get up from back animation
							PlayAnim ("GetUpBack");
						else
							PlayAnim ("GetUpFront"); // gets stuck in the ground...?! (actually, this anim is 7-8 s long!! its basically part of the anim to look stuck! replace anim!)

						if (specialFxParticles != null) {
							LeanTween.moveLocalY (specialFxParticles.gameObject, oldSpecialFxY, 1f);
						}

						endPos = root.position;
						endPos = new Vector3 (endPos.x, 0.12f, endPos.z);
						setEndPos = true;
						gettingUp = true;
						Invoke ("ReadyNext", 2f);
//					Invoke ("RotateToFrontFacing", 0.2f); // dsiable to wait with rotation until up

						// ** Blend stuff
						ragdollingEndTime = Time.time; //store the state change time
						state = RagdollState.blendToAnim;  

						//Store the ragdolled position for blending
						foreach (BodyPart b in bodyParts) {
							b.storedRotation = b.transform.rotation;
							b.storedPosition = b.transform.position;
						}

						//Remember some key positions
						ragdolledFeetPosition = 0.5f * (animator.GetBoneTransform (HumanBodyBones.LeftToes).position + animator.GetBoneTransform (HumanBodyBones.RightToes).position);
						ragdolledHeadPosition = animator.GetBoneTransform (HumanBodyBones.Head).position;
						ragdolledHipPosition = animator.GetBoneTransform (HumanBodyBones.Hips).position;
					}
				}
			} else
				stillStandTimeCounter = stillStandTime;

		}

		if (flipperPhysics) {
			// to force player not to shoot into the air. Has effects on overall speed though... experiment
			/* foreach (Component c in boneRig) {
				Rigidbody rb = (Rigidbody)c;
				if (rb.velocity.y > 0)
					rb.velocity = GameUtil.SetY (rb.velocity, rb.velocity.y / (1 + Time.deltaTime * 10));
			} */

			if (speedupTimer > 0) {
				foreach (Component c in boneRig) {
					Rigidbody rb = (Rigidbody)c;
					if (rb.velocity.z > 0 && rb.velocity.z < 50) {
						rb.velocity = GameUtil.SetZ (rb.velocity, rb.velocity.z * (1 + Time.deltaTime * speedupForce));
					}
					// Debug.Log (rb.velocity.z);
				}
				speedupTimer -= Time.deltaTime;
			}
		}

	}

	public Vector3 GetRootPos() {
		return root.position;
	}


	GameObject headSeparate = null;

	public void Awake() {
		if(root == null)
			root = transform.Find("Root");
		if (_model == null) {
			Transform t = transform.Find ("MicroMale");
			if (t != null)
				_model = t.gameObject;
			else
				_model = gameObject;
		}
		if (_headBone == null) {
			_headBone = GameUtil.FindDeepChild (transform, "Head");
		}
		boneRig = gameObject.GetComponentsInChildren<Rigidbody>(); 
		disableRagdoll();
		//Blinking
		colorOriginal = _model.GetComponent<Renderer>().material.color;

		animator = GetComponent<Animator> ();

		rootRb = GetComponent<Rigidbody> ();


		/* ** Blend stuff */
		//Find all the transforms in the character, assuming that this script is attached to the root
		Component[] components=GetComponentsInChildren(typeof(Transform));

		//For each of the transforms, create a BodyPart instance and store the transform 
		foreach (Component c in components)
		{
			BodyPart bodyPart=new BodyPart();
			bodyPart.transform=c as Transform;
			bodyParts.Add(bodyPart);
		}

		specialFxParticles = gameObject.GetComponentInChildren<ParticleSystem>();
		if (specialFxParticles != null)
			oldSpecialFxY = specialFxParticles.gameObject.transform.localPosition.y;


		// experimental attempt to re-paint face area of texture. What happens here is take existing Material texture, instantiate it, get a area of the texture as array, paint into array, then set the array of pixels, apply and recalculate mipmaps
		// This only works if the Texture is imported as (1: Read/Write Enabled On  2: (for each platform) Override is On, and the Format is set to an uncompressed format, like RGB 16 bit.)

		// Then in theory we could create many Textures where only the face is changed, then set the SkinnedMeshRenderer's material texture when we want to change expression.
		// Unless we have very few facial expressions (absolutely no anim between them), like 4-5 MAX, this would be a VERY bad idea because:
		// 1. The ENTIRE texture is duplicated (not just the face part), so we'd have 2048x2048*n size textures in memory
		// 2. It's probably quite slow to create/apply the changes to the textures, as we would have to do in the startup here
		// (3. It might even be slow to swap between the textures during running)
		/*
		SkinnedMeshRenderer rend = GetComponentInChildren<SkinnedMeshRenderer>();
		if (rend != null) {
			// duplicate original texture and assign to material
			Texture2D texture = Instantiate (rend.material.mainTexture) as Texture2D;
			rend.material.mainTexture = texture;

			Color[] colors = { Color.red, Color.yellow, Color.white };

			// entire Texture
			// Color[] cols = texture.GetPixels (0);
			//for (int i = 0; i < cols.Length; ++i)
			//	cols [i] = colors [(i/50000) % 3];
			//texture.SetPixels (cols, 0);

			// face only, hardcorded
			Color[] cols = texture.GetPixels (200, 400, 740, 600, 0); // y is stored "backwards" apparently, so hard to see in a program where it actually starts :(
			for (int i = 0; i < cols.Length; ++i) {
				cols [i] = colors [(i/50000) % 3];
			}
			texture.SetPixels (200, 400, 740, 600, cols, 0);

			// apply SetPixels, recalculate all mip levels
			texture.Apply (true);
		}
		*/

		headSeparate = GameObject.Find ("OnlyHead");
		if (headSeparate != null && _headBone != null) {
			headSeparate.transform.position = _headBone.position;
			headSeparate.transform.rotation = _headBone.rotation;
			headSeparate.transform.localScale = gameObject.transform.localScale * 1.1f;
		}

	}

	public void Start() {
		GameObject g = GameObject.Find("TESTME");
		if (g != null) {
			separateTestAnimator = g.GetComponent<Animator> ();
			if (separateTestAnimator != null)
				separateTestAnimator.Play ("Unlocked");
		}
	}


	public void Blink(int times,float speed,float red,float green,float blue){
		CancelInvoke();
		_randomColor= false;
		_R = red;
		_G = green;
		_B = blue;
		_stopBlink = times;
		InvokeRepeating("BlinkInvoke", speed, speed);
	}

	public void Blink(int times,float speed){
		CancelInvoke();
		_randomColor = true;
		_stopBlink = times;
		InvokeRepeating("BlinkInvoke", speed, speed);
	}

	public void BlinkInvoke() {
		if(_blinkCounter < _stopBlink){
			if(_randomColor){
				color = new Color((float)UnityEngine.Random.Range(1, 5) ,(float)UnityEngine.Random.Range(1, 5),(float)UnityEngine.Random.Range(1, 5),1.0f);
			}else{
				color = new Color(_R , _G , _B ,1.0f);
			}

			if(_model.GetComponent<Renderer>().material.color == colorOriginal){
				_model.GetComponent<Renderer>().material.color = color;
			}else{
				_model.GetComponent<Renderer>().material.color = colorOriginal;
			}
			_blinkCounter++;
		}else{
			_model.GetComponent<Renderer>().material.color = colorOriginal;
			_blinkCounter = 0;
			CancelInvoke();
		}
	}

	public void disableRagdoll(bool blendAnim = false) {
		isRagDoll = false;

		foreach(Component ragdoll in boneRig) {
			if((ragdoll.GetComponent<Collider>() != null) && ragdoll.GetComponent<Collider>()!=this.GetComponent<Collider>()){
				ragdoll.GetComponent<Collider>().enabled = false;
				ragdoll.GetComponent<Rigidbody>().isKinematic = true;
				ragdoll.GetComponent<Rigidbody>().mass = 0.01f;
			}
		}
		Collider col = GetComponent<Collider> ();
		if (col != null) {
				col.enabled = true;
		}
	}

	public enum GravityMode
	{
		GRAVITY_ON,
		GRAVITY_OFF,
		GRAVITY_ROOT
	};


	public void enableRagdoll(GravityMode gMode) {

		isRagDoll = true;

		foreach(Component ragdoll in boneRig) {
			if (ragdoll.GetComponent<Collider> () != null) {
				ragdoll.GetComponent<Collider> ().enabled = true;
				ragdoll.GetComponent<Collider> ().material = physMat;
			}
			if (ragdoll.GetComponent<CharacterJoint> () != null) {
				ragdoll.GetComponent<CharacterJoint> ().enablePreprocessing = true;
				ragdoll.GetComponent<CharacterJoint> ().enableProjection = true;
			}
			ragdoll.GetComponent<Rigidbody>().isKinematic = false; 
			ragdoll.GetComponent<Rigidbody>().mass = mass;
			ragdoll.GetComponent<Rigidbody>().useGravity = true;
			if (gMode == GravityMode.GRAVITY_OFF || (gMode == GravityMode.GRAVITY_ROOT && ragdoll.gameObject.name != "Root"))
				ragdoll.GetComponent<Rigidbody>().useGravity = false;
		}
		animator.enabled=false;
		GetComponent<Collider>().enabled = false;
	//	Destroy(GetComponent<BotControlScript>());
		GetComponent<Rigidbody>().isKinematic = true;
		GetComponent<Rigidbody>().useGravity = false;

		animator.enabled = false;
	}


	public int GetTossNumber() {
		return tossNumber;
	}

	private void InitPhysics(GravityMode gMode) {
		gravityMode = gMode;
		enableRagdoll (gMode);

		foreach (Component c in boneRig) {
			(c as Rigidbody).drag = drag_Angulardrag_Mass.x;
			(c as Rigidbody).angularDrag = drag_Angulardrag_Mass.y;
			(c as Rigidbody).mass = drag_Angulardrag_Mass.z;
			(c as Rigidbody).interpolation = RigidbodyInterpolation.Interpolate;
			(c as Rigidbody).collisionDetectionMode = collisionDetectionMode;
			if (flipperPhysics)
				originalSleepTreshold = (c as Rigidbody).sleepThreshold;
			(c as Rigidbody).sleepThreshold = 0;
		}

		wasTossed = wasTossedOnce = true;
		tossNumber++;

		stillStandTimeCounter = stillStandTime;
	}

	public void Toss(float forcePos, float anglePos, float skew, GravityMode gMode = GravityMode.GRAVITY_ON) {

		InitPhysics (gMode);

		float newForce = 0.5f + forcePos / 1.8f;
		float div = 30;
		float zForce = newForce * 600/div;
		if (zForce < 200/div)
			zForce = 200/div;
		
		foreach (Component c in boneRig) {
//			if (c.gameObject.name != "Head" && c.gameObject.name != "Spine1")
			(c as Rigidbody).velocity = new Vector3 (((anglePos - 0.5f) + skew / 30f) * 220 * forcePos / div * tossForce.x, tossForce.y, zForce * tossForce.z);
		}

		if (specialFxParticles != null) {
			LeanTween.moveLocalY (specialFxParticles.gameObject, 0, 1f);
		}

	}

	public void Drop(GravityMode gMode = GravityMode.GRAVITY_ON) {
		InitPhysics (gMode);

		if (specialFxParticles != null) {
			specialFxParticles.gameObject.transform.localPosition = GameUtil.SetY (specialFxParticles.gameObject.transform.localPosition, 0);
		}
	}


	public bool IsReadyForNext() {
		return readyForNext;
	}


	public bool WasTossed() {
		return wasTossed;
	}

	public void SetInactive () {
		readyForNext = true;
		wasTossed = false;
	}

	public void SetOutOfBounds () {
		readyForNext = true;
		wasTossed = false;
		isOutOfBounds = true;
		this.gameObject.SetActive (false);
	}

	public bool IsOutOfBounds() {
		return isOutOfBounds;
	}

	public void SetMaterial(PhysicMaterial physMat) {
		this.physMat = physMat;
	}


	public void HeadMove(bool enabled) {
		foreach(Component ragdoll in boneRig) {
			if(ragdoll.name == "Head" && enabled) {

				if (ragdoll.GetComponent<Animator>() != null) {
					ragdoll.GetComponent<Animator>().enabled = true;
					ragdoll.GetComponent<Animator> ().updateMode = AnimatorUpdateMode.AnimatePhysics;
					ragdoll.GetComponent<Animator>().applyRootMotion = false;

					ragdoll.GetComponent<Collider>().enabled = true;
					ragdoll.GetComponent<Rigidbody>().isKinematic = true;
					ragdoll.GetComponent<Rigidbody>().useGravity = true;
				}
			}
		}
	}

	public void PlayAnim(string animName, float animSpeed = 1) {
		animator.speed = 1 * animSpeed;
		animator.enabled = true;
		animator.Play (animName);
	}

	public void TrigAnim(string triggerName, float animSpeed = 1) {
		animator.speed = 1 * animSpeed;
		animator.enabled = true;
		animator.SetTrigger (triggerName);
	}

	public void Reset() {
		readyForNext = false;
		wasTossed = false;
		setEndPos = false;
		gettingUp = false;
		isOutOfBounds = false;
		//this.gameObject.SetActive (true);
		disableRagdoll ();
		wasReset = true;
	}

	public void SetStandup(bool standup, bool progressive, bool turnStandingIntoRagdoll) {
		standUpAtEnd = standup;
		progressiveLevel = progressive;
		if (progressiveLevel)
			standUpAtEnd = true;
		this.turnStandingIntoRagdollOnPlayerHit = turnStandingIntoRagdoll;
	}

	public void HitGoal (int goalIndex) {
		goalHit = goalIndex;
	}

	public int HasHitGoal() {
		int retval = goalHit;
		goalHit = -1;
		return retval;
	}

	void OnCollisionEnter(Collision collision) {
		if (rootRb != null && !rootRb.isKinematic || !wasReset || !turnStandingIntoRagdollOnPlayerHit)
			return;

		GameObject findMe = GameUtil.FindParentWithTag (collision.collider.gameObject, "Player");
		if (findMe != null || allHitsTurnsRagdoll) {
			enableRagdoll (gravityMode);
			if (getUpAfterReenabledRagdoll) {
				rootRb.isKinematic = false;
				Invoke ("SetWasTossed", 2f);
			}
		}
	}

	private void SetWasTossed() {
		wasTossed = true;
	}

	public void EnableAngularVelocityCheck() {
		checkAngularVelocityMove = true;
		Invoke ("DisableAngularVelocityCheck()", 6f);
	}
	public void DisableAngularVelocityCheck() {
		checkAngularVelocityMove = false;
	}



	/* Transition from ragdoll to anim smoothly code here: */

	//Possible states of the ragdoll
		enum RagdollState
	{
		animated,	 //Mecanim is fully in control
		ragdolled,   //Mecanim turned off, physics controls the ragdoll
		blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last ragdolled pose
	}

	//The current state
	RagdollState state=RagdollState.animated;

	//How long do we blend when transitioning from ragdolled to animated
	private float ragdollToMecanimBlendTime=0.5f;
	float mecanimToGetUpTransitionTime=0.05f;

	//A helper variable to store the time when we transitioned from ragdolled to blendToAnim state
	float ragdollingEndTime=-100;

	//Declare a class that will hold useful information for each body part
	public class BodyPart
	{
		public Transform transform;
		public Vector3 storedPosition;
		public Quaternion storedRotation;
	}
	//Additional vectores for storing the pose the ragdoll ended up in.
	Vector3 ragdolledHipPosition,ragdolledHeadPosition,ragdolledFeetPosition;

	//Declare a list of body parts, initialized in Start()
	List<BodyPart> bodyParts=new List<BodyPart>();


	void LateUpdate()
	{
		//Clear the get up animation controls so that we don't end up repeating the animations indefinitely
		//		animator.SetBool("GetUpFromBelly",false);
		//		animator.SetBool("GetUpFromBack",false);

		//Blending from ragdoll back to animated
		if (state==RagdollState.blendToAnim)
		{
			if (Time.time<=ragdollingEndTime+mecanimToGetUpTransitionTime)
			{
				//If we are waiting for Mecanim to start playing the get up animations, update the root of the mecanim
				//character to the best match with the ragdoll
				Vector3 animatedToRagdolled=ragdolledHipPosition-animator.GetBoneTransform(HumanBodyBones.Hips).position;
				Vector3 newRootPosition=transform.position + animatedToRagdolled;

				//Now cast a ray from the computed position downwards and find the highest hit that does not belong to the character 
				RaycastHit[] hits=Physics.RaycastAll(new Ray(newRootPosition,Vector3.down)); 
				newRootPosition.y=0;
				foreach(RaycastHit hit in hits)
				{
					if (!hit.transform.IsChildOf(transform))
					{
						newRootPosition.y=Mathf.Max(newRootPosition.y, hit.point.y);
					}
				}
				transform.position=newRootPosition;

				//Get body orientation in ground plane for both the ragdolled pose and the animated get up pose
				Vector3 ragdolledDirection=ragdolledHeadPosition-ragdolledFeetPosition;
				ragdolledDirection.y=0;

				Vector3 meanFeetPosition=0.5f*(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position + animator.GetBoneTransform(HumanBodyBones.RightFoot).position);
				Vector3 animatedDirection=animator.GetBoneTransform(HumanBodyBones.Head).position - meanFeetPosition;
				animatedDirection.y=0;

				//Try to match the rotations. Note that we can only rotate around Y axis, as the animated characted must stay upright,
				//hence setting the y components of the vectors to zero. 
				transform.rotation*=Quaternion.FromToRotation(animatedDirection.normalized,ragdolledDirection.normalized);
			}
			//compute the ragdoll blend amount in the range 0...1
			float ragdollBlendAmount=1.0f-(Time.time-ragdollingEndTime-mecanimToGetUpTransitionTime)/ragdollToMecanimBlendTime;
			ragdollBlendAmount=Mathf.Clamp01(ragdollBlendAmount);

			//In LateUpdate(), Mecanim has already updated the body pose according to the animations. 
			//To enable smooth transitioning from a ragdoll to animation, we lerp the position of the hips 
			//and slerp all the rotations towards the ones stored when ending the ragdolling
			foreach (BodyPart b in bodyParts)
			{
				if (b.transform!=transform){ //this if is to prevent us from modifying the root of the character, only the actual body parts
					//position is only interpolated for the hips
					if (b.transform==animator.GetBoneTransform(HumanBodyBones.Hips))
						b.transform.position=Vector3.Lerp(b.transform.position, b.storedPosition, ragdollBlendAmount);
					//rotation is interpolated for all body parts
					b.transform.rotation=Quaternion.Slerp(b.transform.rotation, b.storedRotation, ragdollBlendAmount);
				}
			}

			//if the ragdoll blend amount has decreased to zero, move to animated state
			if (ragdollBlendAmount==0)
			{
				state=RagdollState.animated;
				return;
			}
		}

		if (headSeparate != null && _headBone != null) {
			headSeparate.transform.position = _headBone.position;
			headSeparate.transform.rotation = _headBone.rotation;
		}
		
		// testing separate anim from "ghost" doll
		if (separateTestAnimator != null) {
			Transform t = separateTestAnimator.GetBoneTransform (HumanBodyBones.Head);
			Transform t2 = animator.GetBoneTransform (HumanBodyBones.Head);

			t2.transform.rotation = t.transform.rotation; // looks smoother (to me), but supposedly not good, perhaps affects collisions etc

			// supposedly this is more correct. Also, it automatically only works when player is ragdolled
			//Rigidbody rb = t2.gameObject.GetComponent<Rigidbody> ();
			//rb.MoveRotation (t.transform.rotation);
		}

	}

	public void SetStillStandTime(float value, bool setOnlyIfHigher = false) {
		if (setOnlyIfHigher == false || value > stillStandTime)
			stillStandTime = value;
		stillStandTimeCounter = stillStandTime;
	}

	public bool WasTossedOnce() {
		return wasTossedOnce;
	}

}
