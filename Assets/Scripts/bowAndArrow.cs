

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


// this is the master game script, attached to th ebow

public class bowAndArrow : MonoBehaviour
{

    public bool inEditMode = false;
    public bool isHoming = true;

    // to determine the mouse position, we need a raycast
    private Ray mouseRay1;
    private RaycastHit rayHit;
    // position of the raycast on the screen
    private float posX;
    private float posY;

    // References to the gameobjects / prefabs
    //public GameObject bowString;
    GameObject arrow;
    public GameObject arrowPrefab;
    public GameObject gameManager;
    public GameObject[] CornTargets;
    public ParticleSystem popParticles;
    public ParticleSystem hitParticles;
    public GameObject HitScoreText;
    public GameObject RewardScoreText;
    public GameObject coinSound;
    public GameObject CornTarget;
    public TargetManager targetManager;
    public ThrowerManager throwerManager;
    // Sound effects
    public SoundFXManager mySoundFXManager;

    // has sound already be played

    int NUKE_VERSION = 16;
    public GameObject throwerParent;
    public GameObject happyMouth;
    public GameObject sadMouth;
    public GameObject legs;
    public GameObject body;
    public GameObject GameOverPanel;
    public GameObject unlockObject;
    public GameObject unlockPrefabParent;
    public bool useNewLevelSystem = false;
    public TargetLevelManager levelManager;
    // to determine the string pullout
    float arrowStartX;
    float length;
    public bool skip = false;
    public float MaxSpeed = 0.3f;

    // some status vars
    bool arrowShot;
    bool arrowPrepared;

    // position of the line renderers middle part
    int inARow = 1;
    int currentTargetType = -1;

    int theCurrentTargetDifficulty = -1;
    // game states
    public enum GameStates
    {
        menu,
        instructions,
        game,
        over,
        hiscore,
        sendhighscore,
        idle,
        unlock
    }

    ;

    public GameObject HighScorePanel;
    // store the actual game state
    public GameStates gameState = GameStates.menu;
    private GameStates oldState = GameStates.menu;
    // references to main objects for the UI screens
    //public Canvas menuCanvas;
    //public Canvas instructionsCanvas;
    //public Canvas highscoreCanvas;
    public Canvas gameCanvas;
    //public Canvas gameOverCanvas;

    // referene to the text fields of game UI
    //public Text arrowText;
    public GameObject[] arrowsDisplay;
    public Text scoreText;
    public Text endscoreText;
    public Text actualHighscoreText;
    public Text newHighscoreText;
    public Text newHighText;

    // amount of arrows for the game
    public int arrows = 3;
    // actual score
    public int score = 0;
    public Material rainBowMaterial;
    public int debugUseTargetIndex = -1;
    float bowMoveStartTime;
    private bool isInUnlockMode = false;
    // have we unlocked all targets? We do the difficulty settings differently. true if there is still targets to unlock
    private int resetableScore;
    public GameObject TutorialPrefab;
    private GameObject tutorialObj;

    public GameObject background;
    public GameObject targetContainer;

    public GameObject mainModeContainer;
    public GameObject iceCreamModeContainer;
    public GameObject iceCreamArrowPrefab;

    public static bool useIcecreamMode = false;

    public Text creditsText;
    public GameObject creditsPanel;
    public GameObject animalProgress;

    private bool fadeOldTarget = true;

    //
    // void resetGame()
    //
    // this method resets the game status
    //

    void SetUnlockMode()
    {
        int lastUnlockIndex = CornTargets.Length;
        isInUnlockMode = !targetManager.IsAvatarUnlocked(lastUnlockIndex);
    }

    void resetGame()
    {
        GameOverPanel.SetActive(false);
        levelManager.ResetLevel();
        endscoreText.text = "";
        arrows = 3;
        score = 0;
        resetableScore = 0;
        newHighscoreText.text = "";
        newHighscoreText.gameObject.SetActive(false);
        SetUnlockMode();
        //targetManager.NewRound ();
        // be sure that there is only one arrow in the game
        if (GameObject.Find("arrow") == null)
            createArrow(true);
        creditsPanel.SetActive(true);
        animalProgress.SetActive(true);
        fadeOldTarget = true;

        showScore();
    }


    void SetMaterialRecursive(Material m, GameObject obj)
    {
        if (obj == null)
            return;
        foreach (Transform t in obj.transform)
        {
            SetMaterialRecursive(m, t.gameObject);
        }
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            if (obj.CompareTag("Eye") == false)
            {
                renderer.material = m;
            }
        }
    }

    void SetColorRecursive(Color c, GameObject obj)
    {
        foreach (Transform t in obj.transform)
        {
            SetColorRecursive(c, t.gameObject);
        }
        SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            if (obj.CompareTag("Eye") == false)
            {
                renderer.color = c;
            }
        }
    }

    private void Skip()
    {
        destroyCornTarget();
        score += 200;
        showScore();

    }

    void SetArmSpriteFromPrefab()
    {

        SpriteRenderer prefabArmRenderer = throwerParent.transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
        Sprite arm = prefabArmRenderer.sprite;
        //	GetComponent<SpriteRenderer> ().sprite = arm;
        GetComponent<SpriteRenderer>().color = prefabArmRenderer.color;

    }

    GameObject GetChildWithTag(GameObject ob, string tag)
    {
        if (ob.CompareTag(tag) == true)
        {
            return ob;
        }
        foreach (Transform t in ob.transform)
        {

            GameObject taggedObject = GetChildWithTag(t.gameObject, tag);
            if (taggedObject != null)
            {
                return taggedObject;
            }

        }
        return null;

    }

    void SetSadAndHappyMouth()
    {
        GameObject o = GetChildWithTag(body, "SadMouth");
        if (o == null)
        {
            Debug.Log("thrower object does not have a sadmouth object tagged!!");
        }
        else
        {
            sadMouth = o;
        }
        o = GetChildWithTag(body, "HappyMouth");
        if (o == null)
        {
            Debug.Log("thrower object does not have a happyMouth object tagged!!");
        }
        else
        {
            happyMouth = o;
        }

    }

    void SetThrowerDude()
    {
        GameObject prefab = throwerManager.GetSelected();

        throwerParent = Instantiate(prefab, null);
        body = throwerParent.transform.GetChild(0).gameObject;
        legs = throwerParent.transform.GetChild(2).gameObject;
    }

    void NukeSettingsIfBelowVersion()
    {
        bool nuke = false;
        if (PlayerPrefs.HasKey("Version") == false)
        {
            nuke = true;
        }
        else
        {
            if (PlayerPrefs.GetInt("Version") < NUKE_VERSION)
            {
                nuke = true;
            }
        }
        if (nuke == true)
        {
            PlayerPrefs.DeleteAll();
        }
        PlayerPrefs.SetInt("Version", NUKE_VERSION);
        PlayerPrefs.Save();
    }

    // Use this for initialization
    void Start()
    {
        NukeSettingsIfBelowVersion();
        currentTargetType = -1;

        if (InTutorialMode())
        {
            CreateTutorialMode();
        }
#if UNITY_TVOS
		UnityEngine.Apple.TV.Remote.touchesEnabled = true;
		//	UnityEngine.Apple.TV.Remote.reportAbsoluteDpadValues = true;
		UnityEngine.Apple.TV.Remote.allowExitToHome = false;
#endif
        // set the UI screens

        //gameCanvas.enabled = true;

        levelManager.Load();
        // create the PlayerPref
        initScore();
        SetUnlockMode();
        // create an arrow to shoot
        // use true to set the target
        createArrow(false);


        arrowStartX = 0.0f;
        SetThrowerDude();

        creditsText.text = "" + StaticManager.GetNumberOfCredits();

        startGame();
        GenerateTick.OnTick += Dance;
        SetArmSpriteFromPrefab();
        SetSadAndHappyMouth();
        sadMouth.SetActive(false);
        //NewUnlock (1);
    }

    void Dance()
    {

        if (legs.transform.localEulerAngles.z == 0)
        {
            LeanTween.rotateZ(legs, Random.Range(5.0f, 10.0f), 0.2f).setEaseInOutElastic();
            LeanTween.rotateZ(happyMouth, Random.Range(5.0f, 10.0f), 0.2f).setEaseInOutElastic();
            LeanTween.rotateZ(body, Random.Range(5.0f, 10.0f), 0.2f).setEaseInOutElastic();
            LeanTween.moveLocalY(body, Random.Range(-0.4577361f, -0.4777361f), 0.2f).setEaseInOutElastic();




        }
        else
        {
            LeanTween.rotateZ(legs, 0, 0.2f).setEaseInOutCubic();
            LeanTween.rotateZ(happyMouth, 0, 0.2f).setEaseInOutCubic();
            LeanTween.rotateZ(body, 0, 0.2f).setEaseInOutCubic();
            LeanTween.moveLocalY(body, -0.3777361f, 0.2f).setEaseInOutCubic();


        }

    }

    void OnDestroy()
    {
        GenerateTick.OnTick -= Dance;

    }

    bool InputDown()
    {
#if UNITY_TVOS

		return Input.GetKey( KeyCode.JoystickButton14);	

#endif
        return Input.GetMouseButton(0);
    }

    bool InputUp()
    {
#if UNITY_TVOS
		return Input.GetKey( KeyCode.JoystickButton14)==true?false:true;	

#endif
        return Input.GetMouseButtonUp(0);
    }

    // Update is called once per frame
    void Update()
    {
        CornTarget.transform.position = Vector3.zero;
        if (skip)
        {
            Skip();
            skip = false;
        }
        // check the game states
        switch (gameState)
        {
            case GameStates.menu:
                // leave the game when back key is pressed (android)
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Application.Quit();
                }
                break;

            case GameStates.game:
                // set UI related stuff
                showArrows();
                //	showScore ();

                // return to main menu when back key is pressed (android)
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    showMenu();
                }

                // game is steered via mouse
                // (also works with touch on android)
                if (InputDown() && arrowShot == false)
                {
                    // the player pulls the string

                    // detrmine the pullout and set up the arrow
                    //prepareArrow ();
                    prepareArrow2();


                }

                // ok, player released the mouse
                // (player released the touch on android)
                if (InputUp() && arrowPrepared && arrowShot == false)
                {
                    // play string sound

                    mySoundFXManager.playEffect("swoosh");
                    // play arrow sound

                    // shot the arrow (rigid body physics)
                    shootArrow();

                }

                // in any case: update the bowstring line renderer

                break;
            case GameStates.instructions:
                break;
            case GameStates.over:
                {


                    GameOverPanel.SetActive(true);
                    gameState = GameStates.idle;
                    break;
                }
            case GameStates.hiscore:
                break;
        }
    }

    public void Menu()
    {
        SceneManager.LoadScene("Main");
    }

    public void PlayGame()
    {
        gameState = GameStates.game;
        fadeOldTarget = false;
        resetGame();

    }

    //
    // public void initScore()
    //
    // The player score is stored via Playerprefs
    // to make sure they can be stored,
    // they have to be initialized at first
    //

    public void initScore()
    {
        if (!PlayerPrefs.HasKey("Score"))
            PlayerPrefs.SetInt("Score", 0);

    }

    IEnumerator IncreaseScore()
    {
        yield return new WaitForSeconds(1.0f);
        showScore();
    }

    public void showScore()
    {
        scoreText.text = "" + score.ToString();
    }


    public void showArrows()
    {
        //arrowText.text = "Arrows: " + (arrows + 0);

        for (int i = 0; i < arrowsDisplay.Length; i++)
        {
            if (i < arrows)
            {
                arrowsDisplay[i].SetActive(true);
            }
            else
                arrowsDisplay[i].SetActive(false);
        }



    }
    private void CreateTutorialMode()
    {
        PlayerPrefs.SetInt("TutorialMode", 0);
        PlayerPrefs.Save();
        if (tutorialObj != null)
        {
            Destroy(tutorialObj);
        }
        tutorialObj = Instantiate(TutorialPrefab);
        tutorialObj.SetActive(true);
    }
    public bool InTutorialMode()
    {
        //	return false;
        if (inEditMode) return false;
        if (PlayerPrefs.HasKey("TutorialMode") == false)
        {
            CreateTutorialMode();
            return true;
        }
        int times = PlayerPrefs.GetInt("TutorialMode");
        if (times >= 2)
            return false;
        return true;
    }
    private void IncreaseTutorialMode()
    {
        int times = PlayerPrefs.GetInt("TutorialMode");
        times = times + 1;
        PlayerPrefs.SetInt("TutorialMode", times);
        PlayerPrefs.Save();

        if (times >= 2)
        {
            if (tutorialObj != null)
            {
                Destroy(tutorialObj);
            }
        }
    }


    public static void Rate()
    {

        UniRate r = GameObject.FindObjectOfType<UniRate>();
        if (r.ShouldPromptForRating())
        {
            r.PromptIfNetworkAvailable();
        }
    }

    public void startThePopCorn()
    {
        hitParticles.transform.position = new Vector3(arrow.transform.position.x, arrow.transform.position.y, hitParticles.transform.position.z);
        hitParticles.Play();
        popParticles.Play();
        mySoundFXManager.playEffect("popcorns");
    }

    public void setMiss()
    {
        inARow = 0;
    }

    public void setMissed()
    {
        sadMouth.SetActive(true);
        happyMouth.SetActive(false);
        if (useNewLevelSystem == false)
        {
            CornTarget.GetComponent<MakingItHarder>().stopDancing();
        }
        else
        {
            levelManager.PauseTargetAnim(CornTarget);
        }
        /*
        if (InTutorialMode())
        {
            CreateTutorialMode();
            resetGame();

        }*/
    }
    //
    // public void createArrow()
    //
    // this method creates a new arrow based on the prefab
    //



    public void newArrow()
    {

        // now instantiate a new arrow
        this.transform.localRotation = Quaternion.identity;

        sadMouth.SetActive(false);
        happyMouth.SetActive(true);

        arrow = Instantiate(arrowPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        arrow.name = "arrow";
        arrow.transform.localScale = this.transform.localScale + new Vector3(0.5f, 0.5f, 0);
        arrow.transform.localPosition = this.transform.position + new Vector3(0.05f, 0.95f, 0);
        arrow.transform.localRotation = this.transform.localRotation;
        arrow.transform.parent = this.transform;
        // transmit a reference to the arrow script
        arrow.GetComponent<rotateArrow>().setBow(gameObject);
        arrow.GetComponent<rotateArrow>().setSoundFXManager(mySoundFXManager);

        arrowShot = false;
        arrowPrepared = false;
        transform.eulerAngles = new Vector3(0, 0, -45);
        MakingItHarder harder = CornTarget.GetComponent<MakingItHarder>();
        if (harder == null && useNewLevelSystem == false)
        {
            harder = CornTarget.AddComponent<MakingItHarder>();
        }
        if (targetManager.AllowDance())
        {
            if (useNewLevelSystem == false)
            {
                harder.enabled = true;
                harder.GetComponent<MakingItHarder>().startDancing();
            }
            else
            {
                levelManager.PlayTargetAnim(CornTarget);
            }
        }

    }
    /*
	void CornTargetFade ()
	{
		for (float f = 1f; f >= 0; f -= 0.1f) {
			Color c = CornTarget.GetComponent<SpriteRenderer> ().color;

			c.a = f;
			CornTarget.GetComponent<SpriteRenderer> ().color = c;
		}
	}*/

    IEnumerator CornTargetFadeIn(GameObject target)
    {

        for (float f = 0f; f <= 1.1; f += 0.1f)
        {
            if (target.GetComponent<SpriteRenderer>() == null)
            {
                //	yield return new WaitForSeconds (0.05f);
                SpriteRenderer[] sprites = target.GetComponentsInChildren<SpriteRenderer>();

                for (int i = 0; i < sprites.Length; i++)
                {

                    Color c = sprites[i].color;
                    c.a = f;

                    sprites[i].color = c;
                }




            }
            else
            {
                Color c = target.GetComponent<SpriteRenderer>().color;
                c.a = f;

                target.GetComponent<SpriteRenderer>().color = c;

                SpriteRenderer[] sprites = target.GetComponentsInChildren<SpriteRenderer>();

                for (int i = 0; i < sprites.Length; i++)
                {

                    Color c2 = sprites[i].color;
                    c2.a = f;

                    sprites[i].color = c2;
                }


            }
            yield return null;
        }
    }


    IEnumerator CornTargetFadeOut(GameObject target, bool doDestroy)
    {

        if (target.GetComponent<SpriteRenderer>() == null)
        {

            for (float f = 1f; f > -0.1f; f -= 0.1f)
            {

                if (target)
                {
                    SpriteRenderer[] sprites = target.GetComponentsInChildren<SpriteRenderer>();

                    for (int i = 0; i < sprites.Length; i++)
                    {

                        Color c = sprites[i].color;
                        c.a = f;

                        sprites[i].color = c;
                    }
                }
                yield return null;
            }
        }
        else
        {

            for (float f = 1f; f > -0.1f; f -= 0.1f)
            {

                Color c = target.GetComponent<SpriteRenderer>().color;
                c.a = f;

                target.GetComponent<SpriteRenderer>().color = c;
                SpriteRenderer[] sprites = target.GetComponentsInChildren<SpriteRenderer>();

                for (int i = 0; i < sprites.Length; i++)
                {

                    Color c2 = sprites[i].color;
                    c2.a = f;

                    sprites[i].color = c2;
                }
                yield return null;
            }
        }
        if (doDestroy)
        {
            destroyCornTarget();
        }
    }

    private int GetResetableScore()
    {
        return resetableScore;
    }
    // here we will put logic for determining what kind of target the player should get.
    // for now just make sure the player don't get the same target twice in a row.
    int GetTargetType()
    {

        if (debugUseTargetIndex > -1)
        {
            return debugUseTargetIndex;
        }
        bool loop = true;
        int targettype = -1;
        while (loop)
        {
            targettype = (int)UnityEngine.Random.Range(0.0f, CornTargets.Length);
            if (targettype != currentTargetType)
                loop = false;
            currentTargetType = targettype;

        }
        return targettype;
    }


    void Flash()
    {
        SpriteRenderer[] renderers;

        renderers = CornTarget.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in renderers)
            if (!sr.gameObject.name.ToLower().Contains("eye"))
                sr.enabled = isFlashLit;

        background.SetActive(isFlashLit);

        if (!isFlashLit)
        {
            Camera.main.backgroundColor = Color.black;
            Invoke("Flash", Random.Range(0.3f, 2f));
        }
        else
        {
            Camera.main.backgroundColor = prevCamCol;
            Invoke("Flash", Random.Range(0.05f, 0.2f));
        }
        isFlashLit = !isFlashLit;
    }

    public enum EasterEgg { Lightning = 0, Darkness, AlsoLightning }; // UpAndDown, Backwards
    Color prevCamCol = Color.clear;
    bool isFlashLit;
    public float EASTER_EGG_CHANCE = 0.1f;

    private void InitiateEasterEgg(bool restoreOnly)
    {
        SpriteRenderer[] renderers;
        if (CornTarget == null)
            return;

        if (CornTarget.transform == null)
            return;
            
        // restore to previous
        background.SetActive(true);
        if (prevCamCol != Color.clear)
            Camera.main.backgroundColor = prevCamCol;
        prevCamCol = Color.clear;

		if (!useIcecreamMode)
            Camera.main.GetComponent<Animator>().enabled = true;

        CancelInvoke("Flash");

        if (restoreOnly)
            return;

        if (Random.Range(0f, 1f) >= EASTER_EGG_CHANCE || score < 100 || bJustUnlocked)
		{
			bJustUnlocked = false;
			return;
        }
		bJustUnlocked = false;

		int easterEgg = Random.Range(0, System.Enum.GetNames(typeof(EasterEgg)).Length);
        //easterEgg = 0;
        EasterEgg egg = (EasterEgg)easterEgg;

        switch (egg)
        {
            case EasterEgg.Darkness:
                renderers = CornTarget.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer sr in renderers)
                    if (!sr.gameObject.name.ToLower().Contains("eye"))
                        sr.enabled = false;

                background.SetActive(false);

                Camera.main.GetComponent<Animator>().enabled = false;
                prevCamCol = Camera.main.backgroundColor;
                Camera.main.backgroundColor = Color.black; // this accomplishes nothing?!
                break;

            case EasterEgg.Lightning:
            case EasterEgg.AlsoLightning: // lighning twice as likely as darkness
                isFlashLit = false;
                Camera.main.GetComponent<Animator>().enabled = false;
                prevCamCol = Camera.main.backgroundColor;
                Flash();

                break;
                /*// rainbow material/shader not working with inverted scale
                            case EasterEgg.Backwards:
                                CornTarget.transform.parent.localScale = new Vector3(-1, 1, 1);
                                break;

                            case EasterEgg.UpAndDown:
                                CornTarget.transform.parent.localScale = new Vector3(1, -1, 1);
                                break;
                */
                //case EasterEgg.Minimal: // too difficult (sometimes)
                //	CornTarget.transform.parent.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                //	break;

                //case EasterEgg.Maximal: // too weird results, like the snails ending up half outside of the screen
                //	CornTarget.transform.parent.localScale = new Vector3(2f, 2f, 2f);
                //	break;

        }

    }
    /*
	 * *
	 *
	 */
    bool useThelevelTargets = true;
    public void destroyCornTarget()
    {
        Destroy((GameObject)CornTarget);
        targetManager.NewRound();
        TargetManager.UnlockResult target = targetManager.GetTargetType(score, CornTargets.Length - 1);
        //if (InTutorialMode() == false && score > 0)
        if (score > 0)
        {
            StaticManager.AddCredits(5);
            creditsText.text = "" + StaticManager.GetNumberOfCredits();
        }


        if (target != null && target.newUnlock)
        {
            resetableScore = 0;
        }
        if (CornTargets.Length <= target.unlock)
        {
            target.unlock = GetTargetType();

        }
        if (debugUseTargetIndex != -1)
            target.unlock = debugUseTargetIndex;
        if (inEditMode)
        {


            return;
        }
        if (useNewLevelSystem)
        {
            int level = 0;

            level = targetManager.GetCurrentHardLevel(score, theCurrentTargetDifficulty);
            if (useThelevelTargets)
            {
                int maxLev = targetManager.GetLatestUnlockLevelIndex();
                if (target.newUnlock == false)
                    target.unlock = levelManager.GetTargeIndextFromLevelRestrictions(maxLev, level);

            }

            int targetIndex = 0;

            if (target.unlock > 0)
            {
                targetIndex = target.unlock;
                /*
				//	targetIndex = levelManager.GetTargeIndextFromLevelRestrictions (target.unlock);
				if (target.newUnlock == false)
				{
					//targetIndex = levelManager.GetTargetIndex(target.unlock);
					targetIndex = target.unlock;
				} else {
					targetIndex = target.unlock;
				}
				*/
            }
            if (targetIndex < 0)
                targetIndex = 0;


            if (InTutorialMode())
            {
                target.unlock = 0;
                target.newUnlock = false;
                IncreaseTutorialMode();
                //  level = 0;
                targetIndex = 0;
            }
            if (debugUseTargetIndex > -1)
            {
                targetIndex = debugUseTargetIndex;
                target.unlock = targetIndex;
            }

            CornTarget = Instantiate(CornTargets[targetIndex]);
            CornTarget.transform.SetParent(targetContainer.transform);

            if (InTutorialMode())
            {
                Transform t = GameUtil.FindDeepChild(CornTarget.transform, "TutArrow");
                if (t) t.gameObject.SetActive(true);
            }

            if (useNewLevelSystem == false && inEditMode == false && target.newUnlock == false)
            {

                int result = levelManager.InitWithTarget(CornTarget, targetIndex);
                int levelIndex = 0;
                while (result == -1 && levelManager.LevelsLeft(levelIndex))
                {
                    levelIndex++;
                    if (CornTarget != null)
                    {
                        Destroy(CornTarget);
                    }
                    targetIndex = levelManager.GetTargetIndex(target.unlock);
                    if (target.newUnlock)
                    {
                        targetIndex = target.unlock;
                    }
                    if (debugUseTargetIndex > -1)
                    {
                        targetIndex = debugUseTargetIndex;
                        target.unlock = targetIndex;
                    }
                    CornTarget = Instantiate(CornTargets[targetIndex]);
                    result = levelManager.InitWithTarget(CornTarget, targetIndex);

                    //levelManager.PauseActions(pause);
                    if (result == -1)
                    {
                        levelManager.Load();
                    }
                }
                if (result == -1)
                {
                    Debug.Log("Error!!" + levelManager.LevelsLeft(levelIndex) + " " + targetIndex);
                }
            }
            if (useNewLevelSystem && inEditMode == false && target.newUnlock == false)
            {
                // levelManager.InitCurrentLevelWithTarget(CornTarget);
                Destroy(CornTarget);
                CornTarget = levelManager.InitCurrentLevelWithRandomTarget(targetManager.CalculateDifficultyLevel(score, 0), CornTargets, targetManager.GetLatestUnlockLevelIndex());
            }
            if (target.newUnlock)
            {
                //	CornTarget = Instantiate(CornTargets[targetIndex]);
            }
            if (gameState != GameStates.unlock)
            {
                levelManager.RebindTarget(CornTarget);
                levelManager.SetDifficultyOnTarget(CornTarget);
            }


            if (InTutorialMode() == false)
            {
                levelManager.setScore(score);
                //levelManager.IncreaseLevel ();
                //levelManager.SetLevel (level);

                InitiateEasterEgg(false);
                //InitiateEasterEgg(target.newUnlock);


            }
            else
            {
                levelManager.StartTutorial();
            }

        }
        else
        {
            CornTarget = Instantiate(CornTargets[target.unlock]);
            MakingItHarder harder = CornTarget.GetComponent<MakingItHarder>();
            if (harder)
            {
                harder.SetUpDifficulty(target.unlock, score);
            }
        }
        if (gameState == GameStates.unlock)
        {
            MakingItHarder harder = CornTarget.GetComponent<MakingItHarder>();
            if (harder)
            {
                CornTarget.GetComponent<MakingItHarder>().enabled = false;
            }
            CornTarget.SetActive(false);
            CornTarget.transform.parent = unlockPrefabParent.transform;
            CornTarget.transform.localPosition = new Vector3(0, 0, 1);
            return;
        }


        if ((arrows > -1) || (arrows > 0))
        {
            // may target's position be altered?

            // if the player hit the target with the last arrow, 
            // it's set to a new random position
            /*
				float x = Random.Range (-0.3f, 2f);
				float y = Random.Range (0f, 2);
				Vector3 position = CornTarget.transform.position;
				position.x = x;
				position.y = y;
				CornTarget.transform.position = position;
	*/
            //	newArrow ();

            // findme transform.eulerAngles = new Vector3 (0, 0, -28);
            // subtract one arrow
        }

        StartCoroutine(CornTargetFadeIn(CornTarget));

    }


    public void createArrow(bool hitTarget)
    {


        if (CornTarget == null)
        {
            CornTarget = Instantiate(CornTargets[0]);
            destroyCornTarget();
            //newArrow ();
            //return;
        }

        if (hitTarget)
        {
            if (inEditMode)
            {
                levelManager.LoadLevel();
            }

            if (fadeOldTarget)
                StartCoroutine(CornTargetFadeOut(CornTarget, true));
            else
                destroyCornTarget();
        }


        Camera.main.GetComponent<camMovement>().resetCamera();
        // when a new arrow is created means that:
        // sounds has been played



        // does the player has an arrow left ?
        if ((arrows > -1 && hitTarget == true) || (arrows > 0))
        {
            newArrow();

        }
        else
        {
            // no arrow is left,
            // so the game is over
            gameState = GameStates.over;
            levelManager.RestartLevels();
            GameOverPanel.SetActive(true);
            checkHighScore();
            //string percent = HSController.Instance.GetPercent();
            //endscoreText.text = "you are in the top " + percent + "%";
            endscoreText.text = "";
            targetManager.Restart();
            creditsPanel.SetActive(false);
            animalProgress.SetActive(false);
        }
    }


    IEnumerator armthrow()
    {


        for (float f = transform.eulerAngles.z; f >= -(45 * 2); f -= 16.9f)
        {

            //	Debug.Log ("f: " + f);
            transform.eulerAngles = new Vector3(0, 0, f); //this needs to be the max of 

            yield return null;
        }

    }

    float startz = 0;

    IEnumerator rotateArrow()
    {


        for (float f = 0; f <= 340; f += 20)
        {

            //Debug.Log ("f: " + f);
            //	transform.eulerAngles = new Vector3 (0, 0, f); //this needs to be the max of 
            arrow.transform.eulerAngles = new Vector3(arrow.transform.eulerAngles.x, arrow.transform.eulerAngles.y, startz + f); //this needs to be the max of 
            yield return null;
        }

    }

    IEnumerator SetGameStateAndHideUnlock(GameStates state)
    {
        yield return new WaitForSeconds(0.1f);
        gameState = state;

        unlockObject.SetActive(false);
    }

    IEnumerator FadeUpThrower()
    {
        yield return new WaitForSeconds(2.0f);

        StartCoroutine(CornTargetFadeIn(throwerParent));
        StartCoroutine(CornTargetFadeIn(gameObject));
        arrow.SetActive(true);

    }

    IEnumerator FadeUpCornTarget(GameObject obj, GameStates state)
    {
        yield return new WaitForSeconds(2.5f);
        CornTarget.SetActive(true);
        //CornTarget.transform.position = obj.transform.position;
        CornTarget.transform.parent = null;
        obj.SetActive(false);
        StartCoroutine(CornTargetFadeIn(CornTarget));
        StartCoroutine(SetGameStateAndHideUnlock(state));
    }

	private bool bJustUnlocked = false;
    public void NewUnlock(int index)
    {
        if (gameState != GameStates.unlock)
        {
            oldState = gameState;
            gameState = GameStates.unlock;
            //		Debug.Log("New unlock!! prefab index"+ index);
            unlockObject.SetActive(true);
            //CornTarget.SetActive (false);
            GameObject newTarget = Instantiate(CornTargets[index]);
            MakingItHarder oldHarder = newTarget.GetComponent<MakingItHarder>();
            if (oldHarder)
            {
                oldHarder.enabled = false;
            }
            newTarget.transform.parent = unlockPrefabParent.transform;
            newTarget.transform.localPosition = new Vector3(0.0f, 0.0f, 1.0f);
            CornTarget.transform.position = newTarget.transform.position;
            SetColorRecursive(Color.black, newTarget);
            StartCoroutine(FadeUpCornTarget(newTarget, oldState));
            StartCoroutine(CornTargetFadeOut(throwerParent, false));
            StartCoroutine(CornTargetFadeOut(gameObject, false));
            arrow.SetActive(false);

            StartCoroutine(FadeUpThrower());

			//StartCoroutine (CornTargetFadeIn(newTarget));

			bJustUnlocked = true;
		}
    }

    public void shootArrow()
    {

        if (arrow.GetComponent<Rigidbody2D>() == null)
        {
            if (!InTutorialMode())
                arrows--;
            arrowShot = true;
            arrow.AddComponent<Rigidbody2D>();
            //Vector3 ban = (Quaternion.Euler (new Vector3 (transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)) * new Vector3 (125f * length, 0, 0));
            Vector3 ban = (Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z)) * new Vector3(125f * MaxSpeed, 0, 0));
            arrow.transform.parent = gameManager.transform;
            arrow.GetComponent<Rigidbody2D>().mass = 1.0f;

            arrow.GetComponent<Rigidbody2D>().AddForce(new Vector2(ban.x * 230, ban.y * 345), ForceMode2D.Force);
            //	arrow.GetComponent<Rigidbody2D> ().AddForceAtPosition (new Vector2 (0, ban.x * 200), new Vector2 (arrow.transform.position.x, arrow.transform.position.y - 2.0f), ForceMode2D.Force); 
            //arrow.GetComponent<Rigidbody2D> ().AddTorque (1000);
            startz = arrow.transform.eulerAngles.z;
            if (inARow > 2)
            {
                StartCoroutine("rotateArrow");
            }
            StartCoroutine("armthrow");

            mySoundFXManager.playEffect("stringRelease");
        }

        arrowPrepared = false;


        if (inARow > 3)
        {
            arrow.GetComponentInChildren<TrailRenderer>().enabled = true;
            //StaticManager.AddCredits (2);
        }

        if (inARow > 5)
        {


            ParticleSystem ps = arrow.GetComponentInChildren<ParticleSystem>();
            var emission = ps.emission;
            emission.rateOverTime = 40;
            //StaticManager.AddCredits (10);

        }


        // Cam
        Camera.main.GetComponent<camMovement>().resetCamera();
        Camera.main.GetComponent<camMovement>().setArrow(arrow);
        length = 0;
    }



    public void prepareArrow2()
    {


        if (arrowPrepared == false)
        {

            bowMoveStartTime = Time.time;
        }

        float angleZ = -20 - (Mathf.Cos((Time.time - bowMoveStartTime) * 2) * (45)) + 20;

        transform.eulerAngles = new Vector3(0, 0, angleZ);
        //	transform.localScale = new Vector3 (2.0f, 2.0f, 1.0f);
        // set the arrows position
        Vector3 arrowPosition = arrow.transform.localPosition;
        arrowPosition.x = (arrowStartX - length);
        arrow.transform.localPosition = arrowPosition;

        // determine the arrow pullout
        length += 0.05f;// mousePos.magnitude / 3f;
        length = Mathf.Clamp(length, 0, MaxSpeed);
        // set the bowstrings line renderer

        arrowPrepared = true;

    }

    void StopCoinSound()
    {
        coinSound.GetComponent<AudioSource>().loop = false;
    }

    public void SetReward()
    {
        float totaltime = 1.5f;
        score += 500;
        float s = score;
        s *= 1.25f;
        score = (int)s;
        arrows = 4;
        Camera.main.GetComponent<camMovement>().setArrow(arrow);
        RewardScoreText.GetComponent<TextMesh>().text = "" + score;
        //StaticManager.AddCredits (100);
        RewardScoreText.transform.position = arrow.transform.position;
        LeanTween.alpha(RewardScoreText, 0f, (totaltime / 2.0f)).setDelay(totaltime / 2); //fade in
        LeanTween.alpha(RewardScoreText, 1.0f, (totaltime / 2.0f));

        //LeanTween.rotateZ (HitScoreText, 10.0f, totaltime).setEaseInOutCubic ();
        //LeanTween.scaleX (HitScoreText, 1.0f, totaltime).setEaseInOutCubic ();
        //LeanTween.scaleY (HitScoreText, 1.0f, totaltime).setEaseInOutCubic ();

        //LeanTween.moveLocalZ (HitScoreText, -100.0f, .2f);
        LeanTween.moveZ(RewardScoreText, 10.0f, totaltime).setEaseInOutCubic();
        ;
        LeanTween.moveX(RewardScoreText, -8.88f, totaltime).setEaseInOutCubic();
        ;
        LeanTween.moveY(RewardScoreText, 4.86f, totaltime).setEaseInOutCubic();
        ;

        Animator anim = RewardScoreText.GetComponent<Animator>();
        /*anim.StopPlayback ();
		AnimationClip [] clips = anim.runtimeAnimatorController.animationClips;
		clips[0].
		anim.Play (clips[0].name);
*/
        anim.Play("scoretext", -1, 0f);
        arrow.GetComponentInChildren<TrailRenderer>().enabled = false;
        RewardScoreText.GetComponent<TextMesh>().text = "" + score / 2.0f;
        StartCoroutine(IncreaseScore());
    }
    //
    // public void setPoints()
    //
    // This method is called from the arrow script
    // and sets the points
    // and: if the player hit the bull's eye,
    // he receives a bonus arrow
    //

    public void setPoints(int points)
    {
        /*
		//mySoundFXManager.playEffect ("coin");

		SetMaterialRecursive (rainBowMaterial, CornTarget);

		int myPoints = points * (arrows + 1);
		int additional = 30 * (inARow + 1);
		myPoints += additional;
		// these magic numbers have been scientifically proven to work by a model called "Trial and error"
		int divider = 40;
		if (myPoints >= 100)
			divider = 60;

		int repeats = myPoints / divider;
		float delay = 0.0f + repeats;
		//coinSound.GetComponent<AudioSource> ().loop = true;
		//coinSound.GetComponent<PlayRandomSound> ().Play ();
		//Invoke ("StopCoinSound",delay);

		*/

        Transform t = GameUtil.FindDeepChild(CornTarget.transform, "TutArrow");
        if (t) t.gameObject.SetActive(false);

        //		if (CornTarget.transform.parent.localScale.x > 0 && CornTarget.transform.parent.localScale.y > 0)
        SetMaterialRecursive(rainBowMaterial, CornTarget);

        int myPoints = points;

        //myPoints *= (inARow*2);

        if (inARow > 4)
        {
            myPoints *= 10;
        }
        else if (inARow > 3)
        {
            myPoints *= 5;
        }


        float totaltime = 1.5f;

        HitScoreText.transform.position = arrow.transform.position;
        LeanTween.alpha(HitScoreText, 0f, (totaltime / 2.0f)).setDelay(totaltime / 2); //fade in
        LeanTween.alpha(HitScoreText, 1.0f, (totaltime / 2.0f));

        //LeanTween.rotateZ (HitScoreText, 10.0f, totaltime).setEaseInOutCubic ();
        //LeanTween.scaleX (HitScoreText, 1.0f, totaltime).setEaseInOutCubic ();
        //LeanTween.scaleY (HitScoreText, 1.0f, totaltime).setEaseInOutCubic ();

        //LeanTween.moveLocalZ (HitScoreText, -100.0f, .2f);
        LeanTween.moveZ(HitScoreText, 10.0f, totaltime).setEaseInOutCubic();
        ;
        LeanTween.moveX(HitScoreText, -8.88f, totaltime).setEaseInOutCubic();
        ;
        LeanTween.moveY(HitScoreText, 4.86f, totaltime).setEaseInOutCubic();
        ;

        Animator anim = HitScoreText.GetComponent<Animator>();
        /*anim.StopPlayback ();
		AnimationClip [] clips = anim.runtimeAnimatorController.animationClips;
		clips[0].
		anim.Play (clips[0].name);
*/
        anim.Play("scoretext", -1, 0f);
        arrow.GetComponentInChildren<TrailRenderer>().enabled = false;
        HitScoreText.GetComponent<TextMesh>().text = "" + myPoints;
        StartCoroutine(IncreaseScore());
        ParticleSystem ps = arrow.GetComponentInChildren<ParticleSystem>();
        var emission = ps.emission;
        emission.rateOverTime = 0;



        if (arrows == 2)
        {
            inARow++;
        }
        else
        {
            inARow = 1;
        }

        score += myPoints;
        theCurrentTargetDifficulty = levelManager.GetCurrentDifficulty(levelManager.gameObject);
        // save difficulty and use in  
        resetableScore = score;
        if (points == 50)
        {
            arrows++;

        }


    }

    public int getScore()
    {
        if (isInUnlockMode)
        {
            return GetResetableScore();
        }
        return score;

    }



    //
    // Event functions triggered by UI buttons
    //


    //
    // public void showInstructions()
    //
    // this method shows the instructions screen
    // can be triggered by main menu
    //

    public void showInstructions()
    {

    }


    //
    // public void hideInstructions()
    //
    // this method hides the instructions screen
    // and returns the player to main menu
    //

    public void hideInstructions()
    {

    }


    //
    // public void showHighscore()
    //
    // this method shows the highscore screen
    // can be triggered by main menu
    //

    public void showHighscore()
    {
        newHighscoreText.gameObject.SetActive(true);

        //	actualHighscoreText.text = "Actual Hiscore: " + PlayerPrefs.GetInt ("Score") + " points";
        newHighscoreText.text = "New high score: " + score + " points!";
        int currentHighScore = PlayerPrefs.GetInt("Score");
        if (score > currentHighScore)
        {
            newHighText.enabled = true;
            SaveHighScore();
            Rate();
        }
        else
        {
            newHighText.enabled = false;
        }
    }

    public void Awake()
    {
#if !UNITY_IOS
        //dreamloLeaderBoard ld = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
        //ld.LoadScores ();
#else
		LeaderboardManager.AuthenticateToGameCenter ();
#endif

        if (useIcecreamMode)
        {
            mainModeContainer.SetActive(false);
            iceCreamModeContainer.SetActive(true);
            popParticles = iceCreamModeContainer.transform.Find("Cone pop").GetComponent<ParticleSystem>();
            background = iceCreamModeContainer.transform.Find("Ice bg").gameObject;
            Camera.main.GetComponent<Animator>().enabled = false;
            Camera.main.backgroundColor = new Color(75f / 255f, 1, 193f / 255f);
            arrowPrefab = iceCreamArrowPrefab;
        }
    }

    public void SendHighScore()
    {
#if UNITY_IOS
		//LeaderboardManager.ReportScore (score, leaderBoardID);
#else
        /*
		dreamloLeaderBoard ld = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
		gameState = GameStates.over;
		HighScorePanel.SetActive(false);
		GameObject obj = HighScorePanel.transform.Find ("InputField").gameObject;
		string name = obj.GetComponent<InputField> ().text;

		ld.AddScore(name, score);
		*/

#endif

    }

    public void SaveHighScore()
    {
        if (score > PlayerPrefs.GetInt("Score"))
        {
            PlayerPrefs.SetInt("Score", score);
            PlayerPrefs.Save();
#if !UNITY_IOS

            /* dreamloLeaderBoard ld = dreamloLeaderBoard.GetSceneDreamloLeaderboard();
			if (score > ld.GetLowestScore ()) {
				//	ld.AddScore("Mattias", score);
				HighScorePanel.SetActive(true);
				gameState = GameStates.sendhighscore;
			} */
#elif UNITY_IOS

			LeaderboardManager.ReportScore (score, leaderBoardID);

#endif
			// HSController.Instance.setScoreAndUpload(score);

		}

    }
    //
    // public void hideHighscore()
    //
    // this method hides the highscore screen
    // set the highscore, if neccessary
    // and returns the player to main menu
    //
    public void hideHighScore()
    {

        SaveHighScore();
        resetGame();
    }


    //
    // public void checkHighScore()
    //
    // this method is called after the game over screen
    // it checks for a new high score and displays the
    // highscore screen, if neccessary - else the menu screen

    public void checkHighScore()
    {
        int highScore = PlayerPrefs.GetInt("Score");
        if (score > highScore)
        {
            showHighscore();

        }
        else
        {

            resetGame();
        }
    }

    //
    // public void startGame()
    //
    // this method starts the game
    // can be triggered by main menu
    //

    public void startGame()
    {
        gameState = GameStates.game;
        endscoreText.text = "";
        score = 0;
        resetableScore = 0;
        showScore();
        //StaticManager.AddCredits (1);
    }

    public void showMenu()
    {

        gameState = GameStates.menu;
        resetGame();
    }

    #region PRIVATE_VARIABLES

    string leaderBoardID = "se.pastille.unicorn.leadboard";

    #endregion

    #region BUTTON_EVENT_HANDLER

    /// <summary>
    /// Raises the login event.
    /// </summary>
    /// <param name="id">Identifier.</param>
    public void OnLogin(string id)
    {
        LeaderboardManager.AuthenticateToGameCenter();
    }

    /// <summary>
    /// Raises the show leaderboard event.
    /// </summary>
    public void OnShowLeaderboard()
    {
        LeaderboardManager.ShowLeaderboard();
    }

    /// <summary>
    /// Raises the post score event.
    /// </summary>
    public void OnPostScore()
    {
        LeaderboardManager.ReportScore(score, leaderBoardID);
    }

    #endregion
}
