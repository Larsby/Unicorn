using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInfo : MonoBehaviour {
	public int _price = 0;
	bool _unlocked;
	public string characterName;
	public AudioClip _hitClip = null;
	public AudioClip _wingClip = null;
	public int maxHardLevel;
	void SetupAudioLoop(AudioClip clip) {
		AudioSource source = gameObject.AddComponent<AudioSource> ();
		source.clip = clip;
		source.loop = true;
		source.volume = 0.3f;
		source.pitch = 0.4f;
		source.Play ();
	}

	public AudioClip  wingClip
	{
		get { return _wingClip; }
		set { 
			_wingClip = value; 
			SetupAudioLoop (_wingClip);	
		}
	}

	public AudioClip  hitClip
	{
		get { return _hitClip; }
		set { _hitClip = value; }
	}

	public int price
	{
		get { return _price; }
		set { _price = value; }
	}
	public bool unlocked
	{
		get { return _unlocked; }
		set { _unlocked = value; }
	}

	// Use this for initialization
	void Start () {
		if (_wingClip != null) {
			SetupAudioLoop (_wingClip);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
