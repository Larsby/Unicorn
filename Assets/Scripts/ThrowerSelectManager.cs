using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
public class ThrowerSelectManager : MonoBehaviour, ScrollSnapItemChanged {

	public Text nameText;
	public GameObject charItemPrefab;
	public GameObject scrollView;
	public Button selectButton;
	public Text creditsText;
	public Button previousButton;
	public Button nextButton;
	public Sprite unlockSprite;
	private EventSystem eventSystem = null;

	// private AsyncOperation closeOperation = null;

	public CharacterManager characterManager = null;
	public ThrowerManager manager;
	private int avatarIndex = -1;
	private List<GameObject> charItems = new List<GameObject> ();
	private List<CharacterInfo> characters = new List<CharacterInfo> ();

	private bool showSelectAnimation = false;

	void Awake() {
		//SoundManager.Create ();


		TargetManager targetManager = GetComponent<TargetManager> ();

		for (int i = 0; i < manager.throwers.Length; i++) {
			GameObject charItem = Instantiate (charItemPrefab, scrollView.transform, false);

			GameObject character = Instantiate (manager.throwers[i], charItem.transform, false);
			CharacterInfo player = character.GetComponentInChildren<CharacterInfo> ();
			MakingItHarder harder;
			harder = character.GetComponent<MakingItHarder> ();
			if (harder != null) {
				harder.enabled = false;
			}
			character.transform.localPosition = new Vector3 (-4.8f, 10, -252.6f);
			character.transform.localScale = new Vector3 (15, 15, 200);
			character.transform.localRotation = Quaternion.Euler (0, 0, 0);
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
			unlocked = true;
			bool bought = false;
			if (i == 0) {
				unlocked = true;
				bought = true;
			}
			if (unlocked && targetManager.IsAvatarBought (i)) {
				bought = true;
			}
		//	bought = true;
			if (buyButton && unlocked && bought == false) {
				buyButton.gameObject.SetActive (true);
				buyButton.interactable = true;
				buyButton.gameObject.transform.GetChild (1).gameObject.SetActive (false);
				buyButton.onClick.AddListener (BuyButton);
				if (StaticManager.GetNumberOfCredits () < player.price) {
					buyButton.interactable = false;
				}

			}
			if (buyButton && bought) {
				buyButton.gameObject.SetActive (false);
			}

			if (t != null) {
				Text prizeText = t.gameObject.GetComponent<Text> ();
				if (prizeText != null)
					prizeText.text = ""+player.price;
				if (bought) { 
					prizeText.gameObject.SetActive (false);
				}
			}




			creditsText.text = "" + StaticManager.GetNumberOfCredits();

			ParticleSystem ps = character.GetComponentInChildren<ParticleSystem> ();
			if (ps != null) {
				var main = ps.main;
				main.startDelay = 0.06f;
			
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
		TargetManager targetManager = GetComponent<TargetManager> ();
		/*
		if (closeOperation != null)
			return;

		// Debug.Log ("Go back to previous scene somehow here");
		// Debug.Log ("https://gamedev.stackexchange.com/questions/116698/resuming-a-previous-scene-from-a-current-scene-using-the-unity3d");
		closeOperation = SceneManager.UnloadSceneAsync (SceneManager.GetActiveScene ().name);
		if (closeOperation != null)
			closeOperation.allowSceneActivation = true;
		*/
		/*
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
		*/
		if (manager.GetAvatar (avatarIndex) != null) {

			bool unlocked = targetManager.IsAvatarUnlocked (avatarIndex);

			if (unlocked && targetManager.IsAvatarBought (avatarIndex)) {
				manager.SetSelectedIndex (avatarIndex);
				SceneManager.LoadScene ("Main");
			}

		}

	}

	public void BackButton() {
		SetOrRestoreShadowSettings (true);
		StaticManager.PopScene ();
	}

	public void BuyButton() {
		StaticManager.SetUnlockedAvatarIndex (avatarIndex);
		manager.UnlockAvatar (avatarIndex);
		manager.SetSelectedIndex (avatarIndex);
		StaticManager.UnlockAvatar(avatarIndex, characters[avatarIndex].price);
		TargetManager targetManager = GetComponent<TargetManager> ();
		creditsText.text = "" + StaticManager.GetNumberOfCredits();
		SceneManager.LoadScene ("Main");
	}

	public void NotifyItemChanged(int index) {
		CharacterInfo info = manager.throwers [index].gameObject.GetComponent<CharacterInfo>();
		if(info != null) {
			nameText.text = info.characterName;
		} else {
			nameText.text = "";
			Debug.Log("Error character with index"+ index+ " does not have a charactarinfo component");
		}
		avatarIndex = index;
		selectButton.interactable = StaticManager.IsAvatarUnlocked (index);

		TargetManager targetManager = GetComponent<TargetManager> ();
		if (selectButton.interactable && targetManager.IsAvatarBought (index)) {
			
		}
			
		/*
		 * 
		 * 
		 * if (manager.GetAvatar (avatarIndex) != null) {

			bool unlocked = targetManager.IsAvatarUnlocked (avatarIndex);

			if (unlocked && targetManager.IsAvatarBought (avatarIndex)) {
				manager.SetSelectedIndex (avatarIndex);
				SceneManager.LoadScene (1);
			}

		}*/

		previousButton.gameObject.SetActive (index > 0);

		bool active = index < manager.throwers.Length - 1;
		nextButton.gameObject.SetActive (active);
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