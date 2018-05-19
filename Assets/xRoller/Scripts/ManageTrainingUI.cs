using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManageTrainingUI : MonoBehaviour {

	public GameObject uiTrainingInfo;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(GameManager.instance.isGameStop()){
            uiTrainingInfo.SetActive (false);
        }
	}

	void OnCollisionEnter(Collision collisionInfo){
		// Check Obstacle tag
		if (collisionInfo.collider.tag == "Player") {
			uiTrainingInfo.SetActive (true);
		}
	}

	void OnCollisionExit(Collision collisionInfo){
		// Check Obstacle tag
		if(collisionInfo.collider.tag == "Player"){
			uiTrainingInfo.SetActive (false);
		}
	}
}
