using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonSendOnClick : MonoBehaviour {

	public AudioSource audioPlayer;
	public GameObject messageTo;
	public AudioClip buttonSound;
	public string messageToSend;
	public string messageArgument;

    private Button button;

    void Start() {
		button = GetComponent<Button>();
		button.onClick.AddListener(Action);
    }

    
	void Action(){
		if(messageTo != null){
			PrepareToSend();
		} else {
			Debug.LogError("Your button does no know where to send it's message");
		}
	}


	void PrepareToSend(){
		if(messageToSend != null && messageArgument != null){
			if(audioPlayer != null){
				audioPlayer.PlayOneShot(buttonSound);
			}
			messageTo.SendMessage(messageToSend, messageArgument);
		} else if(messageToSend != null) {
			messageTo.SendMessage(messageToSend);	
			if(audioPlayer != null){
				audioPlayer.PlayOneShot(buttonSound);
			}
		} else {
			Debug.LogError("Your button message and/or argument are not correctly configured");
		}
	}
}
