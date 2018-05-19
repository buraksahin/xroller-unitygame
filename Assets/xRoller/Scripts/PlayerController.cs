using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    #region Variables
    /*
	 *   Player Variables 
     */
    [SerializeField]
	Rigidbody rigidbodyPlayer;          // Rigidbody of the Player
	[SerializeField]
	private float sideForce = 50f;      // Side Force
    [SerializeField]
    private float jumpForce;            // Jump Force
    private float xForce = 0f;          // Horizontal Force
	private Vector3 keepAcc;            // Keep Acceleration Vector
    private float keepX;                // Keep Acceleration x-axis
    public GameObject jumpPowerEffect;  // Particle Effect
    
    // Sound Variables
    private AudioSource audioFx;        // Audio Source
    public AudioClip jumpingSound;    	// New Record Sound

    #endregion

    // Use this for initialization
    void Start(){
        ///...///
        audioFx = GetComponent<AudioSource>();
	}

	// Update is called once per frame
	void Update(){
        // Check Jump Touch
        GetInput();
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer){
            // Get Acceleration of y-axis
            keepAcc = -Input.acceleration;
            xForce = sideForce * -keepAcc.x * 2;
        }
        else{
            xForce = sideForce * Input.GetAxis ("Horizontal");
        }
    }

    // Fixed Update Frame
	void FixedUpdate(){
		if (xForce != 0){
            // Add Side (x axis) Force to Player Rigidbody
			rigidbodyPlayer.AddForce (xForce, 0, 0, ForceMode.Force);
		}
	}

    #region Helper Functions

    // GetInput
    void GetInput(){
        // Jump Force
        // Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended 
        if((Input.GetKeyDown("space") || Input.GetMouseButtonDown(0)) && Physics.Raycast(transform.position, -Vector3.up, 1)){
            if (GameManager.instance.bonunJumpPowerCount > 0 && GameManager.instance.bonusJumpPower){
                // Decrease JumpPower Bonus Count
                GameManager.instance.DecreaseBonusJumpPower();
                
                // Add Force to Player Rigidbody
                rigidbodyPlayer.AddForce(new Vector3(0, jumpForce * 2, 0), ForceMode.Impulse);

                // Check Jump Power and Disable
                if(GameManager.instance.bonunJumpPowerCount<=0){
                    // Disable Jump Effect
                    jumpPowerEffect.SetActive(false);
                }
            }
            else{
                // Add Force to Player Rigidbody
                rigidbodyPlayer.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
            }
			if(GameManager.instance.gameSoundStatus != 2){
				audioFx.PlayOneShot (jumpingSound, 1.5f);
			}
        }
    }
    #endregion
}
