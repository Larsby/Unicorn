using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour, ScrollSnapItemChanged {

	public Text nameText;
	public GameObject charItemPrefab;
	public GameObject scrollView;
	public Button selectButton;
	public Text creditsText;
	public Button previousButton;
	public Button nextButton;

	private EventSystem eventSystem = null;

	// private AsyncOperation closeOperation = null;

	private CharacterManager characterManager = null;
	private int avatarIndex = -1;
	private List<GameObject> charItems = new List<GameObject> ();
	private List<CharacterInfo> characters = new List<CharacterInfo> ();

	private bool showSelectAnimation = false;

	void Awake() {
		//SoundManager.Create ();

		characterManager = CharacterManager.instance;
		TargetManager targetManager = GetComponent<TargetManager> ();

		for (int i = 0; i < characterManager.characterPrefabs.Count; i++) {
			GameObject charItem = Instantiate (charItemPrefab, scrollView.transform, false);

			GameObject character = Instantiate (characterManager.characterPrefabs[i], charItem.transform, false);
			CharacterInfo player = character.GetComponentInChildren<CharacterInfo> ();
			MakingItHarder harder;
			harder = character.GetComponent<MakingItHarder> ();
			if (harder != null) {
				harder.enabled = false;
			}
			character.transform.localPosition = new Vector3 (0, 10, -252.6f);
			character.transform.localScale = new Vector3 (15, 15, 200);
			character.transform.localRotation = Quaternion.Euler (0, 180, 0);
		//	charItem.transform.localScale = new Vector3 (0.2f, 0.2f, 1.0f);
/*
			if (player != null) {
				player.disableRagdoll ();
				player.SetKinematic ();
				player.TrigAnim ("Idle");
			}
*/
			SkinnedMeshRenderer smr = character.GetComponentInChildren<SkinnedMeshRenderer> ();
			Button buyButton = charItem.GetComponentInChildren<Button> ();
			Transform t = charItem.transform.Find ("PrizeText");

			bool unlocked = targetManager.IsAvatarUnlocked (i);
		//	unlocked = true;
			if (!unlocked) {
				if (smr != null) {
					smr.material.color = GameUtil.IntColor (40, 40, 40);
				}
				if (buyButton != null) {
					buyButton.onClick.AddListener (BuyButton);
					if (StaticManager.GetNumberOfCredits () < player.price)
						buyButton.interactable = false;
				}
				if (t != null) {
					Text prizeText = t.gameObject.GetComponent<Text> ();
					if (prizeText != null)
						prizeText.text = player.price + "";
				}

			} else {
				if (t != null)
					t.gameObject.SetActive (false);
				if (buyButton != null)
					buyButton.gameObject.SetActive (false);
			}

			if (characterManager.characterPrefabs[i].name == "Ninja Go Vanish" && smr != null) // hack to make this character appear behind the buy button and prize text, because the default settings for shader Toon->BasicTransparent renders it above
				smr.material.renderQueue = 2000; // "Geometry"
			/*
			Transform tGround = charItem.transform.Find ("Ground"); // position the ground where doll's toes are. The Animator can be used to find all bodyparts, e.g. the toes, without using names. Taken from ToonDollHelper code.
			Animator animator = player.gameObject.GetComponent<Animator> ();
			if (tGround != null && animator != null) {
				Transform tFoot = animator.GetBoneTransform (HumanBodyBones.LeftToes);
				if (tFoot != null)
					tGround.position = GameUtil.SetY (tGround.position, tFoot.position.y - 1);
			}
			*/
			creditsText.text = "" + StaticManager.GetNumberOfCredits();

			ParticleSystem ps = character.GetComponentInChildren<ParticleSystem> ();
			if (ps != null) {
				var main = ps.main;
				main.startDelay = 0.06f;
				main.gravityModifier = 1; // Disco Roboto
				if (character.name.Substring (0, 2) == "Sk") // Skelly Mel
					main.gravityModifier = -8;
				main.scalingMode = ParticleSystemScalingMode.Hierarchy;
			}

			charItems.Add (charItem);
			characters.Add (player);
		}

		eventSystem = FindObjectOfType<EventSystem> ();

		SetOrRestoreShadowSettings (false);
	}

	private ShadowProjection shadow_oldProjection;
	private int shadow_oldCascades;
	private float shadow_oldDistance;

	private void SetOrRestoreShadowSettings(bool restore) {
		if (!restore) {
			shadow_oldProjection = QualitySettings.shadowProjection;
			shadow_oldDistance = QualitySettings.shadowDistance;
			shadow_oldCascades = QualitySettings.shadowCascades;
			QualitySettings.shadowProjection = ShadowProjection.StableFit;
			QualitySettings.shadowDistance = 75;
			QualitySettings.shadowCascades = 1;
		} else {
			QualitySettings.shadowProjection = shadow_oldProjection;
			QualitySettings.shadowDistance = shadow_oldDistance;
			QualitySettings.shadowCascades = shadow_oldCascades;
		}
	}

	void Start () {
	}

	void Update () {
		
		// if (closeOperation != null) { /* supposed to do sth? // closeOperation.isDone ? */ }
	}


	private void DelayedLoad() {
		SetOrRestoreShadowSettings (true);
		StaticManager.PopScene ();
	}

	public void PlayButton() {
		/*
		if (closeOperation != null)
			return;

		// Debug.Log ("Go back to previous scene somehow here");
		// Debug.Log ("https://gamedev.stackexchange.com/questions/116698/resuming-a-previous-scene-from-a-current-scene-using-the-unity3d");
		closeOperation = SceneManager.UnloadSceneAsync (SceneManager.GetActiveScene ().name);
		if (closeOperation != null)
			closeOperation.allowSceneActivation = true;
		*/

		StaticManager.SetSelectedAvatarIndex (avatarIndex);

		if (showSelectAnimation) {
		//	characters [avatarIndex].TrigAnim ("Selected");
			if (eventSystem != null)
				eventSystem.gameObject.SetActive (false); // turn off all interaction with UI
			Invoke ("DelayedLoad", 1.3f);
		} else {
			SetOrRestoreShadowSettings (true);
			StaticManager.PopScene ();
		}
	}

	public void BackButton() {
		SetOrRestoreShadowSettings (true);
		StaticManager.PopScene ();
	}

	public void BuyButton() {
		StaticManager.SetUnlockedAvatarIndex (avatarIndex);
		StaticManager.UnlockAvatar(avatarIndex, characters[avatarIndex].price);
		StaticManager.PushScene ();
		SceneManager.LoadScene ("NewCharacter");
	}

	public void NotifyItemChanged(int index) {
		CharacterInfo info = characterManager.characterPrefabs [index].gameObject.GetComponent<CharacterInfo>();
		if(info != null) {
		nameText.text = info.characterName;
		} else {
			nameText.text = "";
			Debug.Log("Error character with index"+ index+ " does not have a charactarinfo component");
		}
		avatarIndex = index;
		selectButton.interactable = StaticManager.IsAvatarUnlocked (index);
		previousButton.gameObject.SetActive (index > 0);
		nextButton.gameObject.SetActive (index < characterManager.characterPrefabs.Count - 1);
	}

	public int GetStartPage() {
		if (StaticManager.GetUnlockedAvatarIndex () >= 0) {
			int index = StaticManager.GetUnlockedAvatarIndex ();
			StaticManager.SetUnlockedAvatarIndex (-1);
			return index;
		} else {
			return StaticManager.GetSelectedAvatarIndex ();
		}
	}

}
