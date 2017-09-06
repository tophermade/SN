using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideByPlatform : MonoBehaviour {

	public bool hideOnAndroid = false;
	public bool hideOnIOS = false;
	public bool hideOnDesktop = false;

	// Use this for initialization
	void Start () {
		#if UNITY_ANDROID
		if(hideOnAndroid){
			gameObject.SetActive(false);
		}
		#endif

		#if UNITY_IPHONE
		if(hideOnIOS){
			gameObject.SetActive(false);
		}
		#endif

		#if UNITY_WIN
		if(hideOnDesktop){
			gameObject.SetActive(false);
		}
		#endif
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
