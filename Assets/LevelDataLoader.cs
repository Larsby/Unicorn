using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
 


public class LevelDataLoader : MonoBehaviour
{
    private static TargetActionGameData gameData;
    private bool loaded;
    public bool initializeAtStart = false;
    // Use this for initialization
    void Awake()
    {
        loaded = false;
        DontDestroyOnLoad(this);
    }
    void Start()
    {
        if (initializeAtStart)
        {
            gameData = Load();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void Erase()
    {
        if (File.Exists(LevelDataClasses.FILE_PATH))
        {
            File.Delete(LevelDataClasses.FILE_PATH);
        }
    }
    private void CopyGameData(TargetActionGameData fileGameData)
    {
        for (int i = 0; i < fileGameData.levels.Length; i++)
        {
            gameData.levels[i] = fileGameData.levels[i];
            for (int j = 0; j < LevelDataClasses.MAXACTIONS; j++)
            {
                gameData.levels[i].actions[j] = fileGameData.levels[i].actions[j];
            }
            for (int k = 0; k < LevelDataClasses.MAXTARGET; k++)
            {
                if (fileGameData.levels[i].allowedTargetIndexes != null)
                {
                    if (k < fileGameData.levels[i].allowedTargetIndexes.Length)
                    {
                        gameData.levels[i].allowedTargetIndexes[k] = fileGameData.levels[i].allowedTargetIndexes[k];
                    }
                }
            }

        }
    }
    public void ClearLevelsAndActions()
    {
        int i, j;
        for (i = 0; i < LevelDataClasses.MAXLEVELS; i++)
        {
            //for (j = 0; j < MAXLEVELS; j++) {
            gameData.levels[i] = new TargetActionLevel();
            gameData.levels[i].actions = new TargetActionData[LevelDataClasses.MAXACTIONS];
            gameData.levels[i].allowedTargetIndexes = new int[LevelDataClasses.MAXTARGET];
            //	gameData.levels [i]
            //}

            for (j = 0; j < LevelDataClasses.MAXACTIONS; j++)
            {
                gameData.levels[i].actions[j] = new TargetActionData();
                gameData.levels[i].actions[j].from = LevelDataClasses.Vector3ToPastilleVec3(Vector3.zero);
                gameData.levels[i].actions[j].to = LevelDataClasses.Vector3ToPastilleVec3(Vector3.zero);
                gameData.levels[i].actions[j].state = 0;
            }


            for (int k = 0; k < LevelDataClasses.MAXTARGET; k++)
            {
                gameData.levels[i].allowedTargetIndexes[k] = 0;
            }
        }
    }

     
    void UnpackMobileFileAndroid(string fileName)
    { 

    
        //copies and unpacks file from apk to persistentDataPath where it can be accessed
        string destinationPath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        string sourcePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

        //if DB does not exist in persistent data folder (folder "Documents" on iOS) or source DB is newer then copy it
        if (!System.IO.File.Exists(destinationPath) || (System.IO.File.GetLastWriteTimeUtc(sourcePath) > System.IO.File.GetLastWriteTimeUtc(destinationPath)))
        {
            if (sourcePath.Contains("://"))
            {// Android  
                WWW www = new WWW(sourcePath);
                while (!www.isDone) {; }                // Wait for download to complete - not pretty at all but easy hack for now 
                if (System.String.IsNullOrEmpty(www.error))
                {
                    System.IO.File.WriteAllBytes(destinationPath, www.bytes);
                }
                else
                {
                    Debug.Log("ERROR: the file DB named " + fileName + " doesn't exist in the StreamingAssets Folder, please copy it there.");
                }
            }
            else
            {                // Mac, Windows, Iphone                
                             //validate the existens of the DB in the original folder (folder "streamingAssets")
                if (System.IO.File.Exists(sourcePath))
                {
                    //copy file - alle systems except Android
                    System.IO.File.Copy(sourcePath, destinationPath, true);
                }
                else
                {
                    Debug.Log("ERROR: the file DB named " + fileName + " doesn't exist in the StreamingAssets Folder, please copy it there.");
                }
            }
        }
       

    }



    public TargetActionGameData Load()
    {
        if (loaded)
            return gameData;
        if (LevelDataClasses.FILE_PATH.Length <= 1)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (!File.Exists(Application.persistentDataPath + "/" + LevelDataClasses.saveName)) UnpackMobileFileAndroid(LevelDataClasses.saveName);
                LevelDataClasses.FILE_PATH = Application.persistentDataPath + "/" + LevelDataClasses.saveName;
            }
            else
            {
            LevelDataClasses.FILE_PATH = Application.streamingAssetsPath + "/"+ LevelDataClasses.saveName;
            }
        }
         
		// Initial values
		gameData = new TargetActionGameData ();
		gameData.version = LevelDataClasses.version;



		gameData.levels = new TargetActionLevel[LevelDataClasses.MAXLEVELS];



		if (File.Exists (LevelDataClasses.FILE_PATH)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (LevelDataClasses.FILE_PATH, FileMode.Open, FileAccess.Read);
			TargetActionGameData fileGameData = (TargetActionGameData)bf.Deserialize (file);

			file.Close ();
			//useful for edit commentout for now
			/*
			if (fileGameData.levels.Length < gameData.levels.Length) {
				CopyGameData (fileGameData);
			} else {
				gameData = fileGameData;
			}
			*/

			gameData = fileGameData;
			//UpdateLoad ();
			/*}

		bool wasChanged = UpdateAvatars (gameData.avatarData);
		if (wasChanged)
			Save ();

		bWasLoaded = true;
		*/
			loaded = true;
		} else {
			Debug.Log ("File does not exist" + LevelDataClasses.FILE_PATH);
			gameData = null;
			loaded = false;

		}
	
		return gameData;
	}
}
