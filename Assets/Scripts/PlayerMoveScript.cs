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

    internal Transform thisTransform;
    public float speed = 2.0f;
    public float acceleration = 0.1f;
    public float minSpeed = 1.0f;
    public float maxSpeed = 3.0f;
    private Renderer _myRenderer;
    private BobbingAnimationScript _bobbingScript;
    private Animator _animator;

    public GameObject _gameObj;
    private GameStartScript _gameController;
    void Start()
    {
        thisTransform = this.transform;
        _myRenderer = GetComponent<Renderer>();
        _bobbingScript = GetComponent<BobbingAnimationScript>();    
        _animator = GetComponent<Animator>();
        _gameController = _gameObj.GetComponent<GameStartScript>();
    }

    public void Fire()
    {
        if( health > 50 )
        {
			_animator.Play("Base Layer.PlayerDefaultFiring");
        } else {
            _animator.Play("Base Layer.PlayerDamagedFiring");
        }
    }

    public void IncrementSpeed( float inc) 
    {
        float newSpeed = speed + inc ;
        if(newSpeed < maxSpeed && newSpeed > minSpeed) {
            speed = newSpeed;
            _gameController.ChangeSpeed(speed);
        }
    }

    public void Repaired()
    {
        health = 100;
        _animator.Play("Base Layer.PlayerShipIdle");
    }

    // Update is called once per frame
    void Update()
    {
        
		if (Input.GetKeyDown(KeyCode.UpArrow)){
            if(_currentWave > 1) {
                _currentWave--;
                transform.position =  new Vector3(transform.position.x,  _wavePositions[ _currentWave-1], transform.position.z);
                _myRenderer.sortingOrder = 1 + _currentWave;
                _bobbingScript.originalY = transform.position.y;
            }
			
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)){
			if(_currentWave < 3) {
                _currentWave++;
                transform.position = new Vector3(transform.position.x,  _wavePositions[ _currentWave-1], transform.position.z);
                 _myRenderer.sortingOrder = 1 + _currentWave;
                 _bobbingScript.originalY = transform.position.y;
            }
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
