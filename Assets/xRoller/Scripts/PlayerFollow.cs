using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFollow : MonoBehaviour {

	[SerializeField]
	Transform player;
	public Vector3 cameraReposition = new Vector3(0, 0, 0);
    public bool following = true;
    public float fixedHorizontalFOV = 60;
    
    void Awake(){
        if(GetComponent<Camera>()!=null){
            GetComponent<Camera>().fieldOfView = 2 * Mathf.Atan(Mathf.Tan(fixedHorizontalFOV * Mathf.Deg2Rad * 0.5f) / GetComponent<Camera>().aspect) * Mathf.Rad2Deg;
        }
    }

	// Use this for initialization
	void Start(){
		
	}
	
	// Update is called once per frame
	void FixedUpdate(){
        if(following){
            transform.position = player.position + cameraReposition;
        }
        else{
            // if follower is a camera
            if(GetComponent<Camera>()!=null){
                transform.position = Vector3.Lerp(transform.position, new Vector3(0, player.transform.position.y + 20, player.transform.position.z -100f), Time.deltaTime * 3.0f);
            }
        }
	}
  
    public void StartMoveBack(){
        following = false;
    }
}
