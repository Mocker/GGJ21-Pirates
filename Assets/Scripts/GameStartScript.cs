using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartScript : MonoBehaviour
{
    public GameObject playerObject;
    public GameObject spawnTop;
    public GameObject spawnMid;
    public GameObject spawnBottom;

    public GameObject obstacleTemplate;
    public GameObject enemyTemplate;

    public GameObject[] waves;

    public GameObject[] cloudSpawners;
    private List<GameObject> cloudObjects = new List<GameObject>{};
    private float _cloudSpawnCounter = 0.1f;

    public GameObject gameUI, menuUI, pauseUI, winUI, loseUI;

    public string currentState = "menu";

    public float shipSpeed = 1.0f, shipBaseY = -0.78f;
    public float pirateStartTime, pirateCountdown, distanceStart, distanceCountdown;

    private float _clearBigMessageTimer = 0.0f;

    public int isPaused = 0;

    private PlayerMoveScript _playerScript;
    private Vector3 _playerStartPos;
    // collision and repairs
    public float _repairTimer = 0.0f, _repairShipSpeed = 0.5f, _repairWait = 4.0f;
    
    // control spawning waves
    public float waveTimer = 15.0f, timeTillWave = 1.0f;
    public List<GameObject> waveObjects = new List<GameObject>{}; //store all game objects we need to move here
    // Start is called before the first frame update

    public List<GameObject> enemyHorde = new List<GameObject>{};

    public GameObject overlayLeft, overlayRight;
    private Vector3 overlayLeftStart, overlayLeftEnd;
    private Vector3 overlayRightStart, overlayRightEnd;
    private IEnumerator overlayRightCo, overlayLeftCo, _playerMoveCo;
    void Start()
    {
        isPaused = 1;
        _playerScript = playerObject.GetComponent<PlayerMoveScript>();
        shipSpeed = _playerScript.speed;
        _playerStartPos = playerObject.transform.position;
        playerObject.transform.position = new Vector3(-7f, _playerStartPos.y, _playerStartPos.z);
        overlayLeftStart = overlayLeft.transform.position;
        overlayRightStart = overlayRight.transform.position;
        overlayLeftEnd = new Vector3(overlayLeftStart.x-10, overlayLeftStart.y, overlayLeftStart.z);
        overlayRightEnd = new Vector3(overlayRightStart.x+10, overlayRightStart.y, overlayLeftStart.z);
    }

    public void StartPlaying()
    {
        pirateStartTime = pirateCountdown; distanceStart = distanceCountdown;
        Debug.Log("Start play transition");
        currentState = "menuToPlayTransition";
        GameObject.Find("PlayButton").SetActive(false);
        //overlayRightCo = MoveOverSeconds( overlayRight, overlayRightEnd, 10f, "menuToPlay");
        _playerMoveCo = MoveOverSeconds( playerObject, _playerStartPos, 4f, "actuallyPlay");

        StartCoroutine(_playerMoveCo);
        
    }
    public void ActuallyStartPlaying()
    {
        Debug.Log("can actually play");
        playerObject.transform.position = _playerStartPos;
        currentState = "playing";
        menuUI.SetActive(false);
        pauseUI.SetActive(false);
        gameUI.SetActive(true);
        isPaused = 0;
        SpawnHorde();
    }

    public void DisplayMenu()
    {
        currentState = "menu";
        //GameObject.Find("overlay-color-left").SetActive(true);
        //GameObject.Find("overlay-color-right").SetActive(true);
        GameObject.Find("PlayButton").SetActive(true);
        menuUI.SetActive(true);
        pauseUI.SetActive(false);
        gameUI.SetActive(false);
        isPaused = 1;
    }

    public void GameOver()
    {
        isPaused = 1;
        currentState = "GameOver";
        menuUI.SetActive(false);
        pauseUI.SetActive(false);
        gameUI.SetActive(false);
        loseUI.SetActive(true);
        //ShowBigMessage("Pirates Caught you, YARRR", 60.0f);
    }
    public void GameWon()
    {
        isPaused = 1;
        currentState = "GameWon";
        menuUI.SetActive(false);
        pauseUI.SetActive(false);
        gameUI.SetActive(false);
        loseUI.SetActive(false);
        winUI.SetActive(true);
    }

    //manage fleet of enemy ships behind you , each tick existing ships move closer and scale up
    //and new tiny ones spawn
    public void SpawnHorde()
    {
        float progress = 100 -(int)((pirateCountdown*100) / pirateStartTime); //scale 1-100 of how close pirates are
        int totalPirates = (int)(progress / 10) + (progress < 10f ? 1 : 2);
        //Debug.Log("horde progress "+progress+" - total pirates: "+totalPirates);
        //add any pirates that need to be created
        int currentPirates = enemyHorde.Count;
        GameObject EnemyParent = GameObject.Find("Enemies");
        for(int i =currentPirates; i<=totalPirates; i++){
            Debug.Log("spawning new pirate yarrr "+i+" out of "+totalPirates);
            int whichWave = Random.Range(1,4);
        Vector3 ePos = new Vector3( 
            enemyTemplate.transform.position.x + Random.Range(-1f,1f), 
            (whichWave==1 ? spawnTop.transform.position.y : (whichWave==2? spawnMid.transform.position.y : spawnBottom.transform.position.y)), 
            enemyTemplate.transform.position.z);
            GameObject pirate = (GameObject) Instantiate(enemyTemplate, ePos, Quaternion.identity);
                pirate.transform.SetParent(EnemyParent.transform);
                pirate.transform.localScale = new Vector3( 1.0f, 1.0f, 1.0f);
                pirate.SetActive(true);
                pirate.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Player");
                pirate.GetComponent<Renderer>().sortingOrder = (whichWave==1 ? 1 : (whichWave==2? 3: 4));
                pirate.tag = "EnemyHorde";
                pirate.GetComponent<EnemyHordeScript>().StartMovingIt(
                    new Vector3( playerObject.transform.position.x, ePos.y, ePos.z),
                    new Vector3( 3.0f, 3.0f, 1f), 
                    pirateCountdown
                );
                enemyHorde.Add(pirate);
        }

    }

    // called when player speed is modified to change all related factors
    public void ChangeSpeed( float newSpeed=1.0f)
    {
        
        shipSpeed = newSpeed;
        _playerScript.speed = newSpeed;
        for(int i = 0; i < waves.Length; i++) {
            waves[i].GetComponent<AnimateTextureScript>().ChangeSpeed(shipSpeed);
            //waves[i].GetComponent<AnimateTextureScript>()._playerSpeed = (_playerScript.speed/2/0f);
        }
        
    }

    public void IncrementSpeed( float inc) 
    {
        float newSpeed = shipSpeed + inc ;
        if(newSpeed < _playerScript.maxSpeed && newSpeed > _playerScript.minSpeed) {
            ChangeSpeed(newSpeed);
        }
    }

    public void ShowBigMessage( string msg, float duration)
    {
        GameObject.Find("BigMessage").GetComponent<UnityEngine.UI.Text>().text = msg;
        _clearBigMessageTimer = duration;
    }

    void SpawnWave()
    {
        //start with an x offset and iterate to cover a screen width of spawned obstacles
        //change hard coded screen width and enemy intervals, need enough room for ship to dodge between waves
        for(float x = 0.0f; x < waveTimer /*TODO: CHANGE TO REAL WIDTH*/; x+= 4.0f)
        {
            // TODO:: can change weights based on difficulty to spawn more often if we want
            int shouldSpawnTop = Random.Range(0,3);
            int shouldSpawnBottom = Random.Range(0,11);
            int shouldSpawnMid = (shouldSpawnTop > 0 && shouldSpawnBottom > 0) ? 0 : Random.Range(0,11);
            if(shouldSpawnTop < 1 && shouldSpawnBottom < 1 ) {
                shouldSpawnMid = 0;
            }


            GameObject EnemyParent = GameObject.Find("Obstacles");
            // TODO:: support spawning enemy ships
            // could refactor to have array of lanes with associated spawner.. but meeeeeehhhh
            if(shouldSpawnTop < 1)
            {
                Vector3 ePos = new Vector3(spawnTop.transform.position.x + x, spawnTop.transform.position.y, spawnTop.transform.position.z);
                GameObject EnemyTop = (GameObject) Instantiate(obstacleTemplate, ePos, Quaternion.identity);
                EnemyTop.transform.SetParent(EnemyParent.transform);
                EnemyTop.transform.localScale = new Vector3( 2.0f, 2.0f, 1.0f);
                EnemyTop.SetActive(true);
                EnemyTop.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Player");
                EnemyTop.GetComponent<Renderer>().sortingOrder = 2;
                EnemyTop.GetComponent<MoveObstacleScript>().currentLane = 1;
                EnemyTop.tag = "Obstacle";
                waveObjects.Add(EnemyTop);
            }
            if(shouldSpawnMid < 1)
            {
                Vector3 ePos = new Vector3(spawnTop.transform.position.x + x, spawnMid.transform.position.y, spawnTop.transform.position.z);
                
                GameObject EnemyMid = (GameObject) Instantiate(obstacleTemplate, ePos, Quaternion.identity);
                EnemyMid.transform.SetParent(EnemyParent.transform);
                EnemyMid.transform.localScale = new Vector3( 2.0f, 2.0f, 1.0f);
                EnemyMid.SetActive(true);
                EnemyMid.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Player");
                EnemyMid.GetComponent<Renderer>().sortingOrder = 3;
                EnemyMid.GetComponent<MoveObstacleScript>().currentLane = 2;
                EnemyMid.tag = "Obstacle";
                waveObjects.Add(EnemyMid);
            }
            if(shouldSpawnBottom < 1)
            {
                Vector3 ePos = new Vector3(spawnTop.transform.position.x + x, spawnBottom.transform.position.y, spawnTop.transform.position.z);
                
                GameObject EnemyBottom = (GameObject) Instantiate(obstacleTemplate, ePos, Quaternion.identity);
                EnemyBottom.transform.SetParent(EnemyParent.transform);
                EnemyBottom.transform.localScale = new Vector3( 2.0f, 2.0f, 1.0f);
                EnemyBottom.SetActive(true);
                EnemyBottom.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Player");
                EnemyBottom.GetComponent<Renderer>().sortingOrder = 4;
                EnemyBottom.GetComponent<MoveObstacleScript>().currentLane = 3;
                EnemyBottom.tag = "Obstacle";
                waveObjects.Add(EnemyBottom);
            }
        }
    }

    public void PlayerCollided(GameObject whammo) {
        ChangeSpeed(_repairShipSpeed);
        _repairTimer = _repairWait;
    }
    
    //CoRoutine to move an object from start to dest over time
    public IEnumerator MoveOverSeconds (GameObject objectToMove, Vector3 end, float seconds, string eventName)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
        objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
        elapsedTime += Time.deltaTime;
        yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
        if(currentState == "menuToPlayTransition") {
            ActuallyStartPlaying();
        }
    } 
    

    public void MoveClouds()
    {
                   //cloud spawn
            if( _cloudSpawnCounter != 0.0f) {
                _cloudSpawnCounter -= Time.deltaTime;
                if( _cloudSpawnCounter <= 0.0f) {
                    _cloudSpawnCounter = 1.2f;
                    for(int i = 0; i < cloudSpawners.Length; i++) {
                        float doCloud = Random.Range(0.0f, 1.0f);
                        if( 0.9f < doCloud  ) {
                            int cloudIndex = Random.Range(0, cloudSpawners.Length-1);
                            GameObject cloud = (GameObject) Instantiate(cloudSpawners[cloudIndex], cloudSpawners[i].transform.position, Quaternion.identity);
                            
                            cloud.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("waves");
                            cloudObjects.Add(cloud);
                        }
                    }
                }
            }

            List<GameObject> destroyThese = new List<GameObject>{};
            foreach( GameObject cloudObj in cloudObjects) {
                    cloudObj.transform.position = new Vector3(cloudObj.transform.position.x -  Time.deltaTime, cloudObj.transform.position.y, cloudObj.transform.position.z);
                    if(cloudObj.transform.position.x < -10f) {
                        destroyThese.Add(cloudObj);
                        
                    }
            }
            foreach( GameObject cloudObj in destroyThese) {
                cloudObjects.Remove(cloudObj);
                Destroy(cloudObj);
            }
    }

    // Update is called once per frame

    void Update()
    {

        // menu transition
        /*if(currentState == "menuToPlayTransition") {
            GameObject.Find("overlay-color-left").transform.position = Vector3.Lerp(overlayLeftStart, overlayLeftEnd, 0.1f*Time.deltaTime);
            GameObject.Find("overlay-color-right").transform.position = Vector3.Lerp(overlayRightStart, overlayRightEnd, 0.1f*Time.deltaTime);
            if( GameObject.Find("overlay-color-right").transform.position.x > 1.5f ) {
                ActuallyStartPlaying();
            }
        }*/

        // Pause
        // TODO:: use input mapping
        if(currentState == "playing" &&Input.GetKeyDown(KeyCode.LeftShift)){
            isPaused = isPaused < 1 ? 1 : 0;
            if(isPaused < 1){
                pauseUI.SetActive(false);
            } else {
                pauseUI.SetActive(true);
            }
            for(int i = 0; i<waves.Length; i++){
                waves[i].GetComponent<AnimateTextureScript>().isPaused = isPaused;
            }
        }

        if( currentState == "menu" || currentState == "menuToPlayTransition" 
            || (currentState == "playing" && isPaused < 1) ) {
                MoveClouds();
            }

        if( currentState == "playing" && isPaused < 1) {
            
            SpawnHorde();

            if( playerObject) {
                pirateCountdown -= Time.deltaTime;
                distanceCountdown -= Time.deltaTime * shipSpeed ;
                GameObject.Find("Distance").GetComponent<UnityEngine.UI.Text>().text = 
                    "Distance Left: "+(int)distanceCountdown+"km @ "+shipSpeed+" knots";
                GameObject.Find("BadTimer").GetComponent<UnityEngine.UI.Text>().text = 
                    "Pirates in "+(int)pirateCountdown+"s";
            }

            if(shipSpeed == _repairShipSpeed) {
                _repairTimer -= Time.deltaTime;
                if(_repairTimer <= 0.0f) {
                    ChangeSpeed(2.0f);
                    playerObject.GetComponent<PlayerMoveScript>().Repaired();
                }
            }

            if(_clearBigMessageTimer > 1.0f) {
                _clearBigMessageTimer -= Time.deltaTime;
            }
            if(_clearBigMessageTimer != 0.0f) {
                _clearBigMessageTimer = 0.0f;
                GameObject.Find("BigMessage").GetComponent<UnityEngine.UI.Text>().text = "";
            }

            if(pirateCountdown < 1) {
                // game over
                GameOver();
            } else if( distanceCountdown < 1) {
                // winner!
                GameWon();
            }

            // speed controls
            if(shipSpeed != _repairShipSpeed) {
                if (Input.GetKeyDown(KeyCode.RightArrow)){
                    //transform.position += Vector3.right * speed * Time.deltaTime;
                    IncrementSpeed( _playerScript.acceleration );
                    
                }
                if (Input.GetKeyDown(KeyCode.LeftArrow)){
                    //transform.position += Vector3.left* speed * Time.deltaTime;
                    IncrementSpeed( _playerScript.acceleration * -1 );
                }
            }

            // wave spawner
            if (timeTillWave > 0.0f) {
                timeTillWave -= shipSpeed * Time.deltaTime;
                if(timeTillWave <= 0.0f) {
                    SpawnWave();
                    timeTillWave = waveTimer;
                }
            }
            //  move wave obstacles

            List<GameObject> destroyThese = new List<GameObject>{};
            foreach( GameObject waveObj in waveObjects) {
                waveObj.transform.position = new Vector3(waveObj.transform.position.x - (shipSpeed * Time.deltaTime), waveObj.transform.position.y, waveObj.transform.position.z);
                if(waveObj.transform.position.x < -10f){
                    destroyThese.Add(waveObj);
                } else if(waveObj.transform.position.x > -0.2f && waveObj.transform.position.x < 1.1f
                    && waveObj.GetComponent<MoveObstacleScript>().currentLane == _playerScript._currentWave) {
                        Debug.Log("waveObj collided!");
                        PlayerCollided(waveObj);
                        _playerScript.OnCollidedObstacle(waveObj);
                        destroyThese.Add(waveObj);
                    }
                
            }
            foreach( GameObject cloudObj in destroyThese) {
                waveObjects.Remove(cloudObj);
                Destroy(cloudObj);
            }
            destroyThese.Clear();

 

            // TODO:: spawn/scale chasing enemy horde based on timer checkpoints
        }
    }
}
