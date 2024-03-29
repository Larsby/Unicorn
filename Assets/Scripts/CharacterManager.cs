using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour {

	public GameObject [] unorderedCharacterPrefabs = null;
	public List<GameObject> characterPrefabs = null;
	public ThrowerManager manager;
	private bool initialized = false;
	public static CharacterManager instance = null;

	void Awake ()
	{
		if (instance == null) {
			instance = this;
			Initialize ();
			DontDestroyOnLoad (gameObject);

		} else if (instance != this)
			Destroy (gameObject);
	}

	public void Initialize ()
	{

	}

	public GameObject GetAvatar(int index) {
		if (index < 0 || index >= manager.throwers.Length) {
			Debug.Log ("Invalid avatar index, returning default avatar");
			return  manager.throwers[0];
		}

		return  manager.throwers[index];
	}
		
}
