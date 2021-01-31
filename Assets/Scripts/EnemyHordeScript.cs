using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHordeScript : MonoBehaviour
{
    // Start is called before the first frame update
    public bool moveIt = false;
    public Vector3 endScale, endX; // = 3.0f;
    //public float endX = 7f;
    public Vector3 startScale, startX;
    public float elapsedTime, destTime;

    private Animator _anim;
    void Start()
    {
        _anim = GetComponent<Animator>();
    }

    public void StartMovingIt( Vector3 dest, Vector3 destScale, float duration) {
        elapsedTime = 0f; destTime = duration;
        endScale = destScale;
        endX = dest;
        startX = transform.position;
        startScale = transform.localScale;
        moveIt = true;
    }


    public void MoveAStep( int progress ) {
        if(moveIt) {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startX, endX, (progress/ destTime));
            transform.localScale = Vector3.Lerp(startScale, endScale, (progress / destTime));
        }   
    }

    // Update is called once per frame
    void Update()
    {
        if(moveIt) {
            float fireTheCannons = Random.Range(0.0f, 1.0f);
            if(fireTheCannons < 0.02f && _anim.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.EnemyShipIdle")){
                _anim.Play("Base Layer.EnemyShipFiring");
                GameObject ball = Instantiate(GameObject.Find("Cannonball"), transform.position, Quaternion.identity);
                ball.SetActive(true);
                ball.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Player");
                ball.GetComponent<Renderer>().sortingOrder = 5;
                ball.GetComponent<CannonGoBoomScript>().isActive = true;
            }
        }
    }
}
