using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
public class IntroManager : MonoBehaviour {

	public GameObject skipButton;
	public GameObject videoCanvas;
	public GameObject splashCanvas;

	private bool showSplash = true;

	void PlayIntro() {
		skipButton.SetActive(true);

		VideoPlayer p = GetComponentInChildren<VideoPlayer> ();

		if (p != null) {
			p.Play ();
			p.loopPointReached += EndReached;
		} else {
			Debug.Log ("Can't find videoplayer, nn00b");
		}
	}

	void Start () {
		if (PlayerPrefs.HasKey ("IntroPlayed")) {
			if (PlayerPrefs.HasKey ("fromMain")) {
				if (PlayerPrefs.GetInt ("fromMain") == 1) {
					PlayerPrefs.SetInt ("fromMain", 0);
					PlayerPrefs.Save ();
					PlayIntro ();
					showSplash = false;
					return;
				}
			}

			LoadMain();
		}
		else {
			PlayerPrefs.SetInt ("IntroPlayed", 1);
			PlayerPrefs.Save ();
		}
		PlayIntro ();

	}
	public void GoToMain() {
		LoadMain();
	}

	void EndReached(UnityEngine.Video.VideoPlayer vp)
	{
		LoadMain();
	}

	void Update () {
	}


	void LoadMain()
	{
		if (showSplash)
		{
			videoCanvas.SetActive(false);
			splashCanvas.SetActive(true);
		}
		Invoke("LoadScene", 0.05f);
	}

	void LoadScene()
	{
		SceneManager.LoadScene("Main");
	}

}
