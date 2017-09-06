using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppodealAds.Unity.Api;
using AppodealAds.Unity.Common;

public class MilkAds : MonoBehaviour,IInterstitialAdListener,IRewardedVideoAdListener {

	public string appKey = "316f2cf960c18a65cb98786bf679177147f2620b7d3b499f";

	#region Interstitial callback handlers
		public void onInterstitialLoaded() { Debug.Log("Interstitial loaded"); }
		public void onInterstitialFailedToLoad() { Debug.Log("Interstitial failed"); }
		public void onInterstitialShown() { Debug.Log("Interstitial opened"); }
		public void onInterstitialClosed() { Debug.Log("Interstitial closed"); }
		public void onInterstitialClicked() { Debug.Log("Interstitial clicked"); }
	#endregion

	#region Rewarded Video callback handlers
		public void onRewardedVideoLoaded() { print("Video loaded"); }
		public void onRewardedVideoFailedToLoad() { print("Video failed"); }
		public void onRewardedVideoShown() { print("Video shown"); }
		public void onRewardedVideoClosed() { print("Video closed"); }
		public void onRewardedVideoFinished(int amount, string name) { 
			print("Reward: " + amount + " " + name); 
			BroadcastMessage("ApplyReward");
		}
	#endregion

	public bool showTopBanner = false;
	public bool showBottomBanner = false;


	// Use this for initialization
	void Start () {
		Appodeal.disableLocationPermissionCheck();
		Appodeal.initialize(appKey, Appodeal.INTERSTITIAL | Appodeal.REWARDED_VIDEO);

		if(showBottomBanner){
			Appodeal.show(Appodeal.BANNER_BOTTOM);
		} else if(showTopBanner){
			Appodeal.show(Appodeal.BANNER_TOP);
		}
	}
	
	void LoadAdvert(){
	}

	void ShowInterstertial(){
		Debug.Log("Attempting show itnerstertial");
		Appodeal.show(Appodeal.INTERSTITIAL);
	}

	public void ShowRewarded(){
		Debug.Log("Attempting show rewarded");
		Appodeal.show(Appodeal.REWARDED_VIDEO);
	}

}
