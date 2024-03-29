using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
 

public class HSController : MonoBehaviour
{
	private static HSController instance6;
	
	public static HSController Instance
	{
		get { return instance6; }
	}


    public static string uniqueID = "unicorn";
    private string secretKey = "refraf"; // Edit this value and make sure it's the same as the one stored on the server
    string addScoreURL = "shuriken.se/bunnyscore/addscore.php?"; //be sure to add a ? to your url
    string highscoreURL = "shuriken.se/bunnyscore/display.php";
    string getPositionURL = "shuriken.se/bunnyscore/getPosition.php?uniqueID=bunnytower11&name=";  //TODO refactor so that the uniqueID is not smack in the middle
                                                                                                  

    private string name3;
    int score;


	void Awake() {
		
	//	DontDestroyOnLoad (gameObject);
		// If no Player ever existed, we are it.
		if (instance6 == null)
			instance6 = this;
		// If one already exist, it's because it came from another level.
		else if (instance6 != this) {
			Destroy (gameObject);
			return;
		}
		getPositionURL = "shuriken.se/bunnyscore/getPosition.php?uniqueID="+uniqueID+"&name=";
     
        if( SystemInfo.deviceUniqueIdentifier != SystemInfo.unsupportedIdentifier )
            name3 = "a_"+SystemInfo.deviceUniqueIdentifier;
        else if (SystemInfo.deviceName != SystemInfo.unsupportedIdentifier) 
            name3 = "b_" +SystemInfo.deviceUniqueIdentifier;
        else
            name3 = "c_" +System.Guid.NewGuid().ToString();

            
        score = 0;
	}
	void Start(){
        // startPostScores();
        //		startGetScores ();

        //
        //HSController.Instance.startGetScores ();

        startGetPosition(); // You get to know that you are better then hmm hmm percent here!
  	}

 


	public string[] onlineHighscore;

	public void startGetScores()
	{
		StartCoroutine(GetScores());
	}

    public void startGetPosition()
    {
        StartCoroutine(GetPosition());
    }

	public void startPostScores()
	{	
		StartCoroutine(PostScores());
	}

 

	string Md5Sum(string strToEncrypt)
	{
		System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
		byte[] bytes = ue.GetBytes(strToEncrypt);
		
		// encrypt bytes
		System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
		byte[] hashBytes = md5.ComputeHash(bytes);
		
		// Convert the encrypted bytes back to a string (base 16)
		string hashString = "";
		
		for (int i = 0; i < hashBytes.Length; i++)
		{
			hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
		}
		
		return hashString.PadLeft(32, '0');
	}
	
	// remember to use StartCoroutine when calling this function!
	IEnumerator PostScores()
	{
 	
		//This connects to a server side php script that will add the name and score to a MySQL DB.
		// Supply it with a string representing the players name and the players score.
		string hash = Md5Sum(name3 + score + secretKey);
		//string post_url = addScoreURL + "name=" + WWW.EscapeURL(name) + "&score=" + score + "&hash=" + hash;
		string post_url = addScoreURL + "uniqueID=" + uniqueID+ "&name=" + WWW.EscapeURL (name3) + "&score=" + score+ "&hash=" + hash;
		 Debug.Log ("post url " + post_url);
		// Post the URL to the site and create a download object to get the result.
		WWW hs_post = new WWW("https://"+post_url);
		yield return hs_post; // Wait until the download is done
		
		if (hs_post.error != null)
		{
			print("There was an error posting the high score: " + hs_post.error);
		}
     
	}

    public void setScoreAndUpload(int scoreIn)
    {
        score = scoreIn;
        StartCoroutine(PostScores());
    }
	
	// Get the scores from the MySQL DB to display in a GUIText.
	// remember to use StartCoroutine when calling this function!
	IEnumerator GetScores()
	{

  	//	Scrolllist.Instance.loading = true;

		WWW hs_get = new WWW("https://"+highscoreURL);

		yield return hs_get;
		
		if (hs_get.error != null)
		{
			 Debug.Log("There was an error getting the high score: " + hs_get.error);

		}
		else
		{

			//Change .text into string to use Substring and Split
			string help = hs_get.text;

			//help= help.Substring(5, hs_get.text.Length-5);
			//200 is maximum length of highscore - 100 Positions (name+score)

			onlineHighscore  = help.Split(";"[0]);
	
            Debug.Log("onlineHighscore: " + onlineHighscore.ToString());
            Debug.Log("help: " + help);


		}
		//Scrolllist.Instance.loading = false;
		//Scrolllist.Instance.getScrollEntrys ();

      
	}
	public string GetPercent() {
		return onlineHighscore[0];
	}
    IEnumerator GetPosition()
    {

        WWW hs_get = new WWW("https://" + getPositionURL + WWW.EscapeURL(name3));
        Debug.Log("whaaaa: " + "https://" + getPositionURL + WWW.EscapeURL(name3));


        yield return hs_get;

        if (hs_get.error != null)
        {
            Debug.Log("There was an error getting the high score: " + hs_get.error);

        }
        else
        {

          
            string text = hs_get.text;
			onlineHighscore[0] = text;       
            Debug.Log("help: " + text);


        }
   

    }



}
