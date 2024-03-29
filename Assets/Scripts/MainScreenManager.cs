using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainScreenManager : MonoBehaviour
{

	public Button settingsButton;
	public Sprite showSettingsImage;
	public Sprite hideSettingsImage;
	public GameObject settingsPanel;
	public Text creditsText;
	private bool showSettings = false;

	public GameObject adPanel;
	public GameObject hiscorePanel;

	public GameObject musicToggleButton;
	public GameObject sfxToggleButton;

	private ScreenFader screenFader;

	private static GameObject musicPlayerStatic = null;
	private AudioSource musicPlayer;

	public Text hiscoreText;

	void Awake()
	{
#if UNITY_IOS
		LeaderboardManager.AuthenticateToGameCenter();
#endif
		StaticManager.showGameIntro = false;

		GameObject[] objs = GameObject.FindGameObjectsWithTag("Music");

		if (objs.Length > 1)
		{
			foreach (GameObject g in objs)
				if (g != musicPlayerStatic)
					Destroy(g);
		}
		else
		{
			musicPlayerStatic = objs[0];
			DontDestroyOnLoad(musicPlayerStatic);
		}

		if (PlayerPrefs.HasKey("Music") == false)
		{
			PlayerPrefs.SetInt("Music", 1);
			PlayerPrefs.Save();
		}

		musicPlayer = musicPlayerStatic.GetComponent<AudioSource>();
		if (!musicPlayer.isPlaying)
		{
			bool playMusic = PlayerPrefs.GetInt("Music") == 1 ? true : false;
			if (playMusic)
				musicPlayer.Play();
		}

		if (!PlayerPrefs.HasKey("Score"))
			PlayerPrefs.SetInt("Score", 0);

		hiscoreText.text = "" + PlayerPrefs.GetInt("Score");
	}

	public void Rate()
	{
#if UNITY_IOS
		Application.OpenURL("https://itunes.apple.com/us/app/appName/id1221829296?mt=8&action=write-review");
#endif
#if UNITY_ANDROID
		Application.OpenURL("http://play.google.com/store/apps/details?id=se.pastille.unicorn_2018");
#endif
	}
	void Start () {
		screenFader = FindObjectOfType<ScreenFader> ();

		creditsText.text = "" + StaticManager.GetNumberOfCredits();
	}
		
	void Update () {
	}
	public void GoHome() {
		 
		Application.OpenURL ("http://www.pastille.se");
		
	}

	public void PlayUnicone() {
		if (PlayerPrefs.HasKey ("icecream") ) {
			musicPlayer.Stop();
			bowAndArrow.useIcecreamMode = true;
			SceneManager.LoadScene ("level_0_0");
		} else {
			SceneManager.LoadScene ("BuyCredits");
		}
	}

	public void PlayGameIntro() {
		musicPlayer.Stop();
		StaticManager.showGameIntro = true;
		PlayerPrefs.SetInt ("fromMain", 1);
		PlayerPrefs.Save ();
		SceneManager.LoadScene ("ShowAd");
	}

	public void ShowLeadboard() {
#if UNITY_IOS && !UNITY_EDITOR
		LeaderboardManager.ShowLeaderboard ();
#else
		SceneManager.LoadScene ("HighScore");
#endif
	}

	public void PlayAction() {
		musicPlayer.Stop();
		bowAndArrow.useIcecreamMode = false;
		if (StaticManager.usePlayLevelScreen) {
			StaticManager.SetSceneToLastPlayedScene ();
			SceneManager.LoadScene ("PlayLevel");
		} else
			StaticManager.LoadLastPlayedScene ();
	}

	public void PlayButton() {
		if (screenFader == null)
			PlayAction ();
		else
			screenFader.FadeOut (PlayAction);
	}

	public void LevelsButton() {
		SceneManager.LoadScene ("WorldSelect");
	}


	public void AddCreditsButton() {
		StaticManager.PushScene ();
		SceneManager.LoadScene ("BuyCredits");
	}

	public void ThrowersButton() {
		StaticManager.PushScene();
		SceneManager.LoadScene ("ThrowerSelect");
	
	}
	public void CharsButton() {
		// test   SceneManager.LoadScene ("NewCharacter", LoadSceneMode.Additive);  // if we want to use additive solution with scene on scene

		StaticManager.PushScene();
		SceneManager.LoadScene ("CharSelect");
	}
		
	public void ToggleMusicButton() {

		StaticManager.ToggleMusic ();
		SetButtonState (musicToggleButton, StaticManager.IsMusicOn());

		if (StaticManager.IsMusicOn ()) {
			PlayerPrefs.SetInt ("Music", 1);
			PlayerPrefs.Save ();
			musicPlayer.Play();
		} else {
			PlayerPrefs.SetInt ("Music", 0);
			PlayerPrefs.Save ();
			musicPlayer.Stop();
		}
	}

	public void ToggleSfxButton() {
		StaticManager.ToggleSfx ();
		int val = StaticManager.IsSfxOn () ? 1 : 0;
		PlayerPrefs.SetInt ("SFX", val);
		PlayerPrefs.Save ();
		SetButtonState (sfxToggleButton, StaticManager.IsSfxOn());
	}

	public void SettingsButton() {
		showSettings = !showSettings;
		if (showSettings) {
			settingsPanel.SetActive (true);
			settingsButton.image.sprite = hideSettingsImage;
			SetButtonState (sfxToggleButton, StaticManager.IsSfxOn());
			SetButtonState (musicToggleButton, StaticManager.IsMusicOn());
			adPanel.SetActive(false);
			hiscorePanel.SetActive(false);
		}
		else { 
			settingsPanel.SetActive (false);
			settingsButton.image.sprite = showSettingsImage;
			adPanel.SetActive(true);
			hiscorePanel.SetActive(true);
		}
	}

	private void SetButtonState (GameObject obj, bool active) {
		int activeIndex = active == true ? 0 : 1;
		Image img = obj.transform.GetChild (1 - activeIndex).gameObject.GetComponent<Image> ();
		obj.transform.GetChild (1 - activeIndex).gameObject.SetActive (false);
		img.enabled = false;
		obj.transform.GetChild (activeIndex).gameObject.SetActive (true);
		Image img2 = obj.transform.GetChild (activeIndex).gameObject.GetComponent<Image> ();
		img2.enabled = true;
	}

	public void ResetLevelsButton() {
		StaticManager.Load (true);
		Debug.Log ("Reset saved data");
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}

	public void UpdateLevelsButton() {
		StaticManager.UpdateLoad ();
		Debug.Log ("Updated saved data");
	}

}
