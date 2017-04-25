using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Coin : MonoBehaviour {

	public int coinValue = 0;

	// Use this for initialization
	void Start () {
		if(coinValue == 0){
			coinValue = Random.Range(1, 6);
		}

		transform.Find("Canvas/Text").gameObject.GetComponent<Text>().text = "+" + coinValue.ToString();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
