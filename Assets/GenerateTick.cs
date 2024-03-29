using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class GenerateTick : MonoBehaviour
{

	public delegate void TickAction ();

	public static event TickAction OnTick;



	public float BPM = 120;


	private double increment;

	private double sampling_frequency = 44100;
	private int pos = 0;

	double clapCheck;





	 

	bool shallSend = false;


	void Start ()
	{
 
		sampling_frequency = AudioSettings.outputSampleRate;
		clapCheck = (sampling_frequency / BPM) * 60f;


	}


	void Update ()
	{


		if (shallSend) {
			if (OnTick != null)
				OnTick ();
			shallSend = false;
		}

	}

	void OnAudioFilterRead (float[] data, int channels)
	{


		for (var i = 0; i < data.Length; i = i + channels) {

			pos++;
			if (pos >= clapCheck) {
				shallSend = true;
				pos = 0;

			}

		}
	}




}


