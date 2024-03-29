using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomSound : MonoBehaviour
{
	public AudioClip[] clips;
	public AudioClip current;
	private AudioSource source;
	public float randMax = 0;
	// Use this for initialization
	void Start ()
	{
		source = GetComponent<AudioSource> ();
	}

	public void Play ()
	{

		source = GetComponent<AudioSource> ();
		source.enabled = true;
		if (source.isPlaying) {
			//source.Stop ();

		}
		if (clips == null)
			return;
		if (clips.Length == 0)
			return;
		
		source.clip = clips [Random.Range (0, clips.Length)];
		if (source.clip != null) {
			source.Play ();
		}

	

	}
}
