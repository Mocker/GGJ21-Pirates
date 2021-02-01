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
    public GameObject enemyTemplate, enemyHordeTemplate, pickupTemplate;

    public GameObject[] waves;

    public GameObject[] cloudSpawners;
    private List<GameObject> cloudObjects = new List<GameObject>{};
    private float _cloudSpawnCounter = 0.1f;

    public GameObject Wellerman, gameUI, menuUI, pauseUI, winUI, loseUI, playButton, helpButton;
    public float wellerManLength = 10f;// 60f * 1.37320f;
    public float wellerManCountdown, wellerManIntermissionCounter;
    public AudioClip[] themeSongs;
    public AudioClip titleSong;
    private int _currentThemeIndex;

    public string currentState = "menu";

    public float shipSpeed = 1.0f, shipBaseY = -0.78f;
    public float pirateStartTime, pirateCountdown, distanceStart, distanceCountdown, distanceCounter = 0f;
    
    // current stage
    public int distanceStage = 1;
    // how many mini speed boosts per stage and how fast to boost
    public float stageSpeedBoostNum = 4.0f, stageSpeedBoostSpeed = 0.25f;
    public float currentBaseSpeed;
    // base speed for stage x , step 0
    public float speedBasePerStage = 2.0f;

    // how many obstacles 
    public int numObstaclesBase = 12;
    public int numObstaclesPerWave = 2;
    public int baseLifesUntilPirates = 4, lifesUntilPirates = 4;

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

    private CameraShakeScript _camShake;
    public float camShakeAmt = 0.1f;
    void Start()
    {
        isPaused = 1;
        _playerScript = playerObject.GetComponent<PlayerMoveScript>();
        _camShake = this.gameObject.GetComponent<CameraShakeScript>();
        shipSpeed = _playerScript.speed;
        _playerStartPos = playerObject.transform.position;
        playerObject.transform.position = new Vector3(-7f, _playerStartPos.y, _playerStartPos.z);
        overlayLeftStart = overlayLeft.transform.position;
        overlayRightStart = overlayRight.transform.position;
        overlayLeftEnd = new Vector3(overlayLeftStart.x-10, overlayLeftStart.y, overlayLeftStart.z);
        overlayRightEnd = new Vector3(overlayRightStart.x+10, overlayRightStart.y, overlayLeftStart.z);
        DisplayMenu();
    }

    public void StartPlaying()
    {
        pirateStartTime = pirateCountdown; distanceStart = distanceCountdown;
        playerObject.transform.position = new Vector3(-7f, _playerStartPos.y, _playerStartPos.z);
        Debug.Log("Start play transition");
        currentState = "menuToPlayTransition";
        playButton.SetActive(false);
        helpButton.SetActive(false);
        lifesUntilPirates = baseLifesUntilPirates;
        distanceCounter = 0f;
        // TODO:: let the player continue from the last stage they were one changing setting stats to match it
        distanceStage = 1;
        
        
        // moves the player to mid screen before we start
        _playerMoveCo = MoveOverSeconds( playerObject, _playerStartPos, 3.8f, "actuallyPlay");
        StartCoroutine(_playerMoveCo);
        _currentThemeIndex = 0;
        Wellerman.GetComponent<AudioSource>().Stop();
        Wellerman.GetComponent<AudioSource>().clip = themeSongs[_currentThemeIndex];
        Wellerman.GetComponent<AudioSource>().Play();
        wellerManLength = Wellerman.GetComponent<AudioSource>().clip.length;
        wellerManCountdown = wellerManLength; //stages are seperated by length of the song
        currentBaseSpeed = distanceStage * speedBasePerStage;
        _playerScript.minSpeed = distanceStage * speedBasePerStage - 1f;
        _playerScript.minSpeed = distanceStage * speedBasePerStage + 1f;
        
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
        string scoreText = "";
        if( PlayerPrefs.HasKey("TopScore") ){
            int topScore = PlayerPrefs.GetInt("TopScore");
            scoreText = "Top Score: "+topScore;
        }
        
        Wellerman.GetComponent<AudioSource>().Stop();
        if(titleSong){
            Wellerman.GetComponent<AudioSource>().clip = titleSong;
            Wellerman.GetComponent<AudioSource>().Play();
        } else { Debug.Log("Missing title song");}

        playButton.SetActive(true);
        menuUI.SetActive(true);
        menuUI.transform.Find("TopScoreText").GetComponent<UnityEngine.UI.Text>().text = scoreText;
        pauseUI.SetActive(false);
        gameUI.SetActive(false);
        winUI.SetActive(false);
        loseUI.SetActive(false);
        isPaused = 1;
    }

    public void GameOver()
    {   
        Debug.Log("Game Over");
        // TODO:: figure better calculation for score, like pirates sunk, weight of the gold etc
        int totalScore = (int)distanceCounter + (_playerScript.gold*50) + (_playerScript.obstaclesSunk*25);
        
        if( PlayerPrefs.HasKey("TopScore") ){
            int topScore = PlayerPrefs.GetInt("TopScore");
            if(totalScore > topScore) {
                PlayerPrefs.SetInt("TopScore", totalScore);
            }
        } else {
            PlayerPrefs.SetInt("TopScore", totalScore);
        }
        ChangeSpeed(1.0f);
        isPaused = 1;
        currentState = "GameOver";
        menuUI.SetActive(false);
        pauseUI.SetActive(false);
        gameUI.SetActive(false);
        loseUI.SetActive(true);
        loseUI.transform.Find("ScoreText").GetComponent<UnityEngine.UI.Text>().text = "Score: "+totalScore;
        //ShowBigMessage("Pirates Caught you, YARRR", 60.0f);
    }
    public void GameWon()
    {
        // DONT USE THIS -- you can never win :)
        Debug.Log("You Won!");
        ChangeSpeed(1.0f);
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
        float progress = (int)((baseLifesUntilPirates - lifesUntilPirates) / baseLifesUntilPirates)*100; //scale 1-100 of how close pirates are
        int totalPirates = (int)((baseLifesUntilPirates - lifesUntilPirates))*3 + (progress < 10f ? 1 : 2);
        
        //Move existing pirates one step closer to the player
        foreach(GameObject pirate in enemyHorde) {
            pirate.GetComponent<EnemyHordeScript>().MoveAStep( baseLifesUntilPirates - lifesUntilPirates);
        }

        //add any pirates that need to be created
        int currentPirates = enemyHorde.Count;
        GameObject EnemyParent = GameObject.Find("Enemies");
        for(int i =currentPirates; i<=totalPirates; i++){
            Debug.Log("spawning new pirate yarrr "+i+" out of "+totalPirates);
            int whichWave = Random.Range(1,4);
        Vector3 ePos = new Vector3( 
            enemyHordeTemplate.transform.position.x + Random.Range(-1f,1f), 
            (whichWave==1 ? spawnTop.transform.position.y : (whichWave==2? spawnMid.transform.position.y : spawnBottom.transform.position.y)), 
            enemyHordeTemplate.transform.position.z);
            ePos.y += 0.2f;
            GameObject pirate = (GameObject) Instantiate(enemyHordeTemplate, ePos, Quaternion.identity);
                pirate.transform.SetParent(EnemyParent.transform);
                pirate.transform.localScale = new Vector3( 1.0f, 1.0f, 1.0f);
                pirate.SetActive(true);
                pirate.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Player");
                pirate.GetComponent<Renderer>().sortingOrder = (whichWave==1 ? 2 : (whichWave==2? 3: 4));
                pirate.tag = "EnemyHorde";
                pirate.GetComponent<EnemyHordeScript>().StartMovingIt(
                    new Vector3( playerObject.transform.position.x-1f, ePos.y-0.2f, ePos.z),
                    new Vector3( 4f, 4f, 4f), 
                    baseLifesUntilPirates
                );
                enemyHorde.Add(pirate);
        }
    }

    // a new life moves the pirate horde back a step as well
    public void LifeUp()
    {
        if( lifesUntilPirates >= baseLifesUntilPirates) return; //don't go above this
        lifesUntilPirates++;
        //Move existing pirates one step further to the player
        foreach(GameObject pirate in enemyHorde) {
            pirate.GetComponent<EnemyHordeScript>().MoveAStep( baseLifesUntilPirates - lifesUntilPirates);
        }
    }

    // called when player speed is modified to change all related factors
    public void ChangeSpeed( float newSpeed=1.0f)
    {
        
        shipSpeed = newSpeed;
        _playerScript.speed = newSpeed;
        for(int i = 0; i < waves.Length; i++) {
            waves[i].GetComponent<AnimateTextureScript>().ChangeSpeed(shipSpeed);
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
        for(float x = 0.0f; x < waveTimer /*TODO: CHANGE TO REAL WIDTH*/; x+= 6.0f)
        {
            // TODO:: can change weights based on difficulty to spawn more often if we want
            // currently we just increase speed every stage
            int shouldSpawnTop = Random.Range(0,5);
            int shouldSpawnBottom = Random.Range(0,5);
            int shouldSpawnMid = (shouldSpawnTop > 0 && shouldSpawnBottom > 0) ? 0 : Random.Range(0,5);
            if(shouldSpawnTop < 1 && shouldSpawnBottom < 1 ) {
                shouldSpawnMid = 1;
            }

            // Decide on type of obstacle. The rocks should be most common
            string tag = "Obstacle";
            GameObject template = obstacleTemplate;
            Vector3 krakenScale = new Vector3(2.0f, 2.0f, 1.0f);
            int spawnTheKraken = Random.Range(0,100);
            string templateName = "rocks";
            // TODO:: modify spawn weights on each template, sum them up and then pick that way
            // TODO:: quick change on percents before release
            if (spawnTheKraken > 98) {
                //repair powerup
                template = pickupTemplate;
                tag = "Pickup";
                templateName = "repair";
            }
            else if(spawnTheKraken > 90) {
                // gold chest
                template = pickupTemplate;
                tag = "Pickup";
                templateName = "gold";
            } else if(spawnTheKraken > 70) {
                // enemy ship
                template = enemyTemplate;
                tag = "Enemy";
                krakenScale = new Vector3(3.0f, 3.0f, 1.0f);
                templateName = "enemy";
            }

            GameObject EnemyParent = GameObject.Find("Obstacles");
            // TODO:: support spawning enemy ships
            // could refactor to have array of lanes with associated spawner.. but meeeeeehhhh
            if(shouldSpawnTop < 1)
            {
                Vector3 ePos = new Vector3(spawnTop.transform.position.x + x, spawnTop.transform.position.y, spawnTop.transform.position.z);
                GameObject EnemyTop = (GameObject) Instantiate(template, ePos, Quaternion.identity);
                EnemyTop.transform.SetParent(EnemyParent.transform);
                EnemyTop.transform.localScale = krakenScale;
                EnemyTop.SetActive(true);
                EnemyTop.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Player");
                EnemyTop.GetComponent<Renderer>().sortingOrder = 2;
                EnemyTop.GetComponent<MoveObstacleScript>().currentLane = 1;
                EnemyTop.GetComponent<MoveObstacleScript>().setObstacleName(templateName);
                EnemyTop.tag = tag;
                waveObjects.Add(EnemyTop);
            }
            if(shouldSpawnMid < 1)
            {
                Vector3 ePos = new Vector3(spawnTop.transform.position.x + x, spawnMid.transform.position.y, spawnTop.transform.position.z);
                
                GameObject EnemyMid = (GameObject) Instantiate(template, ePos, Quaternion.identity);
                EnemyMid.transform.SetParent(EnemyParent.transform);
                EnemyMid.transform.localScale = krakenScale;
                EnemyMid.SetActive(true);
                EnemyMid.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Player");
                EnemyMid.GetComponent<Renderer>().sortingOrder = 3;
                EnemyMid.GetComponent<MoveObstacleScript>().currentLane = 2;
                EnemyMid.GetComponent<MoveObstacleScript>().setObstacleName(templateName);
                EnemyMid.tag = tag;
                waveObjects.Add(EnemyMid);
            }
            if(shouldSpawnBottom < 1)
            {
                Vector3 ePos = new Vector3(spawnTop.transform.position.x + x, spawnBottom.transform.position.y, spawnTop.transform.position.z);
                
                GameObject EnemyBottom = (GameObject) Instantiate(template, ePos, Quaternion.identity);
                EnemyBottom.transform.SetParent(EnemyParent.transform);
                EnemyBottom.transform.localScale = krakenScale;
                EnemyBottom.SetActive(true);
                EnemyBottom.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Player");
                EnemyBottom.GetComponent<Renderer>().sortingOrder = 4;
                EnemyBottom.GetComponent<MoveObstacleScript>().currentLane = 3;
                EnemyBottom.GetComponent<MoveObstacleScript>().setObstacleName(templateName);
                EnemyBottom.tag = tag;
                waveObjects.Add(EnemyBottom);
            }
        }
    }

    public void PlayerCollided(GameObject whammo) {

        
        if(whammo.tag == "Pickup") {
            // repairs can count as gold for sfx/score purpose
            _playerScript.PickupGold(1);
            if(whammo.GetComponent<MoveObstacleScript>().oName == "repair"){
                LifeUp();
            }
        }
        else if(whammo.tag == "Enemy" || whammo.tag == "Obstacle" || whammo.tag == "bullet"){
            if(_camShake){
                _camShake.Shake(camShakeAmt, 0.3f);
            }
            _playerScript.TakeDamage(50);
            ChangeSpeed(_repairShipSpeed);
            _repairTimer = _repairWait;
            Wellerman.GetComponent<AudioSource>().pitch = 0.98f;
            lifesUntilPirates -= 1;
            if(lifesUntilPirates < 1){
                GameOver();
            } else {
                SpawnHorde();
            }
        }
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
                            
                            cloud.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Player");
                            cloud.GetComponent<Renderer>().sortingOrder = 1;
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

    public void ResetGame() {
        pirateCountdown = pirateStartTime;
        isPaused = 1;
        distanceCountdown = distanceStart;
        foreach(GameObject obj in waveObjects){
            Destroy(obj);
        }
        waveObjects = new List<GameObject>{};
        foreach(GameObject obj in enemyHorde){
            Destroy(obj);
        }
        enemyHorde = new List<GameObject>{};
        _playerScript.health = 100;
        DisplayMenu();
    }

    public void NextStage() {
        //clear wave - slight pause for victory sound - then start next wave
        Wellerman.GetComponent<AudioSource>().Stop();
        wellerManCountdown = 500f;
        timeTillWave = waveTimer + 2f;
        ClearWave();
        distanceStage++;
        ShowBigMessage("Stage "+distanceStage+" Cleared!", 5.0f);
        Debug.Log("Next stage.. big message should be showing??");
        // TODO:: intermission? bonus stages?
        wellerManCountdown = wellerManLength;
        ChangeSpeed(0.9f); //1 or greater automatically gets set to base
        LifeUp();
        _playerScript.PlayVictory();
        Invoke("ActuallyNextStage", 3f);
    }
    public void ActuallyNextStage() {
        _currentThemeIndex += 1; 
        if(_currentThemeIndex >= themeSongs.Length) _currentThemeIndex = 0;
        Wellerman.GetComponent<AudioSource>().Stop();
        Wellerman.GetComponent<AudioSource>().clip = themeSongs[_currentThemeIndex];
        wellerManLength = Wellerman.GetComponent<AudioSource>().clip.length;
        wellerManCountdown = wellerManLength;
        Wellerman.GetComponent<AudioSource>().Play();
        _playerScript.minSpeed = distanceStage * speedBasePerStage - 1f;
        _playerScript.minSpeed = distanceStage * speedBasePerStage + 1f;
        ChangeSpeed(distanceStage * speedBasePerStage);
    }

    //remove all current wave obstacles
    public void ClearWave(){

        foreach( GameObject waveObj in waveObjects) {
            Destroy(waveObj);
        }
        waveObjects.Clear();
    }

    public void PlayerRepaired(){
        Wellerman.GetComponent<AudioSource>().pitch = 1.0f;
        ChangeSpeed(currentBaseSpeed);
        playerObject.GetComponent<PlayerMoveScript>().Repaired();
    }

    public void Pause(){
        if(currentState == "playing"){
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
    }
    void Update()
    {


        // Pause
        // TODO:: use input mapping
        if(currentState == "playing" &&Input.GetKeyDown(KeyCode.LeftShift)){
            Pause();
        }

        if( currentState == "menu" || currentState == "menuToPlayTransition" 
            || (currentState == "playing" && isPaused < 1) ) {
                MoveClouds();
            }
        if( currentState == "GameWon" || currentState == "GameOver")
        {
            if( Input.GetKeyDown(KeyCode.Space) ) {
                ResetGame();
            }
        }

        if( currentState == "playing" && isPaused < 1) {
            
            distanceCounter += Time.deltaTime;
            float prevWellerManCount = wellerManCountdown;
            wellerManCountdown -= Time.deltaTime;
            // TODO:: support intermission between stages?
            //if song is finished - next stage, go faster
            if(wellerManCountdown < 0.02f){
                NextStage();
            } else {
                // check if we should do partial speedups
                float wQuarter = wellerManLength / stageSpeedBoostNum;
                if( (int) (prevWellerManCount / wQuarter) != (int) (wellerManCountdown / wQuarter ) ){
                    int miniStage = (int) stageSpeedBoostNum - (int) (wellerManCountdown / wQuarter );
                    currentBaseSpeed += miniStage * stageSpeedBoostSpeed;
                    Debug.Log("Speed going to "+currentBaseSpeed);
                    if(shipSpeed >= 1f){
                        ChangeSpeed(currentBaseSpeed);
                    }
                }
            }

            // TODO:: better UI ..
            if( playerObject) {
                GameObject.Find("Distance Text").GetComponent<UnityEngine.UI.Text>().text = 
                    "Distance: "+(int)distanceCounter+"km @ "+shipSpeed+" knots";
                GameObject.Find("BadTimer").GetComponent<UnityEngine.UI.Text>().text = 
                    "Pirates in "+(int)lifesUntilPirates+"";
            }

            if(shipSpeed == _repairShipSpeed) {
                _repairTimer -= Time.deltaTime;
                if(_repairTimer <= 0.0f) {
                    PlayerRepaired();
                    
                }
            }

            if(_clearBigMessageTimer > 1.0f) {
                _clearBigMessageTimer -= Time.deltaTime;
            }
            if(_clearBigMessageTimer != 0.0f) {
                _clearBigMessageTimer = 0.0f;
                GameObject.Find("BigMessage").GetComponent<UnityEngine.UI.Text>().text = "";
            }

            if(lifesUntilPirates < 1){
                GameOver();
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
                if(waveObj.transform.position.x < -2f){
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
