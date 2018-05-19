using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class GameManager : MonoBehaviour {

    #region Game Variables
    /*
     *   Game Variables 
     */
    // Create static GameManger instance
    public static GameManager instance = null;      // Game Manager
	public delegate void OnStateChangeHandler();    // Game State Handler

    /**
     * GAMESTATUS ENUM
     * 
     * notready: game is not ready to play
     * ready: All settings(ways, obstacles, camera, player etc.) has been done and ready to play
     * running: game is start and running
     * stopped: player destroyed
     */
    public enum GAMESTATUS{notready, ready, running, stopped}   // Game States
	public GAMESTATUS status { get; private set; }              // Game Status Setter and Getter
	public Transform playerSpawn;                               // Player Spawn Point
	public int waitTime = 3;                                    // Start Line Timer Value
    public int gameRemainTime = 60;                             // Game Remain Time (Count Back From gameRemainTime)
    public int gameSoundStatus = 0;                             // Game Sound Status 0: Effects and Music Active - 1: Only Effects - 2: Effects and Music Active - 3: Mute
    public AudioSource audioPlayerSource;                       // AudioSource
    public GameObject MenuCam;                                  // Menu Cam

    /*
     *   Player Varialbes 
     */
    public GameObject Player;       		// Player Game Prefab
    private GameObject gamePlayer;  		// Game Player Object
	public bool isPlayerMoving = false;		// Check Player Movement

    /*
     * Way Variables
     */
    public GameObject startWay;         	// Startway will be free (without any obstacles or bonus)
	public GameObject[] wayTypes;       	// Way Types
	public GameObject[] trainingWayTypes;	// Training Way Types
	public Vector3 wayMovePosition = new Vector3(0f, 0f, 100f); // it will increase with current level
	public float destroyLine = 0f;      	// Destroy Border Value If any way pass away will be destroyed
    private int preparedWayValue = 9;   	// Total Prepare Way Value
    public GameObject[] prepWay;        	// Prepared and instantiated Ways For Scene
    private const float WAYSPEED = -100f; 	// Way Speed
	private int keepRandom;					// Keep Random Number for Way Generation

    /*
     *   Score Variables
     */
    public int highScore;           // High Score
    public int highLevel;           // High Level
    public int currentScore = 0;    // Current game score
    public int currentLevel = 1;   	// Current game level
    private int scoreCounter = 0;   // Score Counter for calculate player score

    /*
     *   Bonus Variables
     */
    public int bonusShield = 0;       	// Shield Bonus Count
    public bool bonusJumpPower = false; // JumpPower Bonus Boolean (Enable or Disable)
    public int bonunJumpPowerCount = 0; // JumpPower Bonus Count

    /*
     *  User Interface 
     */
    public Text uiTime;                 // Time
    public Text uiStartLineCounter;     // Start Counter
    public Text uiJumpBonus;            // Jump Bonus Text
    public Text uiShieldBonus;          // Shield Bonus Text
    public GameObject uiPlayerGameUI;  	// Player Game Play UI
    public GameObject uiMainMenu;       // Main Menu Canvas
    public GameObject uiGameOverMenu;   // Game Over Menu Canvas
    public GameObject uiHighScoreMenu;  // High Score Menu Canvas
    public Sprite[] uiSoundSprites;     // Sound Mute Sprite
    private int previousMenu = 0;       // Keep Previous Menu for High Score Menu 0: Main Menu 1: Game Over Menu

    /*
     *  Audio Sound Clips
     */
    public GameObject gameMusic;        // Game Music
    public AudioClip newRecordSound;    // New Record Sound
    public AudioClip buttonClickSound;  // Button Click Sound
	public GameObject ambianceSound;	// Ambiance Sound

    #endregion

    #region Main Functions
    void Awake(){
        //  Check if instance already exists
         if (instance == null){
             // if not, set instance to this
             instance = this;
        }
         else if (instance != this){
            //  Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager
            Destroy(gameObject);    
        }
        // Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);
    }

	// Use this for initialization
	void Start(){
        SecurePlayerPrefs.Init();
        //ResetHighScores(); // Reset High Scores !!! REMOVE THIS LINE FOR PUBLISH VERSION !!! //
        // Set Audio Source
        audioPlayerSource = GetComponent<AudioSource>();
        //  Load Prefs
        LoadPrefs();
        // Set Game Music
        SetGameMusic();
        // Set Button Listeners
        SetButtonListeners();
        //  Set Game Status as notready
		SetGameState(GAMESTATUS.notready);
	}
	
    // Update Frame
    void Update(){
        // If key input is P so Replay Game
        if(Input.GetKeyDown("p")) {
            Prepare();
        }
    }

    // Update Frame
	void FixedUpdate(){
		if(status == GAMESTATUS.running && gameRemainTime > 0){
            MoveWays(); // Call move ways
            HandleScores(); // Score Handler
            CheckPlayer();
            UpdateUI();
        }
        if(status == GAMESTATUS.running && gameRemainTime == 0){
            KillPlayer();
            UpdateUI();
        }
	}
    #endregion

    #region Game Flow Functions
    // Start Game
    void StartGame(){
        /*
         *  Start Game and call StartCoroutine for player
         *  This work as game start line counter
         */ 
		StartCoroutine(CountTimer(waitTime));
    }

    // Stop Game
    public void StopGame(){
        /*
        *  Stop Game and set status as stopped
        *  Stop the way movement and other works
        *  Wait for replay action
        */ 
        StopWorks();
        SetGameState(GAMESTATUS.stopped);
    }
    
    // Set all settings when gamestate is Stopped
    public void StopWorks(){
        // Game Stop Works
		isPlayerMoving = false;
        uiPlayerGameUI.SetActive(false);
        uiGameOverMenu.SetActive(true);
        uiGameOverMenu.transform.GetChild(1).transform.GetComponent<Text>().text = "Level " + currentLevel + "    Point " + currentScore;
    }

	// Set Run Settings
	public void RunWorks(){
		// Set Player Movement True
		isPlayerMoving = true;
	}

    // Set Game State
	public void SetGameState(GAMESTATUS state){
        if(state==GAMESTATUS.stopped){
            // Call Stop Works
            StopWorks();
        }

		if (state == GAMESTATUS.running){
			RunWorks();
		}
		this.status = state;
	}


	// Prepare Scene Ways and Game Objects
	void Prepare(){
        // Play Click Sound
        PlayClickSound();

		// Set Ambiance Sound
		if(gameSoundStatus!=2){
			ambianceSound.SetActive (true);
		}

        // Disable Game Over Menu
        uiGameOverMenu.SetActive(false);

        // Disable High Score Menu
        uiHighScoreMenu.SetActive(false);

        // Disable Cam
        MenuCam.SetActive(false);

        // Disable Menu
        uiMainMenu.SetActive(false);

        // Game Settings
        gameRemainTime = 60;

        // Way Speed
        wayMovePosition.z = WAYSPEED;

        // Get High Score Prefs
        currentLevel = 1;
        currentScore = 0;
        scoreCounter = 0;

        // Check Player is exist in the scene if there is a player then destroy player and all ways
        if (gamePlayer != null || status == GAMESTATUS.stopped){
            Destroy(gamePlayer);
            DestroyAllWays();
        }

        // Prepare Start Point Way
		prepWay = new GameObject[preparedWayValue];
		prepWay[0] = (GameObject) Instantiate(startWay, Vector3.zero, Quaternion.Euler(Vector3.zero));
		prepWay[0].transform.name = "StartPoint";
		// Prepare Ways
		if (SecurePlayerPrefs.GetInt ("HIGH_SCORE") < 40 && SecurePlayerPrefs.GetInt ("HIGH_LEVEL") < 2) {
			// Set Training Way
			for(int i=1; i<trainingWayTypes.Length; i++){
				prepWay[i] = (GameObject) Instantiate(trainingWayTypes[(i-1)%5], 
					new Vector3(0f, 0f, prepWay[i-1].transform.position.z
						+ (prepWay[i-1].transform.GetChild(0).transform.localScale.z + trainingWayTypes[(i-1)%5].transform.GetChild(0).transform.localScale.z) /2),
					Quaternion.Euler(Vector3.zero));
				prepWay[i].transform.name = "Training" + i;
			}

            for(int z=trainingWayTypes.Length; z<preparedWayValue; z++){
				keepRandom = Random.Range (0, wayTypes.Length);
				prepWay[z] = (GameObject) Instantiate(wayTypes[keepRandom],
					new Vector3(-1000f, -1000f, -1000f),
					Quaternion.Euler(Vector3.zero));
                prepWay[z].transform.localPosition = new Vector3(0f, 0f, prepWay[z-1].transform.localPosition.z
                    + (prepWay[z-1].transform.GetChild(0).transform.localScale.z
                    + prepWay[z].transform.GetChild(0).transform.localScale.z) / 2);
				prepWay[z].transform.name = "Way" + z;
			}

		}
		else {
			// if Score bigger than 50 Else Set as Training Way
			for(int i=1; i<preparedWayValue; i++){
				keepRandom = Random.Range (0, wayTypes.Length);
				prepWay[i] = (GameObject) Instantiate(wayTypes[keepRandom],
					new Vector3(-1000f, -1000f, -1000f),
					Quaternion.Euler(Vector3.zero));
                prepWay[i].transform.localPosition = new Vector3(0f, 0f, prepWay[i-1].transform.localPosition.z
                    + (prepWay[i-1].transform.GetChild(0).transform.localScale.z
                    + prepWay[i].transform.GetChild(0).transform.localScale.z) / 2);
				prepWay[i].transform.name = "Way" + i;
			}
		}


		// Instantiate Player
		gamePlayer = (GameObject)Instantiate(Player, playerSpawn.transform);
        gamePlayer.transform.GetChild(0).transform.GetComponent<ConstantForce>().enabled = false;
        gamePlayer.transform.GetChild(0).transform.GetComponent<PlayerController>().enabled = false;

        // Set Bonus
        bonusShield = 0;
        bonusJumpPower = false;
        bonunJumpPowerCount = 0;
        
        // Set UI
        uiPlayerGameUI.SetActive(true);
        uiTime.text = "60";
        uiJumpBonus.text = "0";
        uiShieldBonus.text = "0";
        uiStartLineCounter.text = "3";

		// Set Player Movement State
		isPlayerMoving = false;
        // Set status as ready
        SetGameState(GAMESTATUS.ready);
        StartGame();
	}// end of the prepare
    #endregion

    #region Way and Way Control Functions
    // Move Ways
    void MoveWays(){
        // Iteration for checking way transform positions and destroy or instantiate a new way
        for(int i=0; i<preparedWayValue; i++){
            // If way passed away from player - 150 (z axis) then Destroy
            if(prepWay[i].transform.position.z <= destroyLine || gamePlayer.transform.GetChild(0).position.z - 200 > prepWay[i].transform.position.z ){
                Destroy(prepWay[i].gameObject);
                // Instantiate new way for 0 index of prepWay
                if(i==0){
                    // Set Random Number
					keepRandom = Random.Range (0, wayTypes.Length);
					// Instantiate Way
					prepWay[i] = (GameObject) Instantiate(wayTypes[keepRandom], new Vector3(0f, 0f, prepWay[preparedWayValue-1].transform.position.z
                        + prepWay[preparedWayValue-1].transform.GetChild(0).transform.localScale.z),
                        Quaternion.Euler(Vector3.zero));
                    // Set Way Name
					prepWay[i].transform.name = "Way" + i;
                }

                // Instantiate new ways
                if(i>0){
                    // Set Random Number
					keepRandom = Random.Range (0, wayTypes.Length);
					// Instantiate Way
					prepWay[i] = (GameObject) Instantiate(wayTypes[keepRandom], new Vector3(0f, 0f, prepWay[i-1].transform.position.z
                        + prepWay[i-1].transform.GetChild(0).transform.localScale.z),
                        Quaternion.Euler(Vector3.zero));
                    // Set Way Name
					prepWay[i].transform.name = "Way" + i;
                }
            }
        }

        // Move Ways
		for (int m = 0; m < preparedWayValue; m++) {
			prepWay[m].transform.position += wayMovePosition * Time.fixedDeltaTime;
		}
    }// End of the MoveWays

    // Destroy All Ways
    void DestroyAllWays(){
        for(int i=0; i<preparedWayValue; i++){
             Destroy(prepWay[i].gameObject);
        }
    }
    #endregion

    #region Player Control Functions

    // Check Player Position
    public void CheckPlayer(){
        /*
         *  Check player if falling down stop the game
         *  and prepare new game or wait for replay action
         */
        if(gamePlayer.transform.GetChild(0).transform.position.y < -1.5f){
            KillPlayer();
        }
    }

    // Kill Player
    public void KillPlayer(){
        gamePlayer.transform.GetChild(0).transform.GetComponent<PlayerCollision>().EnterSyntheticCollision();
    }
    #endregion

    #region Helper Functions for Bonus

    // Increase GameRemainingTime
    public void IncreaseGameRemainingTime(int _gameRemainTime){
        gameRemainTime = gameRemainTime + _gameRemainTime;
    }
    // Increase Shield Bonus
    public void IncreaseBonusShield(int _ShieldBonus){
        bonusShield = bonusShield + _ShieldBonus;
    }

    // Decrease Shield Bonus
    public void DecreaseBonusShield(){
        bonusShield--;
    }

    // Increase JumpPower Bonus
    public void IncreaseBonusJumpPower(int _JumpPowerCount){
        bonunJumpPowerCount = bonunJumpPowerCount + _JumpPowerCount;
    }

    // Decrease JumpPower Bonus
    public void DecreaseBonusJumpPower(){
        bonunJumpPowerCount = bonunJumpPowerCount - 1;
        if(bonunJumpPowerCount == 0){
            SetBonusJumpPower(false);
        }
    }

    // Set JumpPower Bonus Status
    public void SetBonusJumpPower(bool status){
        bonusJumpPower = status;
    }

    // Increase Score Counter
    public void IncreaseScoreCounter(int _value){
        currentScore = currentScore + _value; // Add value to score counter
    }
    #endregion

    #region User Interface Functions
    /*
     *  Set Button Listeners
     */
    void SetButtonListeners(){
        // Main Menu
        uiMainMenu.transform.GetChild(1).transform.GetComponent<Button>().onClick.AddListener(() => Prepare());
        uiMainMenu.transform.GetChild(2).transform.GetComponent<Button>().onClick.AddListener(() => ShowHighScoreMenu(0));
        uiMainMenu.transform.GetChild(3).transform.GetComponent<Button>().onClick.AddListener(() => ChangeSoundStatus());

        // Game Over Menu
        uiGameOverMenu.transform.GetChild(2).transform.GetComponent<Button>().onClick.AddListener(() => Prepare());
        uiGameOverMenu.transform.GetChild(3).transform.GetComponent<Button>().onClick.AddListener(() => ShowHighScoreMenu(1));
        uiGameOverMenu.transform.GetChild(4).transform.GetComponent<Button>().onClick.AddListener(() => ChangeSoundStatus());

        // High Score Menu
        uiHighScoreMenu.transform.GetChild(2).transform.GetComponent<Button>().onClick.AddListener(() => HideHighScoreMenu());
    }

    // int Last Menu -->
    void ShowHighScoreMenu(int _previousMenu){
        // Play Click Sound
        PlayClickSound();

        if(_previousMenu==0){
            previousMenu = 0;
            uiMainMenu.SetActive(false);
        }
        if(_previousMenu==1){
            previousMenu = 1;
            uiGameOverMenu.SetActive(false);
        }
        uiHighScoreMenu.transform.GetChild(1).transform.GetComponent<Text>().text = "Level " + SecurePlayerPrefs.GetInt("HIGH_LEVEL") + "    Point " + SecurePlayerPrefs.GetInt("HIGH_SCORE");
        uiHighScoreMenu.SetActive(true);
    }

    // int Last Menu -->
    void HideHighScoreMenu(){
        // Play Click Sound
        PlayClickSound();

        if(previousMenu==0){
            uiMainMenu.SetActive(true);
        }
        if(previousMenu==1){
            uiGameOverMenu.SetActive(true);
        }
        uiHighScoreMenu.SetActive(false);
    }

    public void SetGameMusic(){
        if(gameSoundStatus==0){
            // Enable Music
            gameMusic.GetComponent<AudioSource>().enabled = true;
        }
        else{
            // Disable Music
            gameMusic.GetComponent<AudioSource>().enabled = false;
        }
        // Change Menu Button Sprite and Disable or Enable Ambiance Sound
        if(gameSoundStatus==0){
            uiMainMenu.transform.GetChild(3).transform.GetComponent<Image>().sprite = uiSoundSprites[gameSoundStatus];
            uiGameOverMenu.transform.GetChild(4).transform.GetComponent<Image>().sprite = uiSoundSprites[gameSoundStatus];
			if(status != GAMESTATUS.notready){
				ambianceSound.SetActive (true);
			}
		}
        else if(gameSoundStatus==1){
            uiMainMenu.transform.GetChild(3).transform.GetComponent<Image>().sprite = uiSoundSprites[gameSoundStatus];
            uiGameOverMenu.transform.GetChild(4).transform.GetComponent<Image>().sprite = uiSoundSprites[gameSoundStatus];
			if(status != GAMESTATUS.notready){
				ambianceSound.SetActive (true);
			}
		}
        else if(gameSoundStatus==2){
            uiMainMenu.transform.GetChild(3).transform.GetComponent<Image>().sprite = uiSoundSprites[gameSoundStatus];
            uiGameOverMenu.transform.GetChild(4).transform.GetComponent<Image>().sprite = uiSoundSprites[gameSoundStatus];
			ambianceSound.SetActive(false);
		}
    }

    // Player Game Informations User Interface
    void UpdateUI(){
        uiTime.text = "" + gameRemainTime;
        uiJumpBonus.text = "" + bonunJumpPowerCount;
        uiShieldBonus.text = "" + bonusShield;
    }
    #endregion

    #region Sound Functions

    // Check Sound Setting and Set Sound Status
    void ChangeSoundStatus(){
        gameSoundStatus++;
        gameSoundStatus = gameSoundStatus % 3;
        SecurePlayerPrefs.SetInt("SOUND", gameSoundStatus);
        PlayClickSound();
        SetGameMusic();
    }

    /*
     *  Play Sounds 
     */
     public void PlayClickSound(){
        if(gameSoundStatus!=2){
            audioPlayerSource.PlayOneShot(buttonClickSound, 1F);
        }
    }
    public void PlayRecordSound(){
        if(gameSoundStatus!=2){
            audioPlayerSource.PlayOneShot(newRecordSound, 1F);
        }
    }
    #endregion
    
    #region Score Functions

    // Check High Score and High Level Prefs
    public void CheckHighScore(){
        uiGameOverMenu.transform.GetChild(5).transform.GetComponent<Image>().enabled = false;
        if (currentLevel>SecurePlayerPrefs.GetInt("HIGH_LEVEL") || (currentLevel==SecurePlayerPrefs.GetInt("HIGH_LEVEL") && currentScore > SecurePlayerPrefs.GetInt("HIGH_SCORE"))){
            // Play Record Sound
            PlayRecordSound();
            SecurePlayerPrefs.SetInt("HIGH_LEVEL", currentLevel);
            SecurePlayerPrefs.SetInt("HIGH_SCORE", currentScore);
            uiGameOverMenu.transform.GetChild(5).transform.GetComponent<Image>().enabled = true;
        }
    }

    /*
     *  Hand Scores
     *  Check Player Score set level and reset
     *  Counter or Current Score for every level
     *  This score technique prevents from the scores which have long digits
     */
    public void HandleScores(){
        // Increase Score Counter
        scoreCounter++;
        // Check Score Counter and Increase Score
        if(scoreCounter % 25 == 0 || scoreCounter > 25){
            currentScore++;
            gameRemainTime--;
            // Check Score and Increase Level
            if(currentScore % (50 *currentLevel) == 0 || currentScore > (50 *currentLevel)){
                currentLevel++;
                currentScore = 0;
            }
            scoreCounter = 0;
        }
    }

    ///////////////////////////////////////////////////// 
    // !!!NOT NEED THIS METHOD FOR PUBLISH VERSION!!! //
    ///////////////////////////////////////////////////
    // Reset High Score and Level Prefs
    public void ResetHighScores(){
        SecurePlayerPrefs.SetInt("HIGH_LEVEL", 1);
        SecurePlayerPrefs.SetInt("HIGH_SCORE", 0);
    }
    #endregion

    #region Game Helper Functions
    /*
     *  Load Prefs
     */
    public void LoadPrefs(){
        highScore = SecurePlayerPrefs.GetInt("HIGH_SCORE");
        highLevel = SecurePlayerPrefs.GetInt("HIGH_LEVEL");
        gameSoundStatus = SecurePlayerPrefs.GetInt("SOUND");
    }

    // if game is stopped return true
    public bool isGameStop(){
        if(status == GAMESTATUS.stopped){
            return true;
        }
        else{
            return false;
        }
    }

	// Start Line Counter
	IEnumerator CountTimer(int _waitTime){
        uiStartLineCounter.enabled = true;
        // Start Timer and Count Down
        while(_waitTime>0){
			yield return new WaitForSeconds(1);
			_waitTime--;
            uiStartLineCounter.text = _waitTime + "";
		}
        uiStartLineCounter.enabled = false;
        
		// Enable Player Components
        gamePlayer.transform.GetChild(0).transform.GetComponent<ConstantForce>().enabled = true;
        gamePlayer.transform.GetChild(0).transform.GetComponent<PlayerController>().enabled = true;

        // Set Game as Running
		SetGameState (GAMESTATUS.running);
	}
    #endregion
}
