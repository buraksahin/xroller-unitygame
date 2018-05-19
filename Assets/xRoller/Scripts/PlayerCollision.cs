using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PlayerCollision : MonoBehaviour {

    #region Variables

    // Particle Effects
    [SerializeField]
    public ParticleSystem smashEffect;          // Player Crash Effect
    public ParticleSystem obstracleSmashEffect; // Obstacle Crash Effect
    public GameObject ShieldEffect;             // Particle Effect
    public GameObject JumpPowerEffect;          // Particle Effect

    // Player Variables
    public MeshRenderer meshPlayer;             // Player Mesh
    public Camera playerCam;                    // Player Cam

    // Game Manager Bonus Variables
    private int addBonusPoint = 0;              // Bonus Point
    private int addBonusShield = 0;             // Shield Bonus Count
    private int addBonusJumpPower = 0;          // JumpPower Bonus Count
    private int addBonusTime = 0;               // Time Bonus Count

    // Sound Variables
    private AudioSource audioFx;                // Audio Source
    public AudioClip[] soundFx;                 // Sound Effects

    // User Interface Information
    public Text bonusInfoText;              // Bonus Information UI Text Game Object
    private float lastTimeShowBonus;      // Last Time Show Bonus Keep Cleaning Time

    #endregion

    #region Functions

    // Start Function Set Variables
    void Start(){
        bonusInfoText.text = "";
        audioFx = GetComponent<AudioSource>();
    }

    void Update(){
        // Clean Bonus Info Text
        if(Time.time - lastTimeShowBonus > 3){
            bonusInfoText.text = "";
        }
    }
    
    // Destroy Player like collided
    public void EnterSyntheticCollision(){
        // Play Sound Fx
        PlayPlayerCrashFx();

        // Set Game Status as Stop Game
        GameManager.instance.StopGame();

        // Disable Player Components
        transform.GetComponent<Collider>().enabled = false;
        transform.GetComponent<ConstantForce>().enabled = false;
        transform.GetComponent<Rigidbody>().useGravity = false;
        Destroy(transform.GetComponent<ConstantForce>());
        Destroy(transform.GetComponent<Rigidbody>());
		transform.GetComponent<PlayerController>().enabled = false;

        // Move Player Cam
        playerCam.GetComponent<PlayerFollow>().StartMoveBack();
        
        // Smash Effect Get Current Ball Material
        smashEffect.GetComponent<ParticleSystemRenderer>().material = transform.GetComponent<MeshRenderer>().material;
        
        // Disable Shield Effect
        ShieldEffect.SetActive(false);

        // Disable Jump Power Effect
        JumpPowerEffect.SetActive(false);

        // Disable player Mesh Render for hide player
        meshPlayer.enabled = false;
        
        // Instantiate and Destroy SmashEffect
        Destroy(Instantiate(smashEffect.gameObject, gameObject.transform.position , Quaternion.FromToRotation(Vector3.forward, Vector3.back)) as GameObject, smashEffect.startLifetime);

        // Check High Score
        GameManager.instance.CheckHighScore();

        // Play Game Over Sound after 1 second
        WaitSound(1f);
        PlayGameOverFx();
    }
    #endregion

    #region Collision Detection

    /*
     *  Collision Detection
     */
    void OnCollisionEnter(Collision collisionInfo){
        // Check Obstacle tag
		if(collisionInfo.collider.tag == "Obstacle" || collisionInfo.collider.tag == "RoadObstacle"){
            // Without Shield Obstacle Smash
			if(GameManager.instance.bonusShield<=0){
                // Play Sound Fx
                PlayPlayerCrashFx();

                // Set Game Status as Stop Game
                GameManager.instance.StopGame();

                // Disable Player Components
                transform.GetComponent<Collider>().enabled = false;
                transform.GetComponent<ConstantForce>().enabled = false;
                transform.GetComponent<Rigidbody>().useGravity = false;
                Destroy(transform.GetComponent<ConstantForce>());
                Destroy(transform.GetComponent<Rigidbody>());
			    transform.GetComponent<PlayerController>().enabled = false;
                
                // Move Player Cam
                playerCam.GetComponent<PlayerFollow>().StartMoveBack();
                
                // Smash Effect Get Current Ball Material
                smashEffect.GetComponent<ParticleSystemRenderer>().material = transform.GetComponent<MeshRenderer>().material;
                
                // Disable Shield Effect
                ShieldEffect.SetActive(false);

                // Disable Jump Power Effect
                JumpPowerEffect.SetActive(false);

                // Disable player Mesh Render for hide player
                meshPlayer.enabled = false;
                
                // Instantiate and Destroy SmashEffect
                Destroy(Instantiate(smashEffect.gameObject, gameObject.transform.position , Quaternion.FromToRotation(Vector3.forward, Vector3.back)) as GameObject, smashEffect.startLifetime);
                
                // Check High Score
                GameManager.instance.CheckHighScore();

                // Play Game Over Sound after 1 second
                WaitSound(1f);
                PlayGameOverFx();
            }
            // With Shield Bonus Obstacle Smash
            else{
                // Enter Player Crash
                if(collisionInfo.collider.tag == "RoadObstacle"){
                    EnterSyntheticCollision();
                }
                
                // Destroy Obstacle
                if(collisionInfo.collider.tag == "Obstacle") {
                    // Play Sound Fx
                    PlayObstacleCrashFx();
                    obstracleSmashEffect.GetComponent<ParticleSystemRenderer>().material = collisionInfo.transform.GetComponent<MeshRenderer>().material;
                
                    Destroy(Instantiate(obstracleSmashEffect.gameObject, collisionInfo.transform.position , Quaternion.FromToRotation(Vector3.forward, Vector3.back)) as GameObject, obstracleSmashEffect.startLifetime);
                    Destroy(collisionInfo.gameObject);
                }
                GameManager.instance.DecreaseBonusShield();
                
                // Check Ball Materials as Bonus Count
                if(GameManager.instance.bonusShield == 1){
                 // Change Mat
                }
                else if(GameManager.instance.bonusShield == 2){
                // Change Mat
                }
                else if(GameManager.instance.bonusShield == 3){
                // Change Mat
                }
                else if(GameManager.instance.bonusShield > 3){
                // Change Mat
                }
                else{
                    // Disable Shield Particle Effect
                    ShieldEffect.SetActive(false);
                }
            }
		}// End of the Obstacle tag collision

        // Check JumpPower Bonus tag
        if(collisionInfo.collider.tag == "JumpPower"){

            // Play Bonus Effect Sound
            PlayBonusFx();

            // Enable or Disable Sound Source as GameManager
            if(GameManager.instance.gameSoundStatus == 2){
                JumpPowerEffect.GetComponent<AudioSource>().enabled = false;
            }else{
                JumpPowerEffect.GetComponent<AudioSource>().enabled = true;
            }

            // Enable Shield Particle Effect
            JumpPowerEffect.SetActive(true);

            // Enable Bonus From Game Manager
            GameManager.instance.SetBonusJumpPower(enabled);

            // Increase Bonus Count From Game Manager
            addBonusJumpPower = Random.Range(1,4);
            GameManager.instance.IncreaseBonusJumpPower(addBonusJumpPower);

            bonusInfoText.text = "+" + addBonusJumpPower + " Jump Bonus";
            lastTimeShowBonus = Time.time;
		}

        // Check Shield Bonus tag
        if(collisionInfo.collider.tag == "Shield"){

            // Play Bonus Effect Sound
            PlayBonusFx();
            
            // Enable or Disable Sound Source as GameManager
            if(GameManager.instance.gameSoundStatus == 2){
                ShieldEffect.GetComponent<AudioSource>().enabled = false;
            }else{
                ShieldEffect.GetComponent<AudioSource>().enabled = true;
            }
            
            // Enable Shield Particle Effect
            ShieldEffect.SetActive(true);

            addBonusShield = Random.Range(1,4);
            GameManager.instance.IncreaseBonusShield(addBonusShield);

            bonusInfoText.text = "+" + addBonusShield + " Shield Bonus";
            lastTimeShowBonus = Time.time;
		}

        // Check Extra Point Bonus tag
        if(collisionInfo.collider.tag == "ExtraPoint"){
            // Play Bonus Effect Sound
            PlayBonusFx();
            
            // Increase Bonus Count From Game Manager
            addBonusPoint = Random.Range(1, 26);
            GameManager.instance.IncreaseScoreCounter(addBonusPoint);

            bonusInfoText.text = "+" + addBonusPoint + " Extra Point";
            lastTimeShowBonus = Time.time;
		}

        // Check Extra Point Bonus tag
        if(collisionInfo.collider.tag == "ExtraTime"){
            // Play Bonus Effect Sound
            PlayBonusFx();

            // Increase Bonus Count From Game Manager
            addBonusTime = Random.Range(1, 26);
            GameManager.instance.IncreaseGameRemainingTime(addBonusTime);

            bonusInfoText.text = "+" + addBonusTime + " Extra Time";
            lastTimeShowBonus = Time.time;
		}
    }

    // Exit Collision
    void OnCollisionExit(Collision collisionInfo){
    }

    #endregion

    #region Sound Fx

    /*
     *   Sound FX 
     */
    // Bonus Sound
    void PlayBonusFx(){
        if(GameManager.instance.gameSoundStatus!=2){
            audioFx.PlayOneShot(soundFx[1], 1.0f);
        }
    }

    // Player Crash Sound
    void PlayPlayerCrashFx(){
        if(GameManager.instance.gameSoundStatus!=2){
            audioFx.PlayOneShot(soundFx[0], 0.8f);
        }
    }

    // Obstacle Crash Sound
    void PlayObstacleCrashFx(){
        if(GameManager.instance.gameSoundStatus!=2){
            audioFx.PlayOneShot(soundFx[0], 0.7f);
        }
    }

    // Game Over Sound
    void PlayGameOverFx(){
        if(GameManager.instance.gameSoundStatus!=2){
            audioFx.PlayOneShot(soundFx[2], 0.9f);
        }
    }
    #endregion

    #region Helper Functions

    IEnumerable WaitSound(float _waitTime){
        yield return new WaitForSeconds(_waitTime);
    }
    #endregion
}
