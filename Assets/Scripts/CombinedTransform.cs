using UnityEngine;

/* CombinedTransform
 * 
 * Author: Mikael Sollenborn
 * 
 * Purpose: Simple animation of object without writing code or using the often overkill Unity animation system. Supports sequences (multi-animations)
 * 
 * Dependencies: ColliderDelegator.cs: Implements ExternalHit
 * 				 Easing.cs
 * 				 LeanTween (for the switchrotation, this should better be refactored though)   (no longer use ITween because it only works if the gameobject does not have a rigidbody (even if it is kinematic!))
 * 
 *	Format of TransformChange string is: StartDelay_Animation_translationX_tY_tZ_translationSwitchTime_rotationX_rY_rZ_scaleX_sY_sX  (don't need to provide more than the first two). TransformChange is very incomplete
 *
 * Todo / known issues:
 *       1. support transform.lossyScale? (which is globalScale, but reportedly not entirely reliable if the objects is "skewed")
 *		 2. Both absolute and absolutedelta are messed up for rotation and only works properly in some cases (basically, when two axes are 0 and rotating the third one) :( Can't be bothered to try to fix at the moment, need a  better understanding of Euler/Quaternion conversion
 *		 3. Changing public variables during runtime is largely untested and is not guaranteed to work
 *
 * Usage:
 *		 If start.autostart is false, the transform must be started from code (by setting autostart to true). start.autostart can also be used to pause or stop an ongoing transform
 */

public class CombinedTransform : ExternalHit
{
	public enum TranslationType { PerFrame, Forward_B, Up_G, Strafe_R, Absolute, AbsoluteDelta, ForwardAbsolute_B, UpAbsolute_G, StrafeAbsolute_R };
	public enum RotationType { PerFrame, Absolute, AbsoluteDelta };
	public enum ScaleType { PerFrame, Absolute, AbsoluteDelta };
	public enum SwitchEaseType { Linear, InOut, In, Out, BounceOut, BounceIn, InheritFromTranslation };
	public enum TransformTimeType { Infinite, SingleStop, SingleKill, SingleRepeat, SingleContinue };
	public enum RandomStartType { None, Animation, Transform, AnimationAndTransform};

	[System.Serializable]
	public class StartData
	{
		public bool autoStart = true;

		public float waitTime = 0;
		public float jumpTime = 0;

		public string animation = null;
		public RandomStartType randomStart = RandomStartType.None;
	}

	[System.Serializable]
	public class TranslationData
	{
		public Vector3 translationOrMagnitudeInX = Vector3.zero;
		public TranslationType type = TranslationType.PerFrame;
		public bool local = false;
		public bool allowSwitch = true;
		public bool inverseMagnitudeOnSwitch = true;
		public SwitchEaseType switchEase = SwitchEaseType.Linear;
		public Vector3 restoreOnEndSequence = Vector3.zero;
		[HideInInspector]
		public float translateHeadingMagnitude = 1;
	}

	[System.Serializable]
	public class RotationData
	{
		public Vector3 rotation = Vector3.zero;
		public RotationType type = RotationType.PerFrame;
		public bool local = true;
		public bool allowSwitch = false;
		public SwitchEaseType switchEaseOverrride = SwitchEaseType.InheritFromTranslation;
		public Vector3 restoreOnEndSequence = Vector3.zero;
	}

	[System.Serializable]
	public class ScalingData
	{
		public Vector3 scaling = Vector3.zero;
		public ScaleType type = ScaleType.PerFrame;
		public bool allowSwitch = false;
		public SwitchEaseType switchEaseOverride = SwitchEaseType.InheritFromTranslation;
		public Vector3 restoreOnEndSequence = Vector3.zero;
	}

	[System.Serializable]
	public class SwitchRotationData
	{
		public Vector3 rotation = Vector3.zero;
		public float time = 0;
		public LeanTweenType ease = LeanTweenType.easeOutExpo;
//		public iTween.EaseType ease = iTween.EaseType.easeOutExpo;
	}

	[System.Serializable]
	public class SettingsData
	{
		public string singleStopAnim = null;
		public bool stopOnExternalHit = true;
		public bool crossfadeAnimatorAnims = true;
		public float animationCrossFadeTime = 0.8f;
		public string id = "";
		public Transform lookAt;
		public bool forceAccurateStartJump = false;
	}

	[System.Serializable]
	public class SequencesData
	{
		public float[] time = null;
		public Vector3[] translationOrMagnitudeInX = null;
		public Vector3[] rotation = null;
		public Vector3[] scaling = null;
		public SwitchEaseType[] switchEase = null;
		public string[] anim = null;
		public Vector3[] switchRotation = null;
		public float[] switchRotationTime = null;
		public SwitchEaseType[] switchEaseRotationOverride = null;
		public SwitchEaseType[] switchEaseScalingOverride = null;
	}

	public StartData start;

	public TranslationData translation;

	public RotationData rotation;

	public ScalingData scaling;

	public TransformTimeType transformTimeType = TransformTimeType.Infinite;
	public float transformTime = -1;

	public SwitchRotationData switchRotation;

	public SettingsData settings;

	public SequencesData sequences;


	private Vector3 absolutePosFullDelta, absolutePosLast, absolutePosTarget;
	private RectTransform rectTransform = null;
	private int sequenceIndex = 0;
	private bool usingSequence = false;
	private Animator animator;
	private float transformTimer = float.MaxValue;
	private bool switched = false;
	private bool stopMe = false;
	private Vector3 orgPos, orgLocalPos, orgScale;
	private Quaternion orgRot, orgLocalRot;
	private bool initialized = false;
	private int seqLen = 0;

	private Vector3 absoluteRotLast, absoluteRotTarget;
	private Vector3 absoluteScaleFullDelta, absoluteScaleLast, absoluteScaleTarget;

	private Vector3 orgtranslationOrMagnitudeInXValue;
	private Vector3 orgRotationValue;
	private Vector3 orgScalingValue;
	private SwitchEaseType orgSwitchEaseValue, orgSwitchEaseRotValue, orgSwitchEaseScaleValue;
	private string orgStartAnimValue;
	private Vector3 orgSwitchRotationValue;
	private float orgSwitchRotationTimeValue;

	private TranslationType underlyingTranslationType = TranslationType.PerFrame;


	void PlayAnim(string stateName, bool randomStart = false) {
		if (animator != null) {
			if (settings.crossfadeAnimatorAnims) {
				if (randomStart)
					animator.CrossFadeInFixedTime(stateName, settings.animationCrossFadeTime, -1, Random.value);
				else
					animator.CrossFadeInFixedTime(stateName, settings.animationCrossFadeTime);
			} else
			{
				if (randomStart)
					animator.Play(stateName, -1, Random.value);
				else
					animator.Play(stateName);
			}
		}		
	}

	void Start() {

		orgtranslationOrMagnitudeInXValue = translation.translationOrMagnitudeInX;
		orgRotationValue = rotation.rotation;
		orgScalingValue = scaling.scaling;
		orgSwitchEaseValue = translation.switchEase;
		orgSwitchEaseRotValue = rotation.switchEaseOverrride;
		orgSwitchEaseScaleValue = scaling.switchEaseOverride;
		orgStartAnimValue = start.animation;
		orgSwitchRotationValue = switchRotation.rotation;
		orgSwitchRotationTimeValue = switchRotation.time;

		orgPos = transform.position;
		orgRot = transform.rotation;
		orgLocalPos = transform.localPosition;
		orgLocalRot = transform.localRotation;
		orgScale = transform.localScale;

		if (start.autoStart)
			DoStart();
	}

	public void Run(bool restorePosition = false, bool restoreLocalPosition = false, bool restoreRotation = false, bool restoreLocalRotation = false, bool restoreScale = false)
	{
		initialized = false;
		RunUnforcedInit(restorePosition, restoreLocalPosition, restoreRotation, restoreLocalRotation, restoreScale);
	}

	public void RunUnforcedInit(bool restorePosition = false, bool restoreLocalPosition = false, bool restoreRotation = false, bool restoreLocalRotation = false, bool restoreScale = false)
	{
		if (restorePosition)
			transform.position = orgPos;
		if (restoreLocalPosition)
			transform.localPosition = orgLocalPos;
		if (restoreRotation)
			transform.rotation = orgRot;
		if (restoreLocalRotation)
			transform.localRotation = orgLocalRot;
		if (restoreScale)
		{
			transform.localScale = orgScale;
		}

		start.autoStart = true;
		sequenceIndex = 0;
		translation.translationOrMagnitudeInX = orgtranslationOrMagnitudeInXValue;
		rotation.rotation = orgRotationValue;
		scaling.scaling = orgScalingValue;
		translation.switchEase = orgSwitchEaseValue;
		rotation.switchEaseOverrride = orgSwitchEaseRotValue;
		scaling.switchEaseOverride = orgSwitchEaseScaleValue;
		start.animation = orgStartAnimValue;
		switchRotation.rotation = orgSwitchRotationValue;
		switchRotation.time = orgSwitchRotationTimeValue;

		DoStart();
	}

	private void DoStart () {

		translation.translateHeadingMagnitude = translation.translationOrMagnitudeInX.x;

		//if (settings.isRectTransform)
		rectTransform = GetComponent<RectTransform>();

		if (sequences.time != null && sequences.time.Length > 0)
			seqLen = sequences.time.Length;
		
		if (seqLen > 0) {
			usingSequence = true;
			if (sequences.translationOrMagnitudeInX != null && sequences.translationOrMagnitudeInX.Length > sequenceIndex)
			{
				if (translation.type != TranslationType.Forward_B && translation.type != TranslationType.Strafe_R && translation.type != TranslationType.Up_G)
					translation.translationOrMagnitudeInX = sequences.translationOrMagnitudeInX[sequenceIndex];
				else
					translation.translateHeadingMagnitude = sequences.translationOrMagnitudeInX[sequenceIndex].x;
			}
			transformTime = sequences.time[sequenceIndex];
			if (sequences.switchRotation != null && sequences.switchRotation.Length > sequenceIndex)
				switchRotation.rotation = sequences.switchRotation[sequenceIndex];
			if (sequences.switchRotationTime != null && sequences.switchRotationTime.Length > sequenceIndex)
				switchRotation.time = sequences.switchRotationTime[sequenceIndex];
			if (sequences.rotation != null && sequences.rotation.Length > sequenceIndex)
				rotation.rotation = sequences.rotation[sequenceIndex];
			if (sequences.scaling != null && sequences.scaling.Length > sequenceIndex)
				scaling.scaling = sequences.scaling[sequenceIndex];
			if (sequences.switchEase != null && sequences.switchEase.Length > sequenceIndex)
				translation.switchEase = sequences.switchEase[sequenceIndex];
			if (sequences.switchEaseRotationOverride != null && sequences.switchEaseRotationOverride.Length > sequenceIndex)
				rotation.switchEaseOverrride = sequences.switchEaseRotationOverride[sequenceIndex];
			if (sequences.switchEaseScalingOverride != null && sequences.switchEaseScalingOverride.Length > sequenceIndex)
				scaling.switchEaseOverride = sequences.switchEaseScalingOverride[sequenceIndex];

			translation.allowSwitch = true;
			translation.inverseMagnitudeOnSwitch = false;
		}

		if (translation.type == TranslationType.ForwardAbsolute_B || translation.type == TranslationType.UpAbsolute_G || translation.type == TranslationType.StrafeAbsolute_R) {
			underlyingTranslationType = translation.type;
			translation.type = TranslationType.AbsoluteDelta;
		}

		if ((translation.type == TranslationType.Absolute || translation.type == TranslationType.AbsoluteDelta)) {
			if (transformTime < 0) {
				Debug.LogError("Absolute positioning needs a transformTime set. Stopping transform.");
				stopMe = true;
			} else
			{
				Vector3 useTranslation = translation.translationOrMagnitudeInX;

				if (underlyingTranslationType != TranslationType.PerFrame)
				{
					switch (underlyingTranslationType)
					{
						case TranslationType.ForwardAbsolute_B: useTranslation = transform.forward * translation.translationOrMagnitudeInX.x; break;
						case TranslationType.UpAbsolute_G: useTranslation = transform.up * translation.translationOrMagnitudeInX.x; break;
						case TranslationType.StrafeAbsolute_R: useTranslation = transform.right * translation.translationOrMagnitudeInX.x; break;
					}
				}

				if (translation.type == TranslationType.AbsoluteDelta) {
					if (rectTransform)
						useTranslation = rectTransform.anchoredPosition3D + useTranslation;
					else
					{
						if (translation.local)
							useTranslation = transform.localPosition + useTranslation;
						else
							useTranslation = transform.position + useTranslation;
					}
				}

				if (rectTransform)
				{
					absolutePosFullDelta = useTranslation - rectTransform.anchoredPosition3D;
					absolutePosLast = rectTransform.anchoredPosition3D;
				}
				else
				{
					if (translation.local)
					{
						absolutePosFullDelta = useTranslation - transform.localPosition;
						absolutePosLast = transform.localPosition;
					}
					else {
						absolutePosFullDelta = useTranslation - transform.position;
						absolutePosLast = transform.position;
					}

				}
				absolutePosTarget = useTranslation;
			}
		}

		if ((scaling.type == ScaleType.Absolute || scaling.type == ScaleType.AbsoluteDelta))
		{
			if (transformTime < 0)
			{
				Debug.LogError("Absolute scaling needs a transformTime set. Stopping transform.");
				stopMe = true;
			}
			else
			{
				// seems ok for RectTransform too
				Vector3 useScaling = scaling.scaling;
				if (scaling.type == ScaleType.AbsoluteDelta)
				{
					useScaling = transform.localScale + scaling.scaling;
				}

				absoluteScaleFullDelta = useScaling - transform.localScale;
				absoluteScaleLast = transform.localScale;
				absoluteScaleTarget = useScaling;
			}
		}

		if ((rotation.type == RotationType.Absolute || rotation.type == RotationType.AbsoluteDelta))
		{
			if (transformTime < 0)
			{
				Debug.LogError("Absolute rotation needs a transformTime set. Stopping transform.");
				stopMe = true;
			}
			else
			{
				// Seems ok for RectTransform as well
				Vector3 useRotation = rotation.rotation;
				if (rotation.type == RotationType.AbsoluteDelta)
				{
					if (rotation.local)
						useRotation = transform.localRotation.eulerAngles + rotation.rotation;
					else
						useRotation = transform.rotation.eulerAngles + rotation.rotation;
				}

				if (rotation.local) {
					absoluteRotLast = transform.localRotation.eulerAngles;
				}
				else {
					absoluteRotLast = transform.rotation.eulerAngles;
				}
				absoluteRotTarget = useRotation;
			}
		}


		if (start.jumpTime > 0 || start.randomStart == RandomStartType.Transform || start.randomStart == RandomStartType.AnimationAndTransform) {

			if (start.randomStart == RandomStartType.Transform || start.randomStart == RandomStartType.AnimationAndTransform) {
				if (transformTime > 0)
					start.jumpTime = Random.Range(0, transformTime);
				else {
					if (start.jumpTime > 0)
						start.jumpTime = Random.Range(0, start.jumpTime);
					else
						start.jumpTime = Random.Range(0, 1f);
				}

			} else {
				if (transformTime > 0 && start.jumpTime >= transformTime && settings.forceAccurateStartJump == false)
					start.jumpTime = transformTime;
			}

			if (!settings.forceAccurateStartJump)
				Updater(start.jumpTime);
			else
			{
				float timeSpent = 0;
				float orgSwitchRotTime = switchRotation.time;
				switchRotation.time = 0;
				do
				{
					float updateTime = 0.1f;
					if (start.jumpTime - timeSpent < 0.1f)
						updateTime = start.jumpTime - timeSpent;
					Updater(updateTime);

					timeSpent += updateTime;
				} while (timeSpent < start.jumpTime);
				switchRotation.time = orgSwitchRotTime;
			}
		}
	}

	private Vector3 GetTranslationVector() {
		Vector3 usedTranslation = translation.translationOrMagnitudeInX;

		switch(translation.type) {
			case TranslationType.Forward_B: usedTranslation = transform.forward * translation.translateHeadingMagnitude; break;
			case TranslationType.Up_G: usedTranslation = transform.up * translation.translateHeadingMagnitude; break;
			case TranslationType.Strafe_R: usedTranslation = transform.right * translation.translateHeadingMagnitude; break;
		}
		return new Vector3(usedTranslation.x, usedTranslation.y, usedTranslation.z);
	}


	void ResetRotation() {
		Vector3 tempRot = transform.rotation.eulerAngles;
		Vector3 tempLocalRot = transform.localRotation.eulerAngles;

		if (rotation.local)
			transform.localRotation = Quaternion.Euler(rotation.restoreOnEndSequence.x > 0 ? orgLocalRot.eulerAngles.x : tempLocalRot.x, rotation.restoreOnEndSequence.y > 0 ? orgLocalRot.eulerAngles.y : tempLocalRot.y, rotation.restoreOnEndSequence.z > 0 ? orgLocalRot.eulerAngles.z : tempLocalRot.z);
		else
			transform.rotation = Quaternion.Euler(rotation.restoreOnEndSequence.x > 0 ? orgRot.eulerAngles.x : tempRot.x, rotation.restoreOnEndSequence.y > 0 ? orgRot.eulerAngles.y : tempRot.y, rotation.restoreOnEndSequence.z > 0 ? orgRot.eulerAngles.z : tempRot.z);
	}

	void GetEaseValues(out float tt, out float mulMod, SwitchEaseType usedSwitchEase) {
		tt = transformTimer / transformTime;
		mulMod = 2f;

		if (usedSwitchEase == SwitchEaseType.InOut)
		{
			if (tt > 0.5f)
				tt = 1 - tt;
			mulMod = 4.1f;
		}
		else if (usedSwitchEase == SwitchEaseType.In)
			tt = 1 - tt;
		else if (usedSwitchEase == SwitchEaseType.BounceOut)
		{
			if (!switched)
				tt = 1 - tt;
		}
		else if (usedSwitchEase == SwitchEaseType.BounceIn)
		{
			if (switched)
				tt = 1 - tt;
		}
	}

	float GetAbsoluteEaseValues(SwitchEaseType usedSwitchEase) {
		if (transformTime <= 0)
			return 0;
		float tt = transformTimer / transformTime;
		switch (usedSwitchEase)
		{
			case SwitchEaseType.InOut: tt = Easing.Sinusoidal.InOut(tt); break;
			case SwitchEaseType.In: tt = Easing.Sinusoidal.In(tt); break;
			case SwitchEaseType.Out: tt = Easing.Sinusoidal.Out(tt); break;
			case SwitchEaseType.BounceIn: tt = Easing.Bounce.In(tt); break;
			case SwitchEaseType.BounceOut: tt = Easing.Bounce.Out(tt); break;
		}
		return tt;
	}

	void Update()
	{
		if (start.autoStart)
			Updater(Time.deltaTime);
	}

	void Updater (float deltaTime) {

		if (stopMe)
			return;

		if (start.waitTime > 0) {
			start.waitTime -= deltaTime;
			return;
		}

		if (!initialized) {
			if (transformTime >= 0)
				transformTimer = 0;

			animator = GetComponent<Animator> ();
			if (!animator)
				animator = GetComponentInChildren<Animator>();

			if (usingSequence == false || sequences.anim.Length < 1) {
				if (animator != null && start.animation != null && start.animation != string.Empty) {
					if (start.randomStart == RandomStartType.Animation || start.randomStart == RandomStartType.AnimationAndTransform)
						PlayAnim(start.animation, true);
					else
						PlayAnim(start.animation);
				}
			} else {
				if (usingSequence && sequences.anim != null && sequences.anim.Length > sequenceIndex && sequences.anim[sequenceIndex].Length > 0 && animator != null) {
					PlayAnim(sequences.anim[sequenceIndex]);
				}
			}
			initialized = true;
		}


		if (rotation.rotation != Vector3.zero || rotation.type == RotationType.Absolute)
		{
			SwitchEaseType sEt = rotation.switchEaseOverrride; if (sEt == SwitchEaseType.InheritFromTranslation) sEt = translation.switchEase;
			if ((rotation.type == RotationType.Absolute || rotation.type == RotationType.AbsoluteDelta) && transformTime >= 0)
			{
				Vector3 relRotation = rotation.rotation;
				if (rotation.type == RotationType.Absolute)
					relRotation -= absoluteRotLast;

				float tt = GetAbsoluteEaseValues(sEt);
				if (rotation.local) {
					transform.localRotation = Quaternion.Euler(absoluteRotLast);
				} else {
					transform.rotation = Quaternion.Euler(absoluteRotLast);
				}
				transform.Rotate(relRotation * tt, rotation.local ? Space.Self : Space.World);
			}
			else if (sEt != SwitchEaseType.Linear && transformTime > 0) {
				float tt, mulMod;
				GetEaseValues(out tt, out mulMod, sEt);
				transform.Rotate(rotation.rotation * 50 * deltaTime * tt * mulMod, rotation.local ? Space.Self : Space.World);
			} else
				transform.Rotate(rotation.rotation * 50 * deltaTime, rotation.local ? Space.Self : Space.World);
		}

		if (scaling.scaling != Vector3.zero || scaling.type == ScaleType.Absolute)
		{
			SwitchEaseType sEt = scaling.switchEaseOverride; if (sEt == SwitchEaseType.InheritFromTranslation) sEt = translation.switchEase;
			if ((scaling.type == ScaleType.Absolute || scaling.type == ScaleType.AbsoluteDelta) && transformTime >= 0)
			{
				float tt = GetAbsoluteEaseValues(sEt);
				transform.localScale = absoluteScaleLast + absoluteScaleFullDelta * tt;
			}
			else if (sEt != SwitchEaseType.Linear && transformTime > 0)
			{
				float tt, mulMod;
				GetEaseValues(out tt, out mulMod, sEt);
				transform.localScale += scaling.scaling * 10 * deltaTime * tt * mulMod;
			}
			else
				transform.localScale += scaling.scaling * 10 * deltaTime;
		}

		if (translation.translationOrMagnitudeInX != Vector3.zero || translation.type != TranslationType.PerFrame)
		{
			SwitchEaseType sEt = translation.switchEase; if (sEt == SwitchEaseType.InheritFromTranslation) sEt = SwitchEaseType.Linear;
			if ((translation.type == TranslationType.Absolute || translation.type == TranslationType.AbsoluteDelta) && transformTime >= 0)
			{
				float tt = GetAbsoluteEaseValues(sEt);
				if (!rectTransform)
				{
					if (translation.local)
						transform.localPosition = absolutePosLast + absolutePosFullDelta * tt;
					else
						transform.position = absolutePosLast + absolutePosFullDelta * tt;
				}
				else
					rectTransform.anchoredPosition3D = absolutePosLast + absolutePosFullDelta * tt;
			}
			else if (translation.switchEase != SwitchEaseType.Linear && transformTime > 0)
			{
				float tt, mulMod;
				GetEaseValues(out tt, out mulMod, sEt);
				Vector3 useTranslation = GetTranslationVector();
				if (translation.local)
					transform.localPosition += (useTranslation * deltaTime * tt * mulMod);
				else
					transform.position += (useTranslation * deltaTime * tt * mulMod);
			}
			else
			{
				Vector3 useTranslation = GetTranslationVector();
				if (translation.local)
					transform.localPosition += (useTranslation * deltaTime);
				else
					transform.position += (useTranslation * deltaTime);
			}
		}

		if (transformTimer <= transformTime) {
			transformTimer += deltaTime;
			if (transformTimer >= transformTime) {
				if (transformTimeType == TransformTimeType.SingleKill && !(usingSequence && sequenceIndex < sequences.time.Length - 1)) {
					Destroy(gameObject);
				}
				else if (transformTimeType == TransformTimeType.SingleRepeat && !(usingSequence && sequenceIndex < sequences.time.Length - 1))
				{
					Run(true, true, true, true, true);
					return;
				}
				else if (transformTimeType == TransformTimeType.SingleContinue && !(usingSequence && sequenceIndex < sequences.time.Length - 1))
				{
					Run();
					return;
				}
				else if (transformTimeType == TransformTimeType.SingleStop && !(usingSequence && sequenceIndex < sequences.time.Length - 1)) {
					SetLastPos();
					usingSequence = false;
					translation.translationOrMagnitudeInX = Vector3.zero;
					translation.translateHeadingMagnitude = 0;
					start.autoStart = false;
					rotation.rotation = scaling.scaling = Vector3.zero;
					if (animator != null && settings.singleStopAnim != null && settings.singleStopAnim.Length > 0)
						PlayAnim (settings.singleStopAnim);

				} else {
					if (translation.allowSwitch)
					{
						translation.translationOrMagnitudeInX = -translation.translationOrMagnitudeInX;
						if (translation.inverseMagnitudeOnSwitch)
							translation.translateHeadingMagnitude = -translation.translateHeadingMagnitude;
					}
					if (rotation.allowSwitch)
					{
						if (rotation.type != RotationType.Absolute)
							rotation.rotation = -rotation.rotation;
						else
							rotation.rotation = absoluteRotLast;
					}
					if (scaling.allowSwitch) {
						if (scaling.type != ScaleType.Absolute)
							scaling.scaling = -scaling.scaling;
						else
							scaling.scaling = absoluteScaleLast;
					}
					
					transformTimer = 0;
					if (switchRotation.rotation != Vector3.zero) {
						if (switchRotation.time <= 0)
							transform.Rotate(switchRotation.rotation, rotation.local ? Space.Self : Space.World);
						else
						{
							Quaternion oldr = transform.rotation;
							transform.Rotate(switchRotation.rotation, rotation.local ? Space.Self : Space.World);
							if (rotation.local)
								LeanTween.rotateLocal(gameObject, transform.rotation.eulerAngles, switchRotation.time).setEase(switchRotation.ease);
							else
								LeanTween.rotate(gameObject, transform.rotation.eulerAngles, switchRotation.time).setEase(switchRotation.ease);
							transform.rotation = oldr;

//							iTween.RotateAdd(gameObject, iTween.Hash("amount", switchRotation.rotation, "time", switchRotation.time, "easetype", switchRotation.ease, "space", rotation.local ? Space.Self : Space.World)); // does not work if there is a rigidbody on thsi object, even if it is kinematic!
						}
					}

					if (usingSequence)
					{
						sequenceIndex++; if (sequenceIndex >= sequences.time.Length) { sequenceIndex = 0; RestorePosOnSwitch(); }

						transformTime = sequences.time[sequenceIndex];

						if (sequences.translationOrMagnitudeInX != null && sequences.translationOrMagnitudeInX.Length > sequenceIndex)
						{
							if (translation.type != TranslationType.Forward_B && translation.type != TranslationType.Strafe_R && translation.type != TranslationType.Up_G)
								translation.translationOrMagnitudeInX = sequences.translationOrMagnitudeInX[sequenceIndex];
							else
								translation.translateHeadingMagnitude = sequences.translationOrMagnitudeInX[sequenceIndex].x;
						}
						if (sequences.switchRotation != null && sequences.switchRotation.Length > sequenceIndex)
							switchRotation.rotation = sequences.switchRotation[sequenceIndex];
						if (sequences.switchRotationTime != null && sequences.switchRotationTime.Length > sequenceIndex)
							switchRotation.time = sequences.switchRotationTime[sequenceIndex];

						if (sequences.rotation != null && sequences.rotation.Length > sequenceIndex)
							rotation.rotation = sequences.rotation[sequenceIndex];
						if (sequences.scaling != null && sequences.scaling.Length > sequenceIndex)
							scaling.scaling = sequences.scaling[sequenceIndex];
						if (sequences.switchEase != null && sequences.switchEase.Length > sequenceIndex)
							translation.switchEase = sequences.switchEase[sequenceIndex];
						if (sequences.switchEaseRotationOverride != null && sequences.switchEaseRotationOverride.Length > sequenceIndex)
							rotation.switchEaseOverrride = sequences.switchEaseRotationOverride[sequenceIndex];
						if (sequences.switchEaseScalingOverride != null && sequences.switchEaseScalingOverride.Length > sequenceIndex)
							scaling.switchEaseOverride = sequences.switchEaseScalingOverride[sequenceIndex];

						if (sequences.anim != null && sequences.anim.Length > sequenceIndex && sequences.anim[sequenceIndex].Length > 0 && animator != null)
						{
							PlayAnim(sequences.anim[sequenceIndex]);
						}
					}
					else
					{
						switched = !switched;
						if (!switched)
							RestorePosOnSwitch();
					}
				}

				if ((translation.type == TranslationType.Absolute || translation.type == TranslationType.AbsoluteDelta) && transformTime >= 0) {
					Vector3 useTranslation = translation.translationOrMagnitudeInX;

					if (underlyingTranslationType != TranslationType.PerFrame)
					{
						switch (underlyingTranslationType)
						{
							case TranslationType.ForwardAbsolute_B: useTranslation = transform.forward * translation.translationOrMagnitudeInX.x; break;
							case TranslationType.UpAbsolute_G: useTranslation = transform.up * translation.translationOrMagnitudeInX.x; break;
							case TranslationType.StrafeAbsolute_R: useTranslation = transform.right * translation.translationOrMagnitudeInX.x; break;
						}
					}

					if (translation.type == TranslationType.AbsoluteDelta)
					{
						useTranslation = absolutePosTarget + useTranslation;
					}

					if (usingSequence == false && transformTimeType != TransformTimeType.SingleRepeat && transformTimeType != TransformTimeType.SingleContinue && !(transformTimeType == TransformTimeType.Infinite && translation.allowSwitch==false) && (underlyingTranslationType == TranslationType.PerFrame || rotation.rotation == Vector3.zero))
						translation.translationOrMagnitudeInX = useTranslation = absolutePosLast;
					
					absolutePosFullDelta = useTranslation - absolutePosTarget;
					absolutePosLast = absolutePosTarget;

					SetLastPos();

					absolutePosTarget = useTranslation;
				}

				if ((scaling.type == ScaleType.Absolute || scaling.type == ScaleType.AbsoluteDelta))
				{
					Vector3 useScaling = scaling.scaling;
					if (scaling.type == ScaleType.AbsoluteDelta)
					{
						useScaling = absoluteScaleTarget + scaling.scaling;
					}

					absoluteScaleLast = absoluteScaleTarget;
					absoluteScaleFullDelta = useScaling - absoluteScaleTarget;
					absoluteScaleTarget = useScaling;
					//print("Current scale:" + absoluteScaleLast + "  To scale:" + useScaling + "  Delta:" + absoluteScaleFullDelta);
					//Debug.Break();
				}

				if ((rotation.type == RotationType.Absolute || rotation.type == RotationType.AbsoluteDelta)) {
					Vector3 useRotation = rotation.rotation;
					if (rotation.type == RotationType.AbsoluteDelta)
					{
						useRotation = absoluteRotTarget + rotation.rotation;
					}

					Vector3 relRotation = rotation.rotation;
					if (rotation.type == RotationType.Absolute)
						relRotation -= absoluteRotLast;

					if (rotation.local) {
						transform.localRotation = Quaternion.Euler(absoluteRotLast);
					} else {
						transform.rotation = Quaternion.Euler(absoluteRotLast);
					}
					transform.Rotate(relRotation, rotation.local ? Space.Self : Space.World);

					//absoluteRotLast = absoluteRotTarget;
					absoluteRotLast = transform.rotation.eulerAngles;

					absoluteRotTarget = useRotation;
					//print("Current rot:" + absoluteRotLast + "  To angle:" + useRotation + "  Delta:" + absoluteRotTarget);
					//Debug.Break();
				}

			}
		}
	}

	private void LateUpdate()
	{
		if (settings.lookAt)
		{
			transform.LookAt(settings.lookAt);
		}
	}


	void SetLastPos() {
		if ((translation.type == TranslationType.Absolute || translation.type == TranslationType.AbsoluteDelta) && transformTime >= 0 && translation.translationOrMagnitudeInX != Vector3.zero)
		{
			if (!rectTransform)
			{
				if (translation.local)
					transform.localPosition = absolutePosTarget;
				else
					transform.position = absolutePosTarget;
			}
			else
				rectTransform.anchoredPosition3D = absolutePosTarget;
		}
	}

	void RestorePosOnSwitch() {
		if (translation.restoreOnEndSequence != Vector3.zero)
		{
			if (translation.local)
				transform.localPosition = new Vector3(translation.restoreOnEndSequence.x > 0 ? orgLocalPos.x : transform.localPosition.x, translation.restoreOnEndSequence.y > 0 ? orgLocalPos.y : transform.localPosition.y, translation.restoreOnEndSequence.z > 0 ? orgLocalPos.z : transform.localPosition.z);
			else
				transform.position = new Vector3(translation.restoreOnEndSequence.x > 0 ? orgPos.x : transform.position.x, translation.restoreOnEndSequence.y > 0 ? orgPos.y : transform.position.y, translation.restoreOnEndSequence.z > 0 ? orgPos.z : transform.position.z);
		}

		if (scaling.restoreOnEndSequence != Vector3.zero) {
			transform.localScale = new Vector3(scaling.restoreOnEndSequence.x > 0 ? orgScale.x : transform.localScale.x, scaling.restoreOnEndSequence.y > 0 ? orgScale.y : transform.localScale.y, scaling.restoreOnEndSequence.z > 0 ? orgScale.z : transform.localScale.z);
		}

		if (rotation.restoreOnEndSequence != Vector3.zero)
		{
			if (switchRotation.rotation != Vector3.zero && switchRotation.time > 0)
				Invoke("ResetRotation", switchRotation.time);
			else
				ResetRotation();
		}
	}


	// SendMessage or Broadcast (or call directly) to have other objects affect the SimpleTransform ( string format: StartDelay_Animation_translationX_tY_tZ_translationSwitchTime_rotationX_rY_rZ_scaleX_sY_sX )
	// This part is far from nice (and also very far from covering all cases). Refactor to make string more obvious with named params, or replace with an interface or similar
	public void TransformChange(string msg) {

		if (msg != null && msg.Length > 0) {
			string [] msgParts = msg.Split (new char [] { '_' });

			msgParam = msg;
			float timeChange = float.Parse(msgParts[0]);
			if (timeChange <= 0)
				ChangeT();
			else
				Invoke ("ChangeT", timeChange);
		}
	}
	private string msgParam;
	void ChangeT() {
		string [] msgParts = msgParam.Split (new char [] { '_' });

		usingSequence = false;

		transformTimer = 0;

		if (animator != null) {
			if (start.randomStart == RandomStartType.Animation || start.randomStart == RandomStartType.AnimationAndTransform)
				PlayAnim(msgParts[1], true);
			else
				PlayAnim (msgParts [1]);
		}

		if (msgParts.Length >= 5) {
			translation.translationOrMagnitudeInX = new Vector3 (float.Parse(msgParts[2]), float.Parse(msgParts[3]), float.Parse(msgParts[4]));
		}

		if (msgParts.Length >= 6) {
			transformTime = float.Parse(msgParts[5]);
		}

		if (msgParts.Length >= 9) {
			rotation.rotation = new Vector3 (float.Parse(msgParts[6]), float.Parse(msgParts[7]), float.Parse(msgParts[8]));
		}

		if (msgParts.Length >= 12)
		{
			scaling.scaling = new Vector3(float.Parse(msgParts[9]), float.Parse(msgParts[10]), float.Parse(msgParts[11]));
		}

		start.autoStart = true;
		sequenceIndex = 0;
		DoStart();

	}

	public override void OnExternalHit() {
		if (settings.stopOnExternalHit)
			stopMe = true;
	}
	public override void OnExternalMinorHit() {}

}
