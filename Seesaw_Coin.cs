/* 
	Script: Seesaw_EnemyPlane.cs
	Author: Sergio Schiavo
	Project: Seesaw
	
 	Description: 
	Simple class responsible for the "Coins" events.
 
 */
 
using UnityEngine;
using System.Collections;

public class Seesaw_Coin : MonoBehaviour {

	GameObject flea;
	bool collected = false;
	bool Magnetic = false;
	srcGameController gController;
	AudioSource audioSource;

	// Use this for initialization
	void Start () {
		gController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<srcGameController> ();
		audioSource = gameObject.AddComponent<AudioSource>();

		if(Random.Range (0, 2)==0)
			audioSource.clip = gController.CoinAudio1;
		else
			audioSource.clip = gController.CoinAudio2;
	}
	
	void Update () {
		if(Magnetic && flea != null) transform.position = Vector3.Lerp(transform.position, flea.transform.position, Time.deltaTime * 10f);
	}

	void OnTriggerEnter2D(Collider2D other) {

		if (collected) return;
		if (other.tag == "Flea")
		{
			collected = true;
			this.gameObject.GetComponent<Animator> ().SetTrigger ("Get");
			gController.MaxCoinsCollected++;
			audioSource.Play ();

		}
		else if(other.tag == "MagneticField")
		{
			flea = other.gameObject.transform.parent.gameObject;
			Magnetic = true;
		}
	}

	void destroyMe()
	{
		Destroy (this.gameObject);
	}
}
