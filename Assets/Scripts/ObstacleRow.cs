using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleRow : MonoBehaviour {

	

	public GameObject circle;
	public GameObject square;
	public GameObject coin;

	public int pickupLine = 17;
	public int coinValue = 3;

	void SetupObstacle(){
		GameObject makeThisBlock = circle;	

		// select block
		if(Random.Range(0,2) > 0){
			makeThisBlock = square;
		}

		// select placement
		float variance = .8f;
		if(Random.Range(0,2) > 0){
			variance = -.8f;
		}

		// instantiate block
		GameObject newObstacle = Instantiate(makeThisBlock, new Vector3(transform.position.x + variance, transform.position.y, transform.position.z), Quaternion.identity);
		newObstacle.transform.parent = transform;

		// decide if has coin
		if(Random.Range(0,20) > pickupLine){
			GameObject newCoin = Instantiate(coin,  new Vector3(transform.position.x + (-variance), transform.position.y, transform.position.z), Quaternion.identity);
			newCoin.transform.parent = transform;
		}
	}

	public IEnumerator DestroySelf(){
		yield return new WaitForSeconds(1);
	}

	// Use this for initialization
	void Start () {
		SetupObstacle();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
