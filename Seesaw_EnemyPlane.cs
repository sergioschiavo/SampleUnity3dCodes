/* 
	Script: Seesaw_EnemyPlane.cs
	Author: Sergio Schiavo
	Project: Seesaw
	
 	Description: 
	Class responsible for controlling moviments and events from the "Enemy Plane".
 
 */
 
using UnityEngine;
using System.Collections;

public class Seesaw_EnemyPlane.cs : MonoBehaviour {


	srcGameController gController;

	bool direction = false;
	float speed = 0;
	public bool Indestructable = false;


	bool frozen = false;
	Color frozenColor = new Color(0.140f, 0.929f, 1.000f, 1.000f);
	float blinkTimer = 0;


	void Start () {
		gController = GameObject.FindGameObjectWithTag ("GameController").GetComponent<srcGameController>();
		speed = Random.Range (1, 6);
		direction = Random.Range (1, 3) == 2;
		if (direction) this.transform.localScale = new Vector3 (-this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
	}


	bool isFrozen(){
		if (gController.fleaTimeFreeze && !frozen) {			
			frozen = true;
			this.gameObject.GetComponent<Animator> ().speed = 0;
			gameObject.GetComponent<SpriteRenderer> ().color = frozenColor;
			gameObject.GetComponent<PolygonCollider2D> ().enabled = false;
		} 
		else if (frozen) {
			if (!gController.fleaTimeFreeze) {
				frozen = false;
				this.gameObject.GetComponent<Animator> ().speed = 1;
				gameObject.GetComponent<SpriteRenderer> ().color = new Color (1, 1, 1, 1);
				blinkTimer = 0;
				gameObject.GetComponent<PolygonCollider2D> ().enabled = true;
			} 
			else {
				if (gController.timerTimeFreeze < 2) {
					blinkTimer += Time.deltaTime*3;
					if (Mathf.RoundToInt(blinkTimer) % 2==0)
						gameObject.GetComponent<SpriteRenderer> ().color = new Color (1, 1, 1, 1);
					else
						gameObject.GetComponent<SpriteRenderer> ().color = frozenColor;
				}
			}				
		}
		return frozen;
	}

	void Update () {

		if (isFrozen()) return;

		float newX;
		if(direction) 
		{
			newX = this.gameObject.transform.position.x + speed * Time.deltaTime;



			if(newX > gController.screenBounds) 
			{
				newX = gController.screenBounds;
				this.transform.localScale = new Vector3 (-this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
				direction = !direction;
			}

		}
		else 
		{
			
			newX = this.gameObject.transform.position.x - speed * Time.deltaTime;
			if(newX < -gController.screenBounds) 
			{
				newX = -gController.screenBounds;
				this.transform.localScale = new Vector3 (-this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
				direction = !direction;
			}
		}

		this.gameObject.transform.position = new Vector3 (newX,this.gameObject.transform.position.y,this.gameObject.transform.position.z);


	}


	void OnTriggerEnter2D(Collider2D Col) {

		if (Col.tag == "FleaLeft" || Col.tag == "FleaRight") {

			srcFlea fleaSrc = Col.GetComponent<srcFlea> ();
			fleaSrc.Dizzy ();
		}
	}
}
