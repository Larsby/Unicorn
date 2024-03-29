using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
	public AudioClip[] stringRelease;
	public AudioClip[] arrowSwoosh;
	public AudioClip[] popcornSounds;
	public AudioClip[] arrowHitSounds;
	public AudioClip[] coins;
	 
	public float randMax = 0.5f;
	private AudioSource source;
	private bool enabled = false;
	private bool init = false;
	// Use this for initialization
	void Start ()
	{
		source = GetComponent<AudioSource> ();
		enabled = LoadSFXEnabled ();
	}

	public bool  LoadSFXEnabled() {
		init = true;
		if (PlayerPrefs.HasKey ("SFX") == false) {
			PlayerPrefs.SetInt ("SFX", 1);
			PlayerPrefs.Save ();
		}
		int enabled = PlayerPrefs.GetInt("SFX");
		if (enabled == 0)
			return false;
		return true;

	}

	public void EnableSFX(bool enable) {
		this.enabled = enable;
		int value = enable == true ? 1 : 0;
		PlayerPrefs.SetInt ("SFX", value);
		PlayerPrefs.Save ();
	}
	public void PlaySFX() {
		EnableSFX (true);
	}
	public void StopSFX() {
		EnableSFX (false);
	}
	public void playClip(AudioClip clip) {
		if (init == false) {
			enabled = LoadSFXEnabled ();
		}
		if (!enabled)
			return;
		
		source.pitch = Random.Range (1.0f - randMax, 1.0f + randMax);
		source.PlayOneShot (clip);
	}
	public void playEffect (string name)
	{
		if (init == false) {
			enabled = LoadSFXEnabled ();
		}
		if (!enabled)
			return;
		source.pitch = Random.Range (1.0f - randMax, 1.0f + randMax);


		switch (name) {

		case ("stringRelease"):
			{
				source.PlayOneShot (stringRelease [Random.Range (0, stringRelease.Length)]);
				break;
			}
		case ("swoosh"):
			{
				source.PlayOneShot (arrowSwoosh [Random.Range (0, arrowSwoosh.Length)]);
				break;
			}
		case ("popcorns"):
			{
				source.PlayOneShot (popcornSounds [Random.Range (0, popcornSounds.Length)]);
				break;
			}
		case ("hit"):
			{
				source.PlayOneShot (arrowHitSounds [Random.Range (0, arrowHitSounds.Length)]);
				break;
			}
		case ("coins"):
			{
				source.PlayOneShot (coins [Random.Range (0, coins.Length)]);
				break;
			}
		default:
			{
				Debug.Log ("No sound for: " + name);
				break;
			}
		 

		}
	}

	// Update is called once per frame
	void Update ()
	{


		
	}
}
