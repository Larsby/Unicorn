using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;



public class TargetLevelManager : MonoBehaviour
{
    private const float version = 1.0f;
    private static TargetActionGameData gameData;
    public GameObject[] levelActions;
    public GameObject target;
    public bowAndArrow gameManager;
    [SerializeField]
    private int currentLevel = 0;

    [InspectorButton("NextLevel")]
    public bool Next;
    [InspectorButton("PreviousLevel")]
    public bool Previous;

    [InspectorButton("CommitLevel")]
    public bool Commit_;


    [InspectorButton("SaveLevels")]
    public bool Save_;

    [InspectorButton("LarsbySaveLevel")]
    public bool Larsby_;

    [InspectorButton("LoadLevel")]
    public bool Load_;

    [InspectorButton("LarsbySaveLevelREVERT")]
    public bool LarsbyREV_;
    [InspectorButton("NextCharacter")]
    public bool NextCharacter_;
    [InspectorButton("SetFirstAvailableChar")]
    public bool SetFirstAvailableChar_;
    public List<String> unlockedCharacters;
    //public HashSet<int> lastLevelForIndex = null;
    //
    Dictionary<int, int> lastLevelForIndex = null;


    private List<ITargetAction> actions;
    public GameObject loadManagerPrefab;
    private LevelDataLoader loadManager;
    private int previousLevel;
    public string status = "";
    private string PRODUCTION_PATH = LevelDataClasses.saveName;
    //	private string EDIT_PATH = "";
    private string EDIT_PATH = LevelDataClasses.saveName;


    [Range(0, 13)]
    public int gameObjectIndex;
    private int prevGameObjectIndex;
    private bool init = false;
    [SerializeField]
    private int[] targetIndexes;
    private bool loaded = false;
    IDictionary<int, int> currentLevelForTargetIndex =
        new Dictionary<int, int>();

    public int actualLevel = 0;
    private bool update = false;
    public Text actualLevelText;
    public static TargetLevelManager instance = null;
    public bool debug_showCurrentUnlock = false;
    private int calculatedDifficulty;

    private List<int> availableCharacters;
    private bool reloadGameObject = false;
    private void SetFirstAvailableChar()
    {
        bool iterate = true;
        int k = 0;
        for (int i = 0; i < targetIndexes.Length; i++)
        {
            if (iterate)
            {
                int foo = availableCharacters[i];
                if (targetIndexes[i] == 1)
                {
                    iterate = false;
                    gameObjectIndex = i;
                }
            }
        }
        reloadGameObject = true;

    }


    void setCurrentLevel(int inCurrentLevel)
    {
        //   Debug.Log(inCurrentLevel + " : " + currentLevel);
        currentLevel = inCurrentLevel;
    }
    void incCurrentLevel()
    {

        currentLevel++;
        //        Debug.Log(currentLevel);

    }




    private void NextCharacter()
    {
        bool iterate = true;
        int k = 0;
        for (int i = 0; i < availableCharacters.Count; i++)
        {
            if (iterate)
            {
                int foo = availableCharacters[i];
                if (foo == gameObjectIndex)
                {
                    availableCharacters[k] = -1;
                }
                else if (foo != -1)
                {
                    gameObjectIndex = foo;
                    iterate = false;
                }

                k++;
            }
        }
        SetLevel(currentLevel);
    }
    private void NextLevel()
    {
        SetLevel(++currentLevel);
    }

    private void PreviousLevel()
    {
        SetLevel(--currentLevel);
    }


    private void CommitLevel()
    {
        SaveDataForLevel();
        LoadDataForLevel();
        SetLevel(currentLevel);
        status = "Committed!";
    }
    private void SaveLevels()
    {
        SaveDataForLevel();
        Save();
        status = "Saved!";
    }

    private void LarsbySaveLevel()
    {
        SaveDataForLarsbylevel();
        //	Save ();
        LoadDataForLevel();
    }

    private void LarsbySaveLevelREVERT()
    {
        for (int i = 0; i < LevelDataClasses.MAXLEVELS; i++)
        {
            LoadDataForLevel();
            if (gameData.levels[i].actions[0].difficulty == 99)
            {
                gameData.levels[i].actions[0].difficulty = 98;
                SetLevel(i);

            }
            //	for (int i = 0; i < 10; i++) {
            Debug.Log("Twanging Level: " + i);
            //	SaveDataForLarsbylevel ();





        }
        Save();
        Debug.Log("Twanging!! ");
    }

    public void LoadLevel()
    {
        LoadDataForLevel();
        SetLevel(currentLevel);
        status = "Loaded";
    }

    void ResetActionTargets()
    {
        ITargetAction[] actions = gameObject.GetComponents<ITargetAction>();
        foreach (Behaviour act in actions)
        {
            act.enabled = false;

        }
        foreach (Behaviour act in actions)
        {
            act.enabled = true;

        }
    }
    public int GetCurrentDifficulty(GameObject ob)
    {
        ITargetInfoDifficulty[] actions = gameObject.GetComponents<ITargetInfoDifficulty>();
        foreach (ITargetInfoDifficulty act in actions)
        {
            return act.GetDifficulty();
        }
        return 1;
    }
    private void SetTargetForActions(GameObject target)
    {
        ITargetAction[] actions = gameObject.GetComponents<ITargetAction>();
        foreach (ITargetAction act in actions)
        {
            act.EditorSetup(gameObject, target);
        }
    }
    public void RebindTarget(GameObject target)
    {
        SetTargetForActions(target);
        ResetActionTargets();
    }
    public void SetTarget(GameObject t)
    {
    }

    public int GetCurrentLevel()
    {
        //LoadDataForLevel ();
        return currentLevel;
    }

    public void ResetLevel()
    {

        setCurrentLevel(0);
        ClearLevels();
    }
    public void SetLevel(int level)
    {
        setCurrentLevel(level);


    }
    /*
	public void IncreaseLevel ()
	{
	//	currentLevel++;
		actualLevel++;
	}
*/

    public void setScore(int inscore)
    {
        //Todo: change here so that the difficulty is increased
        actualLevel++;
        calculatedDifficulty = actualLevel;

    }

    public void PlayTargetAnim(GameObject target)
    {
        if (target == null)
            return;
        ITargetAction[] actions = target.GetComponents<ITargetAction>();
        foreach (ITargetAction act in actions)
        {
            act.Play();
        }
    }

    public void PauseTargetAnim(GameObject target)
    {
        if (target == null)
            return;
        ITargetAction[] actions = target.GetComponents<ITargetAction>();
        foreach (ITargetAction act in actions)
        {
            act.Pause();
        }
    }

    public void SetDifficultyOnTarget(GameObject target)
    {
        Init();
        int i = 0;
        status = " playing " + currentLevel;
        foreach (GameObject obj in levelActions)
        {
            ITargetAction action = obj.GetComponent<ITargetAction>();
            action.EditorSetup(null, target);
            action.Load(gameData.levels[currentLevel].actions[i]);
            i++;
        }
    }

    GameObject InstantiateTargetFromGameManager(int index)
    {

        GameObject obj = Instantiate(gameManager.CornTargets[index]);
        return obj;

    }

    void AddActionComponents()
    {

    }

    void SetupActions(GameObject target)
    {
        actions = new List<ITargetAction>();
        foreach (GameObject obj in levelActions)
        {
            ITargetAction action = obj.GetComponent<ITargetAction>();

            action = action.EditorSetup(gameObject, target);
            actions.Add(action);
        }
    }
    /*
	int FindFirstAvailableLevelForTarget(int level, int targetIndex) {
		int result = -1;
		if (currentLevelForTargetIndex.ContainsKey (targetIndex)) {
			level = currentLevelForTargetIndex [targetIndex];
			result = level;
		}
		for(int i = level;i<gameData.levels.Length;i++) {

				if (gameData.levels [i].allowedTargetIndexes [targetIndex] == 1) {
					currentLevelForTargetIndex [targetIndex] = i + 1;
					return i;
				}
			}
	
		return result;
	}
    */
    int FindFirstAvailableLevelForTargetWithDifficulty(int difficulty, int targetIndex, int startAt)
    {
        //
        //for (int i = LevelDataClasses.MAXLEVELS; i > 0; i--)
        for (int i = startAt; i < LevelDataClasses.MAXLEVELS - 1; i++)
        {
            //check if we have this animal on this level
            if (targetIndex < gameData.levels[i].allowedTargetIndexes.Length)
            {
                if (gameData.levels[i].allowedTargetIndexes[targetIndex] == 1)
                {
                    return i;
                }
                //check if the difficulty is a match for  this level.
                if (gameData.levels[i].actions[0].difficulty <= difficulty)
                {


                    return i;

                }
            }

        }


        Debug.Log("nope");
        return 0; //did not find
    }
    void Awake()
    {
        /*
		if (instance == null) {
			instance = this;
		}
		if (instance != this) {
			Destroy (this);
			return;
		}
		DontDestroyOnLoad (instance);
		*/

        loaded = false;
        previousLevel = -1;

        setCurrentLevel(0);

    }
    public bool LevelsLeft(int index)
    {
        return gameData.levels.Length > index;
    }
    public void RestartLevels()
    {

        lastLevelForIndex = new Dictionary<int, int>();
    }
    public int InitCurrentLevelWithTarget(GameObject t)
    {
        SetupActions(t);
        LoadDataForLevel(currentLevel);

        incCurrentLevel();

        return 1;
    }
    public int CurrentLevel()
    {
        return currentLevel;
    }

    public GameObject InitCurrentLevelWithRandomTarget(int levelNum, GameObject[] CornTargets, int MaxUnlocked)
    {




        target = null;
        //      print("levelNum: " + levelNum);
        /*  for (int i = 0; i < gameData.levels[levelNum].allowedTargetIndexes.Length; i++)
          {
              print(i + " :  " + gameData.levels[levelNum].allowedTargetIndexes[i]);


          }*/
        int tries = 0;
        while (target == null)
        {



            int RandomTarget = UnityEngine.Random.Range(0, MaxUnlocked);

            if (tries > 15)
            {
                target = Instantiate(CornTargets[0]);

                print("Tired of trying");
            }
            else
            {
                //first gate, is this enabled for this level?
                if (gameData.levels[levelNum].allowedTargetIndexes[RandomTarget] == 1)
                {

                    //second gate, check iof the corntarget is  enabled
                    if (MaxUnlocked > RandomTarget)
                    {
                        //  print("choose: " + RandomTarget);
                        target = Instantiate(CornTargets[RandomTarget]);
                    }

                }

            }
            tries++;
        }
        //   target = gameObject;
        setCurrentLevel(levelNum);
        //  Init ();
        SetupActions(target);
        LoadDataForLevel(levelNum);



        return target;
    }


    public int InitWithTarget(GameObject t, int index)
    {
        LevelDataClasses.FILE_PATH = Application.streamingAssetsPath + LevelDataClasses.saveName;
        target = t;
        //Load ();
        // try to find the first available level for the target
        //int theLevel = FindFirstAvailableLevelForTarget (actualLevel, index);
        int startLevel = 0;
        if (lastLevelForIndex == null)
        {
            lastLevelForIndex = new Dictionary<int, int>();
        }
        if (lastLevelForIndex.ContainsKey(index))
        {
            startLevel = lastLevelForIndex[index] + 1;

        }



        int theLevel = FindFirstAvailableLevelForTargetWithDifficulty(calculatedDifficulty, index, startLevel);
        lastLevelForIndex[index] = theLevel;
        if (theLevel > -1)
        {

            setCurrentLevel(theLevel);
            //  Init ();
            SetupActions(target);
            LoadDataForLevel(theLevel);
        }
        else
        {
            return -1;
        }
        //previousLevel = currentLevel;

        return 1;
    }
    public void ClearLevels()
    {
        currentLevelForTargetIndex.Clear();
    }
    void Init()
    {
        if (init)
            return;
        init = true;

        //currentLevel = 0;

        //EDIT_PATH = Application.persistentDataPath + saveName;

        prevGameObjectIndex = gameObjectIndex;
        if (gameManager.inEditMode)
        {
            LevelDataClasses.FILE_PATH = Application.streamingAssetsPath + LevelDataClasses.saveName;
            //previousLevel = currentLevel;
            //ITargetAction action = levelActions [0].GetComponent<ITargetAction> ();
            //action.SetupAction (target, LevelDifficulty.BEGINNER);
            target = InstantiateTargetFromGameManager(gameObjectIndex);
            Load();

            if (File.Exists(LevelDataClasses.FILE_PATH) == false)
            {
                Save();
            }

            SetupActions(target);
            LoadDataForLevel();
        }
        else
        {
            Load();


            //SetupActions (target);
            LoadDataForLevel();
            status = "Enable inEditMode in bow (gameManaager) to use the level editor";
        }

    }

    // Use this for initialization
    void Start()
    {
        availableCharacters = new List<int>();
        SetUpGameLoader();

        actualLevel = 0;

        init = false;
        Init();
    }

    public int GetTargetIndex(int max)
    {
        Init();
        int randomIndex = UnityEngine.Random.Range(0, max + 1);
        return randomIndex;
    }




    public int GetTargeIndextFromLevelRestrictions(int maxLevel, int theLevel = -1)
    {

        Init();

        if (theLevel == -1 || theLevel < currentLevel)
            setCurrentLevel(theLevel);
        if (theLevel > currentLevel)
        {
            setCurrentLevel(theLevel);
        }
        //int count = 0;
        List<int> availableTarget = new List<int>();
        if (gameData.levels[theLevel].allowedTargetIndexes == null)
        {
            Debug.Log("Bad stuff happened here at index " + currentLevel);
            gameData.levels[theLevel].allowedTargetIndexes = new int[1];
            gameData.levels[theLevel].allowedTargetIndexes[0] = 1;
        }
        unlockedCharacters.Clear();
        for (int i = 0; i < gameData.levels[theLevel].allowedTargetIndexes.Length; i++)
        {

            if (gameData.levels[theLevel].allowedTargetIndexes[i] > 0 && i <= maxLevel)
            {
                availableTarget.Add(i);
                unlockedCharacters.Add("" + i);
                availableCharacters.Add(i);
            }
        }
        int range = availableTarget.Count;
        if (range == 0)
        {


            for (int i = 0; i < gameData.levels[theLevel].allowedTargetIndexes.Length; i++)
            {
                if (gameData.levels[theLevel].allowedTargetIndexes[i] > 0)
                {
                    // return the levels first available target. The target has not been unlocked yet but choose a target that the level support. The first available (easiest) one
                    return i;
                }
                return 0;
            }
        }
        else
        {
            range = availableTarget.Count - 1;
        }

        int randomIndex = UnityEngine.Random.Range(0, range);
        //Debug.Log ("GetTargeIndextFromLevelRestrictions" + randomIndex);

        int val = availableTarget[randomIndex];

        return val;

    }


    void LoadDataForLevel()
    {
        LoadDataForLevel(currentLevel);
    }

    void LoadDataForLevel(int level)
    {
        if (actions != null)
        {
            for (int i = 0; i < gameData.levels[level].actions.Length; i++)
            {
                if (i < actions.Count)
                {
                    actions[i].Load(gameData.levels[level].actions[i]);
                }

            }
        }
        for (int i = 0; i < targetIndexes.Length; i++)
        {
            if (gameData.levels[level].allowedTargetIndexes == null)
            {
                //Debug.Log ("Bad stuff happened here at index " + i + " fixing");
                gameData.levels[level].allowedTargetIndexes = new int[10];
                gameData.levels[level].allowedTargetIndexes[0] = 1;
                gameData.levels[level].allowedTargetIndexes[1] = 1;
                gameData.levels[level].allowedTargetIndexes[2] = 0;
                gameData.levels[level].allowedTargetIndexes[3] = 0;
                gameData.levels[level].allowedTargetIndexes[4] = 0;
                gameData.levels[level].allowedTargetIndexes[5] = 0;
                gameData.levels[level].allowedTargetIndexes[6] = 0;
                gameData.levels[level].allowedTargetIndexes[7] = 0;
                gameData.levels[level].allowedTargetIndexes[8] = 0;
                gameData.levels[level].allowedTargetIndexes[9] = 0;
            }
            if (i < gameData.levels[level].allowedTargetIndexes.Length)
            {
                targetIndexes[i] = gameData.levels[level].allowedTargetIndexes[i];
                if (availableCharacters == null)
                {
                    availableCharacters = new List<int>();
                }
                if (targetIndexes[i] == 1)
                {
                    if (availableCharacters.Contains(i) == false)
                    {

                        availableCharacters.Add(i);
                    }
                }
            }


        }
    }

    void SaveDataForLevel()
    {
        SaveDataForLevel(currentLevel);
    }
    void SaveDataForLarsbylevel()
    {
        int level = currentLevel;
        for (int i = 0; i < actions.Count; i++)
        {
            TargetActionData input = actions[i].Save();
            gameData.levels[level].actions[i] = input;

            gameData.levels[level + 1].actions[i] = input;
            //		
        }
        gameData.levels[level + 1].allowedTargetIndexes = new int[targetIndexes.Length];
        for (int i = 0; i < targetIndexes.Length; i++)
        {

            gameData.levels[level + 1].allowedTargetIndexes[i] = targetIndexes[i];


        }
        incCurrentLevel();
        actualLevel++;

    }


    void SaveDataForLevel(int level)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            TargetActionData input = actions[i].Save();
            gameData.levels[level].actions[i] = input;
        }
        gameData.levels[level].allowedTargetIndexes = new int[targetIndexes.Length];
        for (int i = 0; i < targetIndexes.Length; i++)
        {
            gameData.levels[level].allowedTargetIndexes[i] = targetIndexes[i];
        }
    }

    public void StartTutorial()
    {
        //LoadDataForLevel ();
    }
    private int GetAvailIndexForLevel()
    {
        foreach (int i in availableCharacters)
        {
            if (i != -1)
            {
                return i;
            }
        }
        return -1;
    }

    // Update is called once per frame
    void Update()
    {

        if (actualLevelText != null)
        {
            actualLevelText.text = "Level " + actualLevel + "calculatedDifficulty: " + calculatedDifficulty;
        }
        if (init == false)
            return;


        if (previousLevel != currentLevel)
        {

            if (gameManager.inEditMode)
            {
                if (previousLevel != -1)
                {
                    SaveDataForLevel(previousLevel);

                }

                previousLevel = currentLevel;

            }
            if (gameManager.inEditMode == false)
            {
                previousLevel = currentLevel;
            }
            previousLevel = currentLevel;
            LoadDataForLevel();

        }
        if (gameObjectIndex != prevGameObjectIndex || reloadGameObject)
        {
            prevGameObjectIndex = gameObjectIndex;

            Destroy(target);
            //här skall vi kolla vilka targets som är accepterade för nivån, detta känns fel
            int index = gameObjectIndex;
            if (gameManager.inEditMode == true && reloadGameObject == false)
            {
                index = GetAvailIndexForLevel();
            }
            reloadGameObject = false;
            target = InstantiateTargetFromGameManager(index);
            SetTargetForActions(target);
            ResetActionTargets();
            //LoadDataForLevel ();

        }

    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(LevelDataClasses.FILE_PATH, FileMode.OpenOrCreate);

        bf.Serialize(file, gameData);
        file.Close();
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
    public void SetUpGameLoader()
    {
        if (loadManager == null)
        {
            GameObject loadManagerObject = GameObject.FindGameObjectWithTag("LevelData");
            if (loadManagerObject == null)
            {
                loadManagerObject = Instantiate(loadManagerPrefab);
                loadManagerObject.transform.parent = null;
            }
            loadManager = loadManagerObject.GetComponent<LevelDataLoader>();
        }
    }
    public void Load(bool erase = false)
    {
        SetUpGameLoader();
        //	if (gameData == null) {
        gameData = loadManager.Load();
        //	}
    }
}


