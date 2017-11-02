using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideByPref : MonoBehaviour {

	public string prefName;

	// Use this for initialization
	void Start () {
		if(PlayerPrefs.HasKey(prefName)){
			gameObject.SetActive(false);
		}		
	}
	
}
