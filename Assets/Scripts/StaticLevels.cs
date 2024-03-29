using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticLevels : MonoBehaviour {

	public static int[] levelsPerWorld = { 3, 3, 3, 3, 0, 0, 0, 0, 0, 0 };

	public static int[] requiredStarsPerWorld = { 0,0,0,0,0,0,0,0,0,0 }; // most likely set this manually later (and remove the for loop setting this in code in StaticManager.Load)
}
