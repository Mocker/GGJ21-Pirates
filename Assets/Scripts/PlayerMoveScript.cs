using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveScript : MonoBehaviour
{
    // Start is called before the first frame update
    public int _currentWave = 2;

    public float[] _wavePositions = {
        0.2f,
        -0.65f,
        -2f
    };

    public int health = 100; //would like to just flag as damaged or not but can use an internal int for now
    public int gold = 0, obstaclesSunk = 0; // can have random spawned gold pickups as score multipliers
    public AudioClip sfxGold;
    public AudioClip sfxDamage;
    public AudioClip sfxVictory;

    public AudioClip sfxCannon;

    public AudioClip sfxSink;

    public AudioClip sfxSwitchLanes;

    internal Transform thisTransform;
    public float speed = 2.0f;
    public float acceleration = 0.5f;
    public float minSpeed = 1.0f;
    public float maxSpeed = 3.0f;

    public float cannonCooldown = 1.5f, cannonTimer = 0.0f;
    private Renderer _myRenderer;
    private BobbingAnimationScript _bobbingScript;
    private Animator _animator;

    public GameObject _gameObj;
    private GameStartScript _gameController;
    private AudioSource _audioSource;
    void Start()
    {
        thisTransform = this.transform;
        _myRenderer = GetComponent<Renderer>();
        _bobbingScript = GetComponent<BobbingAnimationScript>();    
        _animator = GetComponent<Animator>();
        _gameController = _gameObj.GetComponent<GameStartScript>();
        _audioSource = GetComponent<AudioSource>();
        gold = 0;
    }

    public void Fire()
    {
        if(cannonTimer > 0.0f) return;
        cannonTimer = cannonCooldown;
        if( health > 50 )
        {
			_animator.Play("Base Layer.PlayerDefaultFiring");
        } else {
            _animator.Play("Base Layer.PlayerDamagedFiring");
        }
        if(sfxCannon) {
            _audioSource.clip = sfxCannon;
            _audioSource.Play();
        }
        GameObject ball = Instantiate(GameObject.Find("Cannonball"), transform.position, Quaternion.identity);
        ball.SetActive(true);
        ball.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Player");
        ball.GetComponent<Renderer>().sortingOrder = 5;
        ball.GetComponent<CannonGoBoomScript>().isActive = true;
        ball.GetComponent<CannonGoBoomScript>().PlayerScript = this;
        ball.GetComponent<CannonGoBoomScript>().SetStats(
            true, 0.8f, 1f, 3f, 4.5f
        );
        ball.GetComponent<CannonGoBoomScript>().CurrentLane = _currentWave;
    }


    public void IncrementSpeed( float inc) 
    {
        float newSpeed = speed + inc ;
        if(newSpeed < maxSpeed && newSpeed > minSpeed) {
            speed = newSpeed;
            _gameController.ChangeSpeed(speed);
        }
    }

    public void PickupGold( int goldAmount)
    {
        gold += goldAmount;
        if(sfxGold){
            _audioSource.clip = sfxGold;
            _audioSource.Play();
        }
    }

    public bool CannonInLane(int lane, GameObject ball){
        if(lane != _currentWave) return false;
        _gameController.PlayerCollided(ball);
        return true;
    }

    public void DestroyedEnemy( GameObject enemy) {
        Debug.Log("Destroyed enemy ship!");
        obstaclesSunk += 1;
        if(sfxSink) {
            _audioSource.clip = sfxSink;
            _audioSource.Play();
        }
        _gameController.waveObjects.Remove(enemy);
        Destroy(enemy);
    }

    public void TakeDamage(int dmgAmount)
    {
        // TODO:: do we care about dmg/health?
        // health -= dmgAmount
        // if(health < 1) do bad thing
        if(sfxDamage){
            _audioSource.clip = sfxDamage;
            _audioSource.Play();
        }
    }

    public void PlayVictory()
    {
        // TODO:: can add an animation or fire off a billion cannons or fireworks or something
        if(sfxVictory){
            _audioSource.clip = sfxVictory;
            _audioSource.Play(); 
        }
    }

    public void Repaired()
    {
        health = 100;
        _animator.Play("Base Layer.PlayerShipIdle");
    }

    public void MoveUp(){
        if(_currentWave > 1) {
                if(sfxSwitchLanes) {
                    _audioSource.clip = sfxSwitchLanes;
                    _audioSource.Play();
                }
                _currentWave--;
                transform.position =  new Vector3(transform.position.x,  _wavePositions[ _currentWave-1], transform.position.z);
                _myRenderer.sortingOrder = 1 + _currentWave;
                _bobbingScript.originalY = transform.position.y;
            }
    }

    public void MoveDown(){
        if(_currentWave < 3) {
                if(sfxSwitchLanes) {
                    _audioSource.clip = sfxSwitchLanes;
                    _audioSource.Play();
                }
                _currentWave++;
                transform.position = new Vector3(transform.position.x,  _wavePositions[ _currentWave-1], transform.position.z);
                 _myRenderer.sortingOrder = 1 + _currentWave;
                 _bobbingScript.originalY = transform.position.y;
            }
    }

    // Update is called once per frame
    void Update()
    {
        if(cannonTimer > 0.0f) cannonTimer -= Time.deltaTime;

		if (Input.GetKeyDown(KeyCode.UpArrow)){
            MoveUp();			
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)){
			MoveDown();
		}

        if (Input.GetKeyDown(KeyCode.Space)){
            Fire();
		}
    }

    private void OnTriggerEnter2D(Collider2D collision)
    { Debug.Log("OnTriggerEnter2D");
        
    }

    public void OnCollidedObstacle(GameObject collision) {
        if (collision.gameObject.tag == "Obstacle")
        {
            health = 25;
            _animator.Play("Base Layer.PlayerDamaged");
        }
    }

    private void OnTriggerEnter(Collider collider){
        Debug.Log("Collided trigger enter");
    }
}
