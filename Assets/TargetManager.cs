using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    int gameRound = 0;
    int unlockIndex = 0;
    int currentTargetType = 0;
    int m_score = 0;

    int prevUnlockindex;
    public int debugUnlockIndex = -1;
    public ThrowerManager manager;
    public bowAndArrow gameManager;
    RandomNonRepeating random;
    public int LEVEL3 = 4000;
    public int LEVEL4 = 5000;
    public int LEVEL5 = 6300;
    public int LEVEL6 = 8000;
    public int LEVEL7 = 11000;
    public int LEVEL8 = 15000;
    public int LEVEL9 = 17000;
    public int LEVEL10 = 2200;
    public int LEVEL11 = 27000;
    public int LEVEL12 = 32000;

    public bool useNewLevelUpSystem = false;
    public int ScoreIncrement = 100;
    private int currentScoreThreshold = 0;


    public RectTransform animalProgressBar;
    public bool useProgressBarAndSavedProgress = true;
    public int animalProgressIncrement = 2;
    public int initialTreshold = 2;
    int animalIncrementSum = 0;
    int currentNofThrows = 0;
    int levelTreshold = 0;

    private bool newLevel = false;
    public class UnlockResult
    {
        public int unlock;
        public bool newUnlock;

        public UnlockResult(int unlock, bool newUnlock)
        {
            this.unlock = unlock;
            this.newUnlock = newUnlock;
        }
    }

    void Start()
    {
        gameRound = 0;
        unlockIndex = 0;
        random = new RandomNonRepeating();
        LoadValues();
        newLevel = false;
        currentScoreThreshold = ScoreIncrement;

        if (!useProgressBarAndSavedProgress && animalProgressBar)
            animalProgressBar.transform.parent.gameObject.SetActive(false);
    }

    public void Restart()
    {
        currentScoreThreshold = ScoreIncrement;
    }
    private void ResetUnlockIndex_Debug()
    {
        unlockIndex = 0;
        //PlayerPrefs.SetInt ("unlockIndex", unlockIndex);
        PlayerPrefs.SetInt("gameRound", gameRound);
        PlayerPrefs.Save();
        newLevel = false;
    }
    private void LoadValues()
    {

        if (PlayerPrefs.HasKey("gameRound"))
        {
            gameRound = PlayerPrefs.GetInt("gameRound");
        }
        else
        {
            gameRound = 0;
            PlayerPrefs.SetInt("gameRound", gameRound);
            PlayerPrefs.Save();
        }
        /*
		if (PlayerPrefs.HasKey ("unlockIndex")) {
			unlockIndex = PlayerPrefs.GetInt ("unlockIndex");
		} else {
			unlockIndex = 0;
			PlayerPrefs.SetInt ("unlockIndex", unlockIndex);
			PlayerPrefs.Save ();
		}
		*/

        if (useProgressBarAndSavedProgress)
        {
            if (PlayerPrefs.HasKey("CurrentNofThrows"))
            {
                currentNofThrows = PlayerPrefs.GetInt("CurrentNofThrows");
                animalIncrementSum = PlayerPrefs.GetInt("AnimalIncrementSum");
                levelTreshold = PlayerPrefs.GetInt("LevelTreshold");
            }
            else
            {
                currentNofThrows = 0;
                levelTreshold = initialTreshold;
                animalIncrementSum = animalProgressIncrement;
                PlayerPrefs.SetInt("CurrentNofThrows", currentNofThrows);
                PlayerPrefs.SetInt("AnimalIncrementSum", animalIncrementSum);
                PlayerPrefs.SetInt("LevelTreshold", levelTreshold);
                PlayerPrefs.Save();
            }
        }

    }
    private void SetPrefs()
    {

        PlayerPrefs.SetInt("gameRound", gameRound);
        PlayerPrefs.SetInt("unlockIndex", unlockIndex);

        if (useProgressBarAndSavedProgress)
        {
            PlayerPrefs.SetInt("CurrentNofThrows", currentNofThrows);
            PlayerPrefs.SetInt("AnimalIncrementSum", animalIncrementSum);
            PlayerPrefs.SetInt("LevelTreshold", levelTreshold);
        }
        PlayerPrefs.Save();
    }
    public void NewRound()
    {
        gameRound++;
        SetPrefs();
    }

    public bool AllowDance()
    {
        if (m_score > 500 || gameRound > 20)
            return true;
        return false;
    }
    public bool IsAvatarBought(int index)
    {


        if (manager != null)
        {
            return manager.IsAvatarUnlocked(index);
        }
        return false;
    }
    public bool IsAvatarUnlocked(int index)
    {
        LoadValues();
        unlockIndex = GetLatestUnlockLevelIndex();
        Debug.Log("Latest unlock is " + GetTargetName(unlockIndex));
        if (index <= unlockIndex)
            return true;
        return false;
    }

    private int GetUniqueTarget(int unlockidx)
    {
        bool loop = true;
        int targettype = -1;
        while (loop)
        {
            random.SetRange(0, unlockidx + 1);
            targettype = random.GetRandom();
            if (targettype != currentTargetType)
                loop = false;
            currentTargetType = targettype;

        }
        return targettype;
    }

    public int GetDifficultLevel(int index)
    {
        if (PlayerPrefs.HasKey("CharacterDifficulty" + index) == false)
        {
            SaveDifficultLevel(index, 0);
        }
        else
        {
            return PlayerPrefs.GetInt("CharacterDifficulty" + index);
        }
        return 0;

    }
    private void SaveDifficultLevel(int index, int level)
    {

        PlayerPrefs.SetInt("CharacterDifficulty" + index, level);
        PlayerPrefs.Save();
    }

    public int GetCurrentHardLevel(int score, int multiplier = 1)
    {
        return CalculateDifficultyLevel(score, multiplier);
    }
    public int GetLatestUnlockLevelIndex()
    {
        if (PlayerPrefs.HasKey("LatestUnlockLevel") == false)
        {
            SaveLatestUnlockLevel(0);
        }
        else
        {
            return PlayerPrefs.GetInt("LatestUnlockLevel");
        }
        return 0;
    }
    void SaveLatestUnlockLevel(int unlockLevel)
    {
        PlayerPrefs.SetInt("LatestUnlockLevel", unlockLevel);
        PlayerPrefs.Save();
    }
    float map(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }
    public int CalculateDifficultyLevel(int score, int multiplier)
    {
        int toreturn = (int)map(score, 0.0f, 10000.0f, 0.0f, 46.0f);

        if (toreturn > 46)
            toreturn = Random.Range(0, 46);
        //        print("CalculateDifficultyLevel: " + toreturn);

        return toreturn;
    }



    void UpdateProgressBarTween(float newValue)
    {
        animalProgressBar.sizeDelta = new Vector2(newValue, animalProgressBar.sizeDelta.y);
    }
    void CompleteProgressBarTween()
    {
        iTween.ValueTo(gameObject, iTween.Hash("from", animalProgressBar.sizeDelta.x, "to", 0, "time", 0.5f, "onupdatetarget", gameObject, "onupdate", "UpdateProgressBarTween", "easetype", iTween.EaseType.easeOutQuad));
        //animalProgressBar.sizeDelta = new Vector2(0, animalProgressBar.sizeDelta.y);
    }

    private UnlockResult GetUnlockIndex(int score, int characterIndex)
    {
        bool newUnlock = false;
        int unlockIndx = GetLatestUnlockLevelIndex();
        int theScore = score;
        int result = 3;

        if (useProgressBarAndSavedProgress)
        {
            UnlockResult res = new UnlockResult(0, false);

            if (score > 0)
                currentNofThrows++;
            PlayerPrefs.SetInt("CurrentNofThrows", currentNofThrows);

            if (currentNofThrows >= levelTreshold)
            {
                levelTreshold += animalIncrementSum;
                animalIncrementSum += animalProgressIncrement;
                currentNofThrows = 0;
                SetPrefs();

                //if (unlockIndx <= 3) // why??
                //	unlockIndx = 3;
                unlockIndx++;
                result = unlockIndx;

                gameManager.NewUnlock(result);
                SaveDifficultLevel(characterIndex, result);
                SaveLatestUnlockLevel(result);
                res = new UnlockResult(result, true);

            }

            float delta = Mathf.Clamp01((float)currentNofThrows / (float)levelTreshold);
            RectTransform parentRect = animalProgressBar.transform.parent.GetComponent<RectTransform>();

            if (score > 0)
            {
                if (currentNofThrows > 0)
                    iTween.ValueTo(gameObject, iTween.Hash("from", animalProgressBar.sizeDelta.x, "to", parentRect.sizeDelta.x * delta, "time", 1f, "onupdatetarget", gameObject, "onupdate", "UpdateProgressBarTween", "easetype", iTween.EaseType.easeOutQuad));
                else
                    iTween.ValueTo(gameObject, iTween.Hash("from", animalProgressBar.sizeDelta.x, "to", parentRect.sizeDelta.x, "time", 1f, "onupdatetarget", gameObject, "onupdate", "UpdateProgressBarTween", "oncomplete", "CompleteProgressBarTween", "easetype", iTween.EaseType.easeOutQuad));
            }
            else
                animalProgressBar.sizeDelta = new Vector2(parentRect.sizeDelta.x * delta, animalProgressBar.sizeDelta.y);

            return res;
        }


        if (useNewLevelUpSystem)
        {
            UnlockResult res = new UnlockResult(0, false);
            if (theScore >= currentScoreThreshold)
            {

                currentScoreThreshold = theScore + ScoreIncrement;
                if (unlockIndx <= 3)
                    unlockIndx = 3;
                unlockIndx++;
                result = unlockIndx;

                gameManager.NewUnlock(result);
                SaveDifficultLevel(characterIndex, result);
                SaveLatestUnlockLevel(result);
                res = new UnlockResult(result, true);

            }
            return res;
        }

        /*
		if (score > 800 && unlockIndx == 0) {
			result = 1;
			newUnlock = true;
		}
		if ( score > 3600 && unlockIndx == 1) {
			result =  2;
			newUnlock = true;
		}

		if (score > 5600 && unlockIndx == 2) {
			result =  3;
			newUnlock = true;
		}
		*/
        if (score > LEVEL3 && unlockIndx == 3)
        {
            result = 4;
            newUnlock = true;
        }

        if (score > LEVEL4 && unlockIndx == 4)
        {
            result = 5;
            newUnlock = true;
        }
        if (score > LEVEL5 && unlockIndx == 5)
        {
            result = 6;
            newUnlock = true;
        }
        if (score > LEVEL6 && unlockIndx == 6)
        {
            result = 7;
            newUnlock = true;
        }
        if (score > LEVEL7 && unlockIndx == 7)
        {
            result = 8;
            newUnlock = true;
        }
        if (score > LEVEL8 && unlockIndx == 8)
        {
            result = 9;
            newUnlock = true;
        }
        if (score > LEVEL9 && unlockIndx == 9)
        {
            result = 10;
            newUnlock = true;
        }
        if (score > LEVEL10 && unlockIndx == 10)
        {
            result = 11;
            newUnlock = true;
        }
        if (score > LEVEL11 && unlockIndx == 11)
        {
            result = 12;
            newUnlock = true;
        }
        if (score > LEVEL12 && unlockIndx == 12)
        {
            result = 13;
            newUnlock = true;
        }
        if (debugUnlockIndex != -1)
        {
            newUnlock = true;
            result = debugUnlockIndex;
        }
        else
        {
            if (unlockIndx > result)
            {
                result = unlockIndx;
            }
        }
        if (newUnlock)
        {
            gameManager.NewUnlock(result);
            SaveDifficultLevel(characterIndex, result);
            SaveLatestUnlockLevel(result);

        }

        return new UnlockResult(result, newUnlock);
    }
    // debug
    private string GetTargetName(int index)
    {
        switch (index)
        {
            case 0:
                return "Horse";
            case 1:
                return "target_bird";
            case 2:
                return "target Pig";
            case 3:
                return "SnailTarget";
            case 4:
                return "TargetGiraffe";
            case 5:
                return "target lama";
            case 6:
                return "TargetChicken";
            case 7:
                return "target Dino";
            case 8:
                return "flyingPig";
            case 9:
                return "flyingDolphin";
            case 10:
                return "pighen";
            case 11:
                return "pighenMinHen";
            case 12:
                return "Golden_horse";
            case 13:
                return "Unicorn";

        }
        return "";
    }


    //debug end
    public UnlockResult GetTargetType(int score, int maxUnlock)
    {
        m_score = score;
        int latestUnlockIndex = GetLatestUnlockLevelIndex();
        UnlockResult res;
        if (maxUnlock == latestUnlockIndex)
        {
            if (animalProgressBar)
                animalProgressBar.transform.parent.gameObject.SetActive(false);

            random.SetRange(0, maxUnlock + 1);
            // all characters is unlocked.
            // replace this so we don't repeat same char
            res = new UnlockResult(random.GetRandom(), false);
            return res;
        }
        else
        {
            res = GetUnlockIndex(score, (int)latestUnlockIndex);

            if (maxUnlock == res.unlock && animalProgressBar)
                animalProgressBar.transform.parent.gameObject.SetActive(false);

            if (res.unlock > unlockIndex)
            {
                unlockIndex = res.unlock;
                SetPrefs();
            }
            if (res.newUnlock)
            {
                return res;
            }
            if (latestUnlockIndex > 0)
            {
                res.unlock = GetUniqueTarget(latestUnlockIndex);
                res.newUnlock = false;
                return res;
            }
            res.unlock = 0;
            res.newUnlock = false;
            return res;
        }
    }

    // Update is called once per frame
    void Update()
    {


    }
}
