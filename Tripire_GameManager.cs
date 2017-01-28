/* 
	Script: srcTripireGameManager.cs
	Author: Sergio Schiavo
	Project: Tripire
	
 	Description: 
	This is the main class (Game Manager) from a image trivia game I started the development called "Tripire". It has some common methods responsible for displaying information (debug and in-game).
	
	Also this class is responsible for fetching the image (and its informations) from a server via RESTful API in JSON format, and create the GameObject accordingly.
 
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;


public class Tripire_GameManager : MonoBehaviour {

    //Global Game Settings
    string url = "http://rest.tripire.com/images?level=24&lang=en&number=30";
    int NumImagesLoad = 2;
    
    //Debug labels
    public Text txtAnswer,txtRight, txtWrong;

    int countRight = 0, countWrong = 0;
    
	public GameObject GameItem, GameItemContainer, pnlLoading, pnlDebug;

	public Animator AnswerType;
	
	float imageHiddenPos;

	List<GameObject> _gameImages = new List<GameObject>();

	[HideInInspector]
	public int imageLoading = 0;
    [HideInInspector]
    public string currentCorrectAnswer = "";


	void Start () {
        
		
	}

	void Update(){

        //set the "correct answer" label with the current correct answer, for debug purposes
        if(txtAnswer.text != currentCorrectAnswer) txtAnswer.text = currentCorrectAnswer;
	}

	public void StartGame()
	{
		//loads first image gain time.
		WWW www = new WWW(url);
		StartCoroutine(WaitForRequest(www));
		
		//this variable saves the width of the RectTransform, so it can dynamically vary depending on the device screen
		imageHiddenPos = transform.GetComponent<RectTransform> ().rect.width;
	}


	IEnumerator WaitForRequest(WWW www)
	{
        //"Downloading image set information"
		yield return www;
		if (www.error == null)
		{
			//"Download finished"
			JSONNode cardData = JSON.Parse (www.text);
			for (int j = 0; j < cardData.Count; j++) {

				GameObject currentImage = Instantiate (GameItem, this.transform.position, this.transform.rotation) as GameObject;
                currentImage.transform.SetParent (GameItemContainer.gameObject.transform);

				RectTransform currentImage_transf = currentImage.GetComponent<RectTransform> ();
                currentImage_transf.offsetMin = new Vector2(0, 0);
                currentImage_transf.offsetMax = new Vector2(0, 0);
                currentImage.name = j+"_image_" + cardData [j] ["id_image"].Value;
				srcGameImage currentImageSrc = currentImage.GetComponent<srcGameImage> ();
                currentImageSrc.SetImage (cardData [j] ["folder"].Value, cardData [j] ["countdown"].Value, cardData [j] ["url"].Value);
                currentImageSrc.GameManager = this.gameObject.GetComponent<srcGame> ();
                currentImage.transform.position = new Vector3((imageHiddenPos + (imageHiddenPos/2))  , currentImage.transform.position.y, currentImage.transform.position.z);
                currentImageSrc.ImageLimit = (imageHiddenPos);
				_gameImages.Add(currentImage.gameObject);
				if (j <= NumImagesLoad) currentImageSrc.LoadObject ();
				if (j == 0) currentImageSrc.SetAsNextObject ();
			}
		} else {
			Debug.Log("Error downloading image: " + www.error);
		}    
	}


	public void PostAnswer(bool correct)
	{		
		GameObject gameImg = _gameImages [0] as GameObject;

		if (gameImg != null) {
			_gameImages.RemoveAt (0);

			if (correct) countRight++;
			else countWrong++;

			txtRight.text = "Correct: " + countRight.ToString ();
			txtWrong.text = "Wrong: " + countWrong.ToString ();

			if (_gameImages.Count > 0) _gameImages [0].GetComponent<srcGameImage> ().SetAsNextObject ();				
			if (imageLoading > 0) pnlLoading.SetActive (true);
		}			
	}
	public void LoadNextObject()
	{
		if (_gameImages.Count > NumImagesLoad) _gameImages [NumImagesLoad].GetComponent<srcGameImage> ().LoadObject ();
	}


	public void ToggleDebugMode()
	{
		pnlDebug.SetActive (!pnlDebug.activeSelf);
	}

    public void ClearGameItems()
    {
        if(GameItemContainer.transform.childCount> 0) foreach (Transform child in GameItemContainer.transform) Destroy(child.gameObject);
    }
}
