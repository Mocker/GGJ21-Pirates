using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTextureScript : MonoBehaviour
{
    public int isPaused = 0;

    public float scrollSpeed = 2f;
    private Vector2 _savedOffset;

    public float maxOffset = 15.0f;

    private Renderer _myRenderer;
    private float _baseScrollSpeed;
    public float _playerSpeed = 2f;
    // Start is called before the first frame update
    void Start()
    {
        _myRenderer = GetComponent<Renderer>();
        _savedOffset = _myRenderer.material.mainTextureOffset;
        _baseScrollSpeed = scrollSpeed;
    }

    public void ChangeSpeed( float newSpeed =1.0f) 
    {
        _playerSpeed = newSpeed / 1.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if(isPaused < 1) {
            float x = Mathf.Repeat (Time.time * (scrollSpeed*_playerSpeed) , 0.5f); //max offset should leave enough room to show complete wave tile
            Vector2 offset = new Vector2(x, _savedOffset.y);
            _myRenderer.material.mainTextureOffset = offset;
        }
    }
}
