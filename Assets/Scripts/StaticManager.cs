using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

public static class StaticManager : System.Object {

	[Serializable]
	public class LevelData
	{
		public bool bIsActive;
		public int stars;
		public int score;
		public float time;
	}

	[Serializable]
	public class WorldData
	{
		public bool bIsAvailable;
		public bool bIsEnabled;
		public int nofStarsToUnlock;
	}

	[Serializable]
	public class GameSettings
	{
		public bool bPlayMusic;
		public bool bPlaySounds;
	}

	[Serializable]
	public class SingleAvatar
	{
		public SingleAvatar(bool unlocked, string name) {
			this.unlocked = unlocked;
			this.name = name;
		}

		public SingleAvatar() {
			this.unlocked = false;
			this.name = "";
		}

		public bool unlocked;
		public string name;
	}

	[Serializable]
	public class AvatarData
	{
		public int selectedAvatarIndex;
		public string selectedAvatarName;
		public SingleAvatar [] avatars;
		public int nofAvatars;
	}

	[Serializable]
	public class CreditData
	{
		public int credits;
		public int coins;
	}

	[Serializable]
	public class GameData
	{
		public float version;
		public WorldData [] worlds;
		public LevelData [,] levels;
		public GameSettings settings;
		public AvatarData avatarData;
		public CreditData creditData;
		public int worldIndex;
		public int levelIndex;
		public int lastWorldIndex;
		public int lastLevelIndex;
	}

	private const float version = 1.0f; 
	private const string saveName = "/GameInfo1c.dat";
	private static GameData gameData;
	private const int MAXWORLDS = 10;
	private const int MAXLEVELS = 30;
	private const int MAXAVATARS = 50;

	private static bool bWasLoaded = false;
	private static int nofWorlds = 0;

	private static LevelData achievedResults = new LevelData();
	private static LevelData oldResults = new LevelData();

	private static List<string> sceneStack = new List<string>();

	public static bool usePlayLevelScreen = false;

	private static int unlockedAvatarIndex = -1;

	private static int temporaryCredits = 0;

	private static bool _showGameIntro;

	public static bool showGameIntro
	{
		get { return _showGameIntro; }
		set { _showGameIntro = value; }
	}

	public static float GetTimeStepMul() {
		return 0.02f / Time.fixedDeltaTime; // default fixed timestep / current timestep
	}
		
	private static int pushCounter = 0;
	public static float originalLevelTimeStep = 0.02f;

	public static int PushFixedTimeStep (float timeStep) {
		Time.fixedDeltaTime = timeStep;
		pushCounter++;
		return pushCounter;
	}

	public static void RestoreTimeStep(int timeStepPushIndex) {
		if (timeStepPushIndex == pushCounter)
			Time.fixedDeltaTime = originalLevelTimeStep;
	}

	private static void Prepare() {
		if (!bWasLoaded)
			Load ();
	}

	public static int GetNumberOfCredits() {
		Prepare ();
		return gameData.creditData.credits;
	}

	public static void AddCredits(int credits) {
		Prepare ();
		if (credits < 1)
			return;
		
		gameData.creditData.credits += credits;
		Save ();
	}

	public static void AddTemporaryCredits(int credits, bool startFromZero = false) {
		if (startFromZero)
			temporaryCredits = credits;
		else
			temporaryCredits += credits;
	}

	public static void ResetTemporaryCredits() {
		temporaryCredits = 0;
	}

	public static int GetTemporaryCredits() {
		return temporaryCredits;
	}

	public static void StoreTemporaryCredits() {
		if (temporaryCredits > 0)
			AddCredits (temporaryCredits);
		temporaryCredits = 0;
	}

	public static bool IsAvatarUnlocked(int index) {
		
		Prepare ();
		// Debug.Log (index);
		return gameData.avatarData.avatars [index].unlocked;
	}
		
	public static void UnlockAvatar(int index, int creditPrize) {
		Prepare ();
		gameData.avatarData.avatars [index].unlocked = true;
		gameData.creditData.credits -= creditPrize;
		if (gameData.creditData.credits < 0) // sanity check, not supposed to happen
			gameData.creditData.credits = 0;
		Save ();
	}

	// generate initial (alphabetically sorted), run in Prefabs/Characters:  echo { ; for f in *.prefab; do echo new SingleAvatar\(false, \"${f:0:${#f}-7}\"\), ; done ; echo }\;
	private static bool UpdateAvatars (AvatarData ad) {
		bool hasChanged = false;

		SingleAvatar[] avatars = new SingleAvatar[]
		{
			new SingleAvatar(true, "target Dino"),
			new SingleAvatar(false, "target Pig"),
			new SingleAvatar(false, "target lama"),
			new SingleAvatar(false, "target bird"),
			new SingleAvatar(false, "flyingDolphin"),
			new SingleAvatar(false, "flyingPig"),
			new SingleAvatar(false, "TargetChicken"),
			new SingleAvatar(false, "Horse"),
			/*	new SingleAvatar(false, "target rainbowskull"),
			new SingleAvatar(false, "Unicorn"),
			new SingleAvatar(false, "Elf"),
			new SingleAvatar(false, "Franky Fire"),
			new SingleAvatar(false, "Girle Girl"),
			new SingleAvatar(false, "Johan"),
			new SingleAvatar(false, "Knight Kato"),
			new SingleAvatar(false, "Morty the Mummy"),
			new SingleAvatar(false, "Ninja Go Camo"),
			new SingleAvatar(false, "Ninja Go Ninja"),
			new SingleAvatar(false, "Ninja Go Vanish"),
			new SingleAvatar(false, "Pirate Pilt"),
			new SingleAvatar(false, "Pumki"),
			new SingleAvatar(false, "Roberto Roboto"),
			new SingleAvatar(false, "Rumbo Strong"),
			new SingleAvatar(false, "Rumbo"),
			new SingleAvatar(false, "Santa Claus"),
			new SingleAvatar(false, "Skelly Mel"),
			new SingleAvatar(false, "Snowy the Man"),
			new SingleAvatar(false, "Sporty"),
			new SingleAvatar(false, "Swampy the Eye Small"),
			new SingleAvatar(false, "Swampy the Eye"),
			new SingleAvatar(false, "Teachy Tess"),
			new SingleAvatar(false, "Undead Yeti Strong"),
			new SingleAvatar(false, "Undead Yeti"),
			new SingleAvatar(false, "Wizard Wade"),
			new SingleAvatar(false, "Wizard Wicked Wade"),
			new SingleAvatar(false, "Zombie Braains"),
			new SingleAvatar(false, "Zombie Zue"),
			*/
		};

		for (int i = 0; i < avatars.Length; i++) {
			if (avatars [i].name != ad.avatars [i].name) {
				hasChanged = true;
				break;
			}
		}

		ad.selectedAvatarIndex = 0;
		for (int i = 0; i < avatars.Length; i++) {
			if (avatars [i].name == ad.selectedAvatarName) {
				ad.selectedAvatarIndex = i;
				break;
			}
		}

		foreach (SingleAvatar sa in avatars) {
			foreach (SingleAvatar sa_saved in ad.avatars) {
				if (sa_saved.name == sa.name) {
					if (sa_saved.unlocked)
						sa.unlocked = sa_saved.unlocked;
				}
			}
		}

		for (int i = 0; i < MAXAVATARS; i++) {
			if (i < avatars.Length) {
				ad.avatars [i].name = avatars [i].name;
				ad.avatars [i].unlocked = avatars [i].unlocked;
			} else {
				ad.avatars [i].name = "";
				ad.avatars [i].unlocked = false;
			}
		}

		if (ad.nofAvatars != avatars.Length)
			hasChanged = true;

		ad.nofAvatars = avatars.Length;

		return hasChanged;
	}


	public static bool ReorderPrefabs () {
		Prepare();

		CharacterManager cm = GameObject.FindGameObjectWithTag ("CharacterManager").GetComponent<CharacterManager> ();

		for (int i = 0; i < gameData.avatarData.nofAvatars; i++) {
			
			SingleAvatar sa = gameData.avatarData.avatars[i];

			bool found = false;
			foreach (GameObject g in cm.unorderedCharacterPrefabs) {
				if (g.name == sa.name) {
					found = true;
					cm.characterPrefabs.Add (g);
				}
			}

			if (!found) {
				Debug.Log ("PANIC!! All character names in StaticManager data must be possible to find among CharacterManager prefab names!"+sa.name);
				return true;
			}

		}

		return true;
	}


	public static void Load(bool erase = false) {
		int i, j;
		  
		if (erase && File.Exists (Application.persistentDataPath + saveName)) {
			File.Delete (Application.persistentDataPath + saveName);
		}
		nofWorlds = 0;

		// this loop should most likely be removed when we later set StaticLevels.requiredStarsPerWorld[] manually
		int requiredStars = 0;
		for (i = 0; i < MAXWORLDS; i++) {
			StaticLevels.requiredStarsPerWorld[i] = requiredStars;
			requiredStars += StaticLevels.levelsPerWorld [i] * 2 + 1;
		}

		// Initial values
		gameData = new GameData ();
		gameData.version = version;
		gameData.worldIndex = gameData.levelIndex = gameData.lastWorldIndex = gameData.lastLevelIndex = 0;
		gameData.settings = new GameSettings ();
		gameData.settings.bPlayMusic = true;
		gameData.settings.bPlaySounds = true;
		gameData.creditData = new CreditData();
		gameData.creditData.coins = 0;
		gameData.creditData.credits = 0;
		gameData.avatarData = new AvatarData ();
		gameData.avatarData.selectedAvatarIndex = 0;
		gameData.avatarData.selectedAvatarName = "";
		gameData.avatarData.avatars = new SingleAvatar[MAXAVATARS];
		for (i = 0; i < MAXAVATARS; i++) {
			gameData.avatarData.avatars [i] = new SingleAvatar ();
		}
		gameData.avatarData.nofAvatars = 0;

		gameData.worlds = new WorldData[MAXWORLDS];
		gameData.levels = new LevelData[MAXWORLDS, MAXLEVELS];
		for (i = 0; i < MAXWORLDS; i++) {
			for (j = 0; j < MAXLEVELS; j++) {
				gameData.levels [i, j] = new LevelData ();
				gameData.levels [i, j].score = 0;
				gameData.levels [i, j].time = float.MaxValue;
				gameData.levels [i, j].stars = 0;
				gameData.levels [i, j].bIsActive = (j < StaticLevels.levelsPerWorld [i]);
			}
			gameData.worlds [i] = new WorldData ();
			gameData.worlds [i].bIsAvailable = StaticLevels.levelsPerWorld [i] > 0;
			gameData.worlds [i].bIsEnabled = (i == 0);
			gameData.worlds [i].nofStarsToUnlock = StaticLevels.requiredStarsPerWorld[i];
			if (gameData.worlds [i].bIsAvailable)
				nofWorlds++;
		}

		if (File.Exists (Application.persistentDataPath + saveName)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (Application.persistentDataPath + saveName, FileMode.Open);
			gameData = (GameData)bf.Deserialize (file);
			file.Close ();

			UpdateLoad ();
		}

		bool wasChanged = UpdateAvatars (gameData.avatarData);
		if (wasChanged)
			Save ();

		bWasLoaded = true;

	}

	// This function is supposed to be used if you don't want progress to be destroyed, but at the same time you want to update the saved data to reflect new levelsPerWorld settings
	public static void UpdateLoad() {
		int i, j;

		if (!File.Exists (Application.persistentDataPath + saveName)) {
			Load ();
			return;
		}

		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + saveName, FileMode.Open);
		gameData = (GameData)bf.Deserialize (file);
		file.Close ();

		nofWorlds = 0;

		for (i = 0; i < MAXWORLDS; i++) {
			for (j = 0; j < MAXLEVELS; j++) {
				if (j >= StaticLevels.levelsPerWorld [i]) {
					gameData.levels [i, j].score = 0;
					gameData.levels [i, j].time = float.MaxValue;
					gameData.levels [i, j].stars = 0;
					gameData.levels [i, j].bIsActive = false;
				} else {
					gameData.levels [i, j].bIsActive = true;
				}
			}
			gameData.worlds [i].bIsAvailable = StaticLevels.levelsPerWorld [i] > 0;
			if (gameData.worlds [i].bIsAvailable)
				nofWorlds++;
			else
				gameData.worlds [i].bIsEnabled = false;
		}

		Save ();
	}


	public static void Save ()
	{
		BinaryFormatter bf = new BinaryFormatter ();
		FileStream file = File.Open (Application.persistentDataPath + saveName, FileMode.OpenOrCreate);

		bf.Serialize (file, gameData);
		file.Close ();
	}

	public static int GetNofStars() {
		Prepare ();

		int i, j, stars = 0;

		for (i = 0; i < MAXWORLDS; i++) {
			for (j = 0; j < MAXLEVELS; j++) {
				if (gameData.levels [i, j].bIsActive)
					stars += gameData.levels [i, j].stars;
			}
		}
		return stars;
	}

	public static void GotoProgressScreen(LevelData results) {
		Prepare ();

		achievedResults = results;

		oldResults.score = gameData.levels [gameData.worldIndex, gameData.levelIndex].score;
		oldResults.stars = gameData.levels [gameData.worldIndex, gameData.levelIndex].stars;
		oldResults.time = gameData.levels [gameData.worldIndex, gameData.levelIndex].time;

		if (results.score > gameData.levels [gameData.worldIndex, gameData.levelIndex].score)
			gameData.levels [gameData.worldIndex, gameData.levelIndex].score = results.score;

		if (results.stars > gameData.levels [gameData.worldIndex, gameData.levelIndex].stars)
			gameData.levels [gameData.worldIndex, gameData.levelIndex].stars = results.stars;

		if (results.time < gameData.levels [gameData.worldIndex, gameData.levelIndex].time)
			gameData.levels [gameData.worldIndex, gameData.levelIndex].time = results.time;

		Save ();

		bool newWorld = false;

		/* // Use this to unlock world when finished last level of world, regardless of stars 
		if (IsLastLevelOfCurrentWorld () && gameData.worlds [gameData.worldIndex + 1].bIsEnabled == false && gameData.worlds [gameData.worldIndex + 1].bIsAvailable == true) {
			newWorld = true;
			gameData.worlds [gameData.worldIndex + 1].bIsEnabled = true;
			Save ();
		} */

		if (GetNofStars() >= gameData.worlds [gameData.worldIndex + 1].nofStarsToUnlock && gameData.worlds [gameData.worldIndex + 1].bIsEnabled == false && gameData.worlds [gameData.worldIndex + 1].bIsAvailable == true) {
			newWorld = true;
			gameData.worlds [gameData.worldIndex + 1].bIsEnabled = true;
			Save ();
		}

		if (newWorld) {
			PushScene ("LevelProgress"); 
			SceneManager.LoadScene ("NewWorld");
		}
		else
			SceneManager.LoadScene ("LevelProgress");
	}

	public static LevelData GetPlayResults() {
		return achievedResults;
	}
	public static LevelData GetOldPlayResults() {

		return oldResults;
	}

	public static LevelData GetCurrentLevelData() {
		Prepare ();	

		return gameData.levels[gameData.worldIndex, gameData.levelIndex];
	}

	public static void SetCurrentScene() { // the use of this is to set the current level based on our current level. This is useful if we want to start not from main screen but from a level scene
		Prepare ();

		string currentSceneName = SceneManager.GetActiveScene ().name;

		string[] split = currentSceneName.Split ('_');
		if (split.Length == 3) {
			gameData.worldIndex = gameData.lastWorldIndex = int.Parse (split [1]);
			gameData.levelIndex = gameData.lastLevelIndex = int.Parse (split [2]);     
			//Debug.Log ("LVL: " + currentSceneName + "   " + gameData.worldIndex + " : " + gameData.levelIndex);
		}
		Save ();

		/* overkill, save for later use. w is letter, d is number. @ in front of string to not have to use double-backslash
		var r = new Regex(@"(\w+)_(\d+)_(\d+)", RegexOptions.IgnoreCase); var match = r.Match(currentSceneName); if(match.Success) { worldIndex = int.Parse(match.Groups[2].Value); levelIndex = int.Parse(match.Groups[3].Value);} */
	}

	private static void LoadCurrentScene() {
		SceneManager.LoadScene (GetCurrentSceneString());
	}

	public static void LoadLastPlayedScene() {
		Prepare ();
		SceneManager.LoadScene ("level_" + gameData.lastWorldIndex + "_" + gameData.lastLevelIndex);
	}

	public static void SetSceneToLastPlayedScene() {
		Prepare ();
		gameData.worldIndex = gameData.lastWorldIndex;
		gameData.levelIndex = gameData.lastLevelIndex;
	}


	private static string GetCurrentSceneString() {
		Prepare ();
		return "level_" + gameData.worldIndex + "_" + gameData.levelIndex;
	}


	public static void StartNextLevel() {
		Prepare ();

		gameData.levelIndex++;
		if (gameData.levels [gameData.worldIndex, gameData.levelIndex].bIsActive == false) { // world is finished, change world
			gameData.levelIndex = 0;
			gameData.worldIndex++;
			gameData.worlds [gameData.worldIndex].bIsEnabled = true;

			if (gameData.worldIndex >= nofWorlds) { // no more worlds. Safety check, we should not get here because the button for next level should not be active in this case
				gameData.worldIndex = gameData.levelIndex = 0;
				return;
			}
		}
		Save ();

		LoadCurrentScene ();
	}

	public static bool IsNextWorldEnabled() {
		Prepare ();

		if (gameData.worlds[gameData.worldIndex + 1].bIsAvailable && gameData.worlds[gameData.worldIndex + 1].bIsEnabled)
			return true;
		return false;
	}

	public static bool IsLastLevelOfCurrentWorld() {
		Prepare ();

		if (gameData.levels [gameData.worldIndex, gameData.levelIndex + 1].bIsActive == false)
			return true;
		return false;
	}

	public static bool IsLastLevelOfAllWorlds() {
		if (!IsLastLevelOfCurrentWorld ())
			return false;
		if (gameData.worldIndex + 1 >= nofWorlds)
			return true;
		return false;
	}


	public static void SetLevel(int levelIndex, int worldIndex = -1) {
		Prepare ();

		gameData.levelIndex = levelIndex;
		if (worldIndex >= 0)
			gameData.worldIndex = worldIndex;
		Save ();
	}

	public static void StartLevel(int levelIndex, int worldIndex = -1) {
		SetLevel (levelIndex, worldIndex);
		LoadCurrentScene ();
	}

	public static void GotoLevelSelectScreen(int worldIndex) {
		Prepare ();

		gameData.worldIndex = worldIndex;
		gameData.levelIndex = 0;
		Save ();

		SceneManager.LoadScene ("LevelSelect");
	}

	public static void RestartSameLevel() {
		Prepare ();
		LoadCurrentScene ();
	}

	public static void GetLevel (out int worldIndex, out int levelIndex) {
		Prepare ();
		worldIndex = gameData.worldIndex;
		levelIndex = gameData.levelIndex;
	}

	public static string GetLevelString () {
		Prepare ();
		return (gameData.worldIndex + 1) + ":" + (gameData.levelIndex + 1);
	}

	public static LevelData [] GetAllLevelsForWorld (int worldIndex = -1) {
		Prepare ();

		if (worldIndex < 0)
			worldIndex = gameData.worldIndex;
		
		LevelData [] levels = new LevelData[StaticLevels.levelsPerWorld[worldIndex]];

		for (int i = 0; i < StaticLevels.levelsPerWorld[worldIndex]; i++)
			levels[i] = gameData.levels[worldIndex, i];
			
		return levels;
	}

	public static WorldData [] GetWorlds () {
		Prepare ();

		WorldData [] worlds = new WorldData[nofWorlds];

		for (int i = 0; i < nofWorlds; i++)
			worlds[i] = gameData.worlds[i];

		return worlds;
	}

	public static bool IsMusicOn() {
		Prepare ();
		return gameData.settings.bPlayMusic;
	}

	public static bool IsSfxOn() {
		Prepare ();
		return gameData.settings.bPlaySounds;
	}

	public static void ToggleMusic() {
		Prepare ();
		gameData.settings.bPlayMusic = !gameData.settings.bPlayMusic;
		Save();
		// turn music on/off here
	}

	public static void ToggleSfx() {
		Prepare ();
		gameData.settings.bPlaySounds = !gameData.settings.bPlaySounds;
		Save();
		// turn SFX on/off here
	}

	public static void PushScene(string scene = null) {
		if (scene == null)
			sceneStack.Add (SceneManager.GetActiveScene ().name);
		else
			sceneStack.Add (scene);
	}

	public static void PopScene() {
		if (sceneStack.Count <= 0)
			return;
		
		string loadScene = sceneStack[sceneStack.Count - 1];
		sceneStack.RemoveAt (sceneStack.Count - 1);
		// Debug.Log (sceneStack.Count);
		SceneManager.LoadScene (loadScene);
	}


	public static bool IsSoundEffectsEnabled() {
		Prepare ();
		return gameData.settings.bPlaySounds;
	}

	public static bool IsMusicEnabled() {
		Prepare ();
		return gameData.settings.bPlayMusic;
	}

	public static int GetSelectedAvatarIndex() {
		Prepare ();
		return gameData.avatarData.selectedAvatarIndex;
	}

	public static void SetSelectedAvatarIndex(int index) {
		Prepare ();
		if (index >= 0) {
			gameData.avatarData.selectedAvatarIndex = index;
			gameData.avatarData.selectedAvatarName = gameData.avatarData.avatars [index].name;
			Save ();
		}
	}

	public static int GetUnlockedAvatarIndex() {
		return unlockedAvatarIndex;
	}

	public static void SetUnlockedAvatarIndex(int index) {
		unlockedAvatarIndex = index;
	}

}
