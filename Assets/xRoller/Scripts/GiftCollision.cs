using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiftCollision : MonoBehaviour {

    [SerializeField]
    private ParticleSystem giftParticals;
	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collisionInfo){
        if(collisionInfo.collider.tag == "Player"){
            transform.GetComponent<BoxCollider>().enabled = false;
            transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            transform.GetChild(1).GetComponent<MeshRenderer>().enabled = false;
            giftParticals.Play();
        }
    }
}
