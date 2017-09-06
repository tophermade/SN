using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;

public class Lumbergh : MonoBehaviour {
	// we're not using the built in unity platform checking / compiler flags
	// because there is a fair amount of overlap. still use compile flags
	// for libraries etc
	public enum Platform{
		desktop, gameRoom, itch, newgrounds, kongregate, gamejolt, ios, android, kindle
	}


	public Platform platform;
	
	public GameObject player;
	public GameObject cam;
	public GameObject gameCanvas;
	public GameObject[] splatters;
	public AudioClip[] swordSounds;
	public AudioClip[] splatterSounds;
	public AudioClip pickupSound;
	public AudioClip flipSound;
	public AudioClip buttonSound;
	private Rigidbody2D body;
	private GameObject playHud;
	private GameObject useCreditScreen;
	private GameObject gameOverScreen;
	private GameObject mainScreen;
	private PlayerManager playerManager;
	private GameObject playerCircle;
	private GameObject playerSquare;
	private AudioSource audioPlayer;
	public GameObject row;
	private GameObject trail;
	public GameObject spawnedParent;
	public GameObject coinDisplay;
	public GameObject scoreDisplay;
	public GameObject scoreDisplayGameOver;
	public GameObject coinEffect;
	public GameObject bestDisplay;
	


	public float speed = 3.5f;
	public float baseSpeed = 3.5f;
	public float maxSpeed = 9.5f;
	public float speedBoost = 0.0f;
	public float speedBoostDelay = 1.0f;
	private float lastSpeedBoostAt;


	public int coinsOwned = 0;
	public int continueCost = 50;

	public float lastRowAt = 0.0f;
	public float minRowGap = 2.5f;
	public float maxRowGap = 5.5f;
	public int roundScore = 0;
	public int roundCount = 0;


	public bool isPlaying = false;
	public bool isPaused = false;
	public bool isPlayerSquare = true;


	void SetupScene(){
		playerManager = player.GetComponent<PlayerManager>();
		playerManager.lumbergh = gameObject;
		body = player.GetComponent<Rigidbody2D>();
		playerSquare = player.transform.Find("Square").gameObject;
		playerCircle = player.transform.Find("Circle").gameObject;
		audioPlayer = gameObject.GetComponent<AudioSource>();
		trail = player.transform.Find("Trail").gameObject;
		playHud = gameCanvas.transform.Find("PlayHud").gameObject;
		useCreditScreen = gameCanvas.transform.Find("UseCreditScreen").gameObject;
		gameOverScreen = gameCanvas.transform.Find("GameOverScreen").gameObject;
		mainScreen = gameCanvas.transform.Find("MainScreen").gameObject;
		mainScreen.SetActive(true);
		trail.transform.position = new Vector3(.22f, trail.transform.position.y, 0);
		coinsOwned = PlayerPrefs.GetInt("CoinsOwned");
		UpdateOwnedCoins();
		UpdateScoreDisplay();
	}


	IEnumerator EnableWithDelay(GameObject enableThis){
		yield return new WaitForSeconds(.75f);
		enableThis.SetActive(true);
	}

	IEnumerator DisableWithDelay(GameObject disableThis, float timeToDelay){
		yield return new WaitForSeconds(timeToDelay);
		disableThis.GetComponent<Animator>().SetTrigger("PlayOut");
		disableThis.SetActive(false);
	}


	void StartFirstRound(){
		player.SetActive(true);
		gameOverScreen.SetActive(false);
		useCreditScreen.SetActive(false);
		mainScreen.GetComponent<Animator>().SetTrigger("PlayOut");

		StartRound();
	}


	void SetupNewRound(){
		DestroyLastRoundObjects();
		isPaused = false;
		isPlaying = true;
		StartCoroutine(DisableWithDelay(gameOverScreen, .5f));
		useCreditScreen.SetActive(false);
		mainScreen.GetComponent<Animator>().SetTrigger("PlayInAndOut");
		playerSquare.GetComponent<SpriteRenderer>().flipY = false;
		playerCircle.GetComponent<SpriteRenderer>().flipY = false;
		lastRowAt = 0;
		speed = baseSpeed;
		speedBoost = 0.0f;
		roundScore = 0;		
		UpdateOwnedCoins();
		UpdateScoreDisplay();

		StartRound();
	}




	void StartRound(){
		isPlaying = true;
		trail.SetActive(true);
		player.transform.position = new Vector3(.6f, 0,0);
		playerManager.SetToSquare();
		isPlayerSquare = true;
		roundCount++;
	}


	void DestroyLastRoundObjects(){
		foreach (Transform child in spawnedParent.transform) {
            Destroy(child.gameObject);
        }
	}


	public void HitObstacle(GameObject other){
		if(other.tag == "Obstacle"){
			if((isPlayerSquare && other.transform.name == "SquareSprite(Clone)") || (!isPlayerSquare && other.transform.name != "SquareSprite(Clone)")){
				audioPlayer.PlayOneShot(swordSounds[Random.Range(0, swordSounds.Length)]);
				DestroyBlock(other.transform.gameObject);
				SwitchPlayerShape();
				roundScore++;
				UpdateScoreDisplay();
			} else {
				GameObject newSplatter = Instantiate(splatters[Random.Range(0, splatters.Length)], new Vector3(0, player.transform.position.y,0), Quaternion.identity);
				newSplatter.transform.parent = spawnedParent.transform;
				audioPlayer.PlayOneShot(splatterSounds[Random.Range(0, splatterSounds.Length)]);
				cam.GetComponent<CamShaker>().DoShake();
				if(coinsOwned >= continueCost){
					PromptForContinue();
				} else {
					KillPlayer();
				}
			}
		} else if(other.tag == "Pickup"){
			audioPlayer.PlayOneShot(pickupSound);
			GameObject newEffect = Instantiate(coinEffect, other.transform.position, Quaternion.identity);
			newEffect.transform.parent = spawnedParent.transform;
			Destroy(other);
			AcquireCoins(other.GetComponent<Coin>().coinValue);
		}
	}


	void UpdateScoreDisplay(){
		scoreDisplay.GetComponent<Text>().text = roundScore.ToString();
		scoreDisplayGameOver.GetComponent<Text>().text = "Score: " + roundScore.ToString();
	}


	void SwitchPlayerShape(){
		if(Random.Range(0, 20) > 12){
			playerManager.SwitchSprite();
			isPlayerSquare = !isPlayerSquare;
		}
	}


	void DestroyBlock(GameObject block){
		Destroy(block.GetComponent<Rigidbody2D>());
		StartCoroutine(block.transform.parent.GetComponent<ObstacleRow>().DestroySelf());
		block.transform.Find("Sprite").GetComponent<Animator>().SetTrigger("PlayPop");
	}


	void AcquireCoins(int amount){
		coinsOwned = coinsOwned + amount;
		UpdateOwnedCoins();
	}


	void UpdateOwnedCoins(){
		PlayerPrefs.SetInt("CoinsOwned", coinsOwned);
		coinDisplay.GetComponent<Text>().text = coinsOwned.ToString();
	}


	void PromptForContinue(){
		isPaused = true;
		StartCoroutine(EnableWithDelay(useCreditScreen));
	}


	void ContinuePlay(){
		coinsOwned = coinsOwned - continueCost;
		UpdateOwnedCoins();
		isPlaying = true;
		isPaused = false;
		useCreditScreen.SetActive(false);
	}


	void KillPlayer(){
		SetupBestScore();

		isPlaying = false;
		isPaused = false;
		trail.SetActive(false);
		playerCircle.SetActive(false);
		playerSquare.SetActive(false);
		useCreditScreen.SetActive(false);
		StartCoroutine(ShowGameOver());

		if(roundCount == 5){
			roundCount = 0;
			BroadcastMessage("ShowInterstertial");
		}
	}


	void SetupBestScore(){
		if(roundScore > PlayerPrefs.GetInt("Best")){
			PlayerPrefs.SetInt("Best", roundScore);
			bestDisplay.GetComponent<Text>().text = "Best: " + roundScore.ToString();
		} else{
			bestDisplay.GetComponent<Text>().text = "Best: " + PlayerPrefs.GetInt("Best").ToString();
		}
	}


	IEnumerator ShowGameOver(){
		yield return new WaitForSeconds(1);
		gameOverScreen.SetActive(true);
		gameOverScreen.GetComponent<Animator>().SetTrigger("PlayIn");
		mainScreen.GetComponent<Animator>().SetTrigger("PlayOut");
	}


	void UpdateBoost(){		
		if(isPlaying && !isPaused && speedBoost < maxSpeed - speed){
			if(lastSpeedBoostAt + speedBoostDelay < Time.time){
				if(speedBoost == 0){
					speedBoost = 1.0f;
				} else {
					speedBoost = speedBoost * 1.1f;
				}
				lastSpeedBoostAt = Time.time;
			}
		}
	}


	void MovePlayer(){
		if(isPlaying && !isPaused){
			body.drag = 0;
			if(body.velocity.y < speed + speedBoost){
				body.AddForce(player.transform.up * (body.velocity.y + 0.35f));
			}
		} 
		else {
			if(body.velocity.y > 0){
				body.drag = 9.0f;
			}
		}
	}


	void SwitchPlayerSide(){
		audioPlayer.PlayOneShot(flipSound);
		player.transform.position = new Vector3(-player.transform.position.x,player.transform.position.y,0);
		playerSquare.GetComponent<SpriteRenderer>().flipY = !playerSquare.GetComponent<SpriteRenderer>().flipY;
		playerCircle.GetComponent<SpriteRenderer>().flipY = !playerCircle.GetComponent<SpriteRenderer>().flipY;

		if(player.transform.position.x > 0){
			trail.transform.position = new Vector3(.22f, trail.transform.position.y, 0);
		} else {
			trail.transform.position = new Vector3(-.22f, trail.transform.position.y, 0);
		}
	}

	void MoveCamera(){
		cam.transform.position = new Vector3(0, player.transform.position.y, -10.0f);
		//var velocity = Vector3.zero;
		//cam.transform.position = Vector3.SmoothDamp(transform.position, new Vector3(0, player.transform.position.y, -10), ref velocity, 0.5f);
	}


	void ManageRows(){
		if(player.transform.position.y > lastRowAt-10){
			lastRowAt = lastRowAt + Random.Range(minRowGap, maxRowGap);
			GameObject newRow = Instantiate(row, new Vector3(0, lastRowAt, 0), Quaternion.identity);
			newRow.transform.parent = spawnedParent.transform;
		}
	}


	void InitiateShare(){
		if(Application.platform == RuntimePlatform.WindowsPlayer){
			
		}
	}


	void Start () {
		SetupScene();
	}


	void Update(){
		UpdateBoost();
		MoveCamera();
		ManageRows();

		if(isPlaying && !isPaused && Input.GetMouseButtonDown(0)){
			SwitchPlayerSide();
		}
	}
	

	void FixedUpdate () {
		MovePlayer();
	}


	public void QuitApp(){
		Application.Quit();
	}
}
