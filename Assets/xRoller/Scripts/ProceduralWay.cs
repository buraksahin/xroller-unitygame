using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralWay : MonoBehaviour {

    #region Variables
    /*
	 * Variables
	 */
    // Game Object Prefabs
    public GameObject objObstacle;			// Way Obstacle
	public GameObject objRoad;				// Road Prefab
    public GameObject objRoadObstacle;      // Road Front Side Obstacle
	public GameObject objRoadSize;			// Road Size Prefab
	public GameObject objExtraPointBonus;	// Bonus Prefab
	public GameObject objExtraTimeBonus;	// Bonus Prefab
	public GameObject objShieldBonus;		// Bonus Prefab
	public GameObject objJumpBonus;			// Bonus Prefab
    public GameObject[] objSideRoads;       // Side Roads Prefab
    private int gameLevel;                  // Game Level
    public  int maxLevelObstacle = 2;       // Max Obstacle Level
    private int maxObstacle;                // Max Obstacle Value

	// New Game Objects
	private GameObject roadLength;  // Road Size Game Object
	private GameObject road;        // Road Game Object
    private GameObject roadObstacle;// Road Obstacle Object
	private GameObject[] obstacles; // Obstacle Array List
    private GameObject[] bonus;     // Bonus Array List
    private GameObject ramp;        // Road Ramp Game Object
    private GameObject sideRoad;    // Side Road
    private int sideRoadCount = 0;  // Side Road Count
    // Road Scale
    private float roadXScale;           // Road X Scale
    private float roadZScale;           // Road Z Scale
    private float roadMinXScale = 5f;   // Road Min X Scale
    private float roadMinZScale = 50f;  // Road Min Z Scale

    // Road Positon
    private float roadXPos;         // Road X Position
    private float roadZPos;         // Road Z Position

	// Obstacle Min Max Scale Range
	private float obsMinXScale;		// Obstacle Min X Scale
	private	float obsMaxXScale;		// Obstacle Max X Scale
	private float obsMinZScale;		// Obstacle Min Z Scale
	private	float obsMaxZScale;	    // Obstacle Max Z Scale
    
    // Obstacle Scales
	private float obsXScale;   		// Obstacle X Scale
	private float obsYScale;   		// Obstacle Y Scale
	private float obsZScale;   		// Obstacle Z Scale

	// Obstacle Min Max Position Range
    private float obsMinXPosition;	// Obstacle X Min position
    private float obsMaxXPosition;	// Obstacle X Max position
    private float obsMinZPosition;  // Obstacle Z Min position
    private float obsMaxZPosition;	// Obstacle Z Max position

    // Obstacle Position
    private bool obsCorner;         // Set Obstacle Corner
    private float obsXPosition;		// Obstacle X position
	private float obsYPosition;		// Obstacle Y position
	private float obsZPosition;		// Obstacle Z position

    // Obstacle Materials
    public Material[] matObstacle;  // Obstacle Materials

	// Bonus Variables
	public int maxBonusCount;		// Max Bonus Count
	private float bonusXPos;		// Bonus X Pos
	private float bonusYPos;		// Bonus Y Pos
	private float bonusZPos;		// Bonus Z Pos

    #endregion

    #region Helper Functions
    void Awake(){
        // Get Game Level for Prepare Obstacles as Level
        gameLevel = GameManager.instance.currentLevel;
        if(gameLevel < 2){
            maxObstacle = 1;	// Obstacle Count
			maxBonusCount = 1;	// Bonus Count
            sideRoadCount = 0;  // Side Road Count
        }
        else{
            maxObstacle = Random.Range(0,3);    // Obstacle Count
			maxBonusCount = 2;	                // Bonus Count
            sideRoadCount = Random.Range(0,5);  // Side Road Count
        }

        // Set Obstacles Game Object Array
        obstacles = new GameObject[maxObstacle];

        // Set Bonus Game Object Array
        bonus = new GameObject[maxBonusCount];

		// Instantiate Game Objects
		roadLength = Instantiate (objRoadSize, transform.localPosition, new Quaternion (0f, 0f, 0f, 0f), transform);
		road = Instantiate (objRoad, transform.localPosition, new Quaternion (0f, 0f, 0f, 0f), transform);

        // Set Road
        SetRoad();

        // Set Obstacles
		SetObstacles();

		// Set Bonus
		SetBonus();
	}
	#endregion

	#region Set Road
    // Set Road
    void SetRoad(){
        // Set as Parent
        roadLength.transform.parent = transform;

        // Set as Parent
        road.transform.parent = transform;

        // Set Position
        if(gameLevel > 1 && Random.Range(0,4) == 3){
            // Set Road Scale
            roadXScale = Random.Range(roadMinXScale, roadLength.transform.localScale.x);
            roadMinZScale = roadLength.transform.localScale.x / maxObstacle;
            roadZScale = Random.Range(roadMinXScale, roadLength.transform.localScale.z);
            road.transform.localScale = new Vector3(roadXScale, 6f, roadZScale);

            // Set Road Position
            roadXPos = Random.Range((-roadLength.transform.localScale.x + road.transform.localScale.x)/2, (roadLength.transform.localScale.x - road.transform.localScale.x)/2);
            road.transform.localPosition = new Vector3(roadXPos, 0f ,0f);
        }
        else {
            // Set Road Scale
            road.transform.localScale = new Vector3(15f, 6f, 100f);
            
            // Set Road Position
            road.transform.localPosition = new Vector3(0f, 0f ,0f);
        }

        // Set Side Ways
        if(sideRoadCount == 2){
            sideRoad = Instantiate (objSideRoads[Random.Range(0, objSideRoads.Length)], transform.localPosition, new Quaternion (0f, 0f, 0f, 0f), transform);
            sideRoad.transform.parent = transform;
            sideRoad.transform.name = "SideWay";
            if(Random.Range(0,2) == 1){
                sideRoad.transform.localPosition = new Vector3(roadLength.transform.localPosition.x + roadLength.transform.localScale.x + sideRoad.transform.localScale.x /2 ,  Random.Range(0,2), roadZScale);
            }
            else {
                sideRoad.transform.localPosition = new Vector3(-roadLength.transform.localPosition.x - roadLength.transform.localScale.x - sideRoad.transform.localScale.x /2 , Random.Range(0,2), roadZScale);
            }
        }

        // Set Road Obstacle Front Side of Road
        SetRoadObstacle();
    }
	#endregion

	#region Set Obstacles
    /*
     *   Set Road Obstacle
     *   Set obstacle front side of the road
     *   for prevent player movement glitch
     */
    void SetRoadObstacle(){
        roadObstacle = Instantiate (objRoadObstacle, transform.localPosition, new Quaternion (0f, 0f, 0f, 0f), transform);
        roadObstacle.transform.parent = transform;
        roadObstacle.transform.localScale = new Vector3(road.transform.localScale.x, road.transform.localScale.y - road.transform.localScale.y/8, 0.25f);
        roadObstacle.transform.localPosition = new Vector3(road.transform.localPosition.x , road.transform.localPosition.y, -road.transform.localScale.z/2);
    }

    // Set Obstacles
	void SetObstacles(){
        obsCorner = true;
        for(int i=0; i<maxObstacle; i++){
            obstacles[i] = Instantiate (objObstacle, transform.localPosition, new Quaternion (0f, 0f, 0f, 0f), transform);
            obstacles[i].transform.parent = transform;

            // Set X Scale
            obsMinXScale = road.transform.localScale.x / 4f;
            obsMaxXScale = road.transform.localScale.x * 3f / 4f;

            // Set Z Scale
            if(road.transform.localScale.z/3f >= 3f){
                obsMaxZScale = 3f;
            }
            else{
                obsMaxZScale = road.transform.localScale.z/3f;
            }
            if(road.transform.localScale.z/3f >= 1f){
                obsMinZScale = 1f;
            }
            else{
                obsMinZScale = road.transform.localScale.z/3f;
            }

            // Set Obstacle Positions
            obsXScale = Random.Range(obsMinXScale, obsMaxXScale);
            obsYScale = Random.Range(1f, 6f);
            obsZScale = Random.Range(obsMinZScale, obsMaxZScale);
            obstacles[i].transform.localScale = new Vector3(obsXScale, obsYScale, obsZScale);

            // Set Obstacle Material
            if(obsYScale<2f){
                obstacles[i].transform.GetComponent<MeshRenderer>().material = matObstacle[0];
            }
            if(obsYScale>=2f && obsYScale<4f){
                obstacles[i].transform.GetComponent<MeshRenderer>().material = matObstacle[1];
            }
            if(obsYScale>=4f){
                obstacles[i].transform.GetComponent<MeshRenderer>().material = matObstacle[2];
            }

            // Set Obstacle Transform Scale and Position
            // Obstacle X Position
            obsMinXPosition = -road.transform.localScale.x/2 + obsXScale/2;
            obsMaxXPosition = road.transform.localScale.x/2 - obsXScale /2;
            if(obsCorner){
                if(Random.Range(0,2)==1){
                    obsXPosition = obsMinXPosition;
                }
                else{
                    obsXPosition = obsMaxXPosition;
                }
                obsCorner = false;
            }
            else{
                obsXPosition = Random.Range(obsMinXPosition, obsMaxXPosition);
                obsCorner = true;
            }
            // Obstacle Y Position
            obsYPosition = road.transform.localScale.y/2 + obstacles[i].transform.localScale.y/2;
            // Obstacle Z Position
            if(i==0){
                obsZPosition = Random.Range(-road.transform.localScale.z/2 + obsZScale/2, 0);
            }
            else{
				obsZPosition = Random.Range(obstacles[i-1].transform.localPosition.z + obstacles[i-1].transform.localScale.z/2 + obsZScale/2, road.transform.localScale.z/2 - obsZScale/2);
            }
            if(obsZPosition > -road.transform.localScale.z/2 + obsZScale/2 && obsZPosition < road.transform.localScale.z/2 - obsZScale/2){
                obstacles[i].transform.localPosition = new Vector3(obsXPosition, obsYPosition, obsZPosition);
            }
            else{
                Destroy(obstacles[i]);
                break;
            }
        }
    }
    #endregion

	#region Set Bonus
	void SetBonus(){
		/* 
		 * Set Bonus
         * objExtraPointBonus
         * objExtraTimeBonus
         * objShieldBonus
         * objJumpBonus
         */
        if(road.transform.localScale.z > 50f){
            // Instantiate Bonin Loop
		    for(int i=0; i<maxBonusCount; i++){
			    if(GameManager.instance.gameRemainTime < 30){
                    // Instantiate Extra Time Bonus
                    InstantiateBonus(i, objExtraTimeBonus);
                }
                else {
                    int rndBonusChoose = Random.Range(0,3);
                    if(rndBonusChoose == 0) {
                        // Instantiate Extra Point Bonus
                        InstantiateBonus(i, objExtraPointBonus);
                    }
                    if(rndBonusChoose == 1) {
                        // Instantiate Shield Bonus
                        InstantiateBonus(i, objShieldBonus);
                    }
                    if(rndBonusChoose == 2) {
                        // Instantiate Jump Bonus
                        InstantiateBonus(i, objJumpBonus);
                    }
                }
		    }
        }
	}

    // Set Bonus Position
    private void SetBonusPosition(int i){
        SetRandomX();
        bonusYPos = road.transform.localScale.y/2 + 3; // Gift Box Prefab Height is 3
        SetRandomZ();
        for(int x=0; x<maxObstacle; x++){
            if(obstacles[x] != null){
                if(bonusZPos > obstacles[x].transform.localPosition.z - obstacles[x].transform.localScale.z && bonusZPos < obstacles[x].transform.localPosition.z + obstacles[x].transform.localScale.z){
                    if(obstacles[x].transform.localPosition.z>roadLength.transform.localScale.z/2){
                        bonusZPos = obstacles[x].transform.localPosition.z - obstacles[x].transform.localScale.z;
                    }
                    else{
                        bonusZPos = obstacles[x].transform.localPosition.z + obstacles[x].transform.localScale.z;
                    }
                }
            }
        }
    }

    // Instantiate Bonus Object
    public void InstantiateBonus(int i, GameObject bonusPrefab){
            SetBonusPosition(i);
            bonus[i] = Instantiate (bonusPrefab, transform.localPosition, bonusPrefab.transform.localRotation, transform);
            bonus[i].transform.parent = transform;
            // Check Bonus Position
            if(bonusZPos > -road.transform.localScale.z/2 + 1.5f && bonusZPos < road.transform.localScale.z/2 - 1.5f){
                bonus[i].transform.localPosition = new Vector3(bonusXPos, bonusYPos, bonusZPos);
            }
            else{
                Destroy(obstacles[i]);
            }
    }

    private void SetRandomX(){
        bonusXPos = Random.Range(-road.transform.localScale.x/2 + 1.5f, road.transform.localScale.x/2 - 1.5f);
    }

    private void SetRandomZ(){
        bonusZPos = Random.Range(-road.transform.localScale.z/2 + 1.5f, road.transform.localScale.z/2 - 1.5f);
    }
    #endregion
}
