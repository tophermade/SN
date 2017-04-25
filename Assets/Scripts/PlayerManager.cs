using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

	public GameObject squaresprite;
	public GameObject circleSprite;
	public GameObject lumbergh;

	public float offset = .8f;


	private bool isSquare = true;

	
	public void SwitchSprite(){
		if(squaresprite.activeSelf){
			SetToCircle();
		} else {
			SetToSquare();
		}
	}

	void SetToCircle(){
		squaresprite.SetActive(false);
		circleSprite.SetActive(true);
	}

	public void SetToSquare(){
		circleSprite.SetActive(false);
		squaresprite.SetActive(true);
	}

	
	/// <summary>
	/// Sent when another object enters a trigger collider attached to this
	/// object (2D physics only).
	/// </summary>
	/// <param name="other">The other Collider2D involved in this collision.</param>
	void OnTriggerEnter2D(Collider2D other) {
		lumbergh.GetComponent<Lumbergh>().HitObstacle(other.transform.gameObject);
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
