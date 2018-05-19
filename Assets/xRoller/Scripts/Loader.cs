using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loader : MonoBehaviour {

	// Variables
	public GameObject GM;
	public Transform _playerSpawnPoint;
	public GameObject _Player;      // Player Game Prefab
    public GameObject _MenuCam;     // Menu Cam

    // User Interface
    public Text _uiTime;                // Time
    public Text _uiStartLineCounter;    // Start Counter
    public Text _uiJumpBonus;           // Jump Bonus Text
    public Text _uiShieldBonus;         // Shield Bonus Text
    public GameObject _uiPlayerGameUI;  // Player Game Play UI
    public GameObject _uiMainMenu;      // Main Menu Canvas
    public GameObject _uiGameOverMenu;  // Game Over Menu
    public GameObject _uiHighScoreMenu; // High Score Menu Canvas
    public Sprite[] _uiSoundSprites;    // Sound Sprite
	public Text _bonusInfoText;         // Bonus Information UI Text Game Object

    // Sounds
    public GameObject _gameMusic;       // Game Music
	public GameObject _ambianceSound;	// Ambiance Sound

	void Awake ()
	{
        _Player.transform.GetChild(0).GetComponent<PlayerCollision>().bonusInfoText = _bonusInfoText;
		//Check if a GameManager has already been assigned to static variable GameManager.instance or if it's still null
		if (GameManager.instance == null){

            // Set User Interface Prefabs
            GM.GetComponent<GameManager>().uiTime= _uiTime;
            GM.GetComponent<GameManager>().uiStartLineCounter = _uiStartLineCounter;
            GM.GetComponent<GameManager>().uiJumpBonus= _uiJumpBonus;
            GM.GetComponent<GameManager>().uiShieldBonus= _uiShieldBonus;
            GM.GetComponent<GameManager>().uiPlayerGameUI= _uiPlayerGameUI;
            GM.GetComponent<GameManager>().uiMainMenu = _uiMainMenu;
            GM.GetComponent<GameManager>().uiGameOverMenu = _uiGameOverMenu;
            GM.GetComponent<GameManager>().uiHighScoreMenu = _uiHighScoreMenu;
            GM.GetComponent<GameManager>().uiSoundSprites = _uiSoundSprites;

            // Set Audio Sound Clip Prefabs
            GM.GetComponent<GameManager>().gameMusic = _gameMusic;
			GM.GetComponent<GameManager>().ambianceSound = _ambianceSound;

            // Set Other Prefs
			GM.GetComponent<GameManager>().playerSpawn = _playerSpawnPoint;
			GM.GetComponent<GameManager>().Player = _Player;
			GM.GetComponent<GameManager>().MenuCam = _MenuCam;

            // Instantiate gameManager Prefabs
			Instantiate (GM);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
