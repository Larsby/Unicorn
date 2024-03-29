
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
[Serializable]
public class PastilleVector3
{
	public float x;
	public float y;
	public float z;
}

[Serializable]
public class TargetActionData
{
	public PastilleVector3 from;
	public PastilleVector3 to;
	public int state;
	public int indexID;
	public int useSameValue; 
	public PastilleVector3 fromRandom ;
	public PastilleVector3 stopRandom ;
	public bool useRandom;

    public bool useShadow = true;
    public int difficulty = 0;


}

[Serializable]
public class TargetActionLevel
{
	public TargetActionData[] actions;
	public int[] allowedTargetIndexes;
}

[Serializable]
public class TargetActionGameData
{
	public float version;
	public TargetActionLevel[] levels;
}
	public class LevelDataClasses
	{

	 public const string saveName = "unicorndata.dat";
	public  static TargetActionGameData gameData;
	//public  static  int MAXLEVELS = 360;
    public static int MAXLEVELS = 50; 

	public  static  int MAXACTIONS = 10;
	 public static  int MAXTARGET = 10;
	public static string FILE_PATH = "";
	public  static float version = 1.0f;
		public LevelDataClasses ()
		{
		}
	public static Vector3 GetRandomRange(Vector3 startFromVector, Vector3 stopToVector) {

		float x = UnityEngine.Random.Range (startFromVector.x, stopToVector.x);
		float y = UnityEngine.Random.Range (startFromVector.y, stopToVector.y);
		return new Vector3 (x, y, 1.0f);


	}


	public static Vector3 PastilleVectorToVec3 (PastilleVector3 vec)
	{
		if(vec == null)
		{
			return Vector3.zero;
		}
		return new Vector3 (vec.x, vec.y, vec.z);
	}

	public static PastilleVector3 Vector3ToPastilleVec3 (Vector3 vec)
	{
		PastilleVector3 v = new PastilleVector3 ();
		v.x = vec.x;
		v.y = vec.y;
		v.z = vec.z;
		return v;

	}
	}


