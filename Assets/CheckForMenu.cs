using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckForMenu : MonoBehaviour {

	public AudioLowPassFilter ALPF;
	private bool menyVisible = false;
	public GameObject GameOverPanel;
	enum CutOff {
		LOW=510,
		HIGH=23000
	}
	private CutOff cutoff;
	public float amount = 500;
	void SetCutOff(bool doCuttoff) {
		if (doCuttoff) {
			cutoff = CutOff.LOW;
			//ALPF.cutoffFrequency = 500;
		} else {
			cutoff = CutOff.HIGH;
			//ALPF.cutoffFrequency = 22000;
		}
	}

	// Use this for initialization
	void Start () {
 
		SetCutOff (menyVisible);
			
	}

	public void SetmenuVisible(bool inBool)
	{
		menyVisible = inBool;
		SetCutOff (menyVisible);
	}

	// Update is called once per frame
	void Update () {
		//float limit = 
	
		if (GameOverPanel.activeSelf == true) {
		
			SetmenuVisible (true);
		}

		if (GameOverPanel.activeSelf == false) {

			SetmenuVisible (false);

		}
		if (cutoff == CutOff.LOW && ALPF.cutoffFrequency >(float)CutOff.LOW + amount) {
			ALPF.cutoffFrequency = ALPF.cutoffFrequency - amount;
		} else if (cutoff == CutOff.LOW && ALPF.cutoffFrequency >(float)CutOff.LOW) {
			ALPF.cutoffFrequency = (float)CutOff.LOW;
		}
		if (cutoff == CutOff.HIGH && ALPF.cutoffFrequency < (float)CutOff.HIGH - amount) {
			ALPF.cutoffFrequency = ALPF.cutoffFrequency + amount;
		} else if (cutoff == CutOff.HIGH) {
		//	ALPF.cutoffFrequency = (float)CutOff.HIGH;
		}


		//Man kan ha en koll här för när gamestatet är i menu eller när det ändrar till ngn meny, eller bara kalla det från bow.cs liksom altså
	}
}
