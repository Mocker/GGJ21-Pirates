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
    void Start()
    {

    }

    public void StartMovingIt( Vector3 dest, Vector3 destScale, float duration) {
        elapsedTime = 0f; destTime = duration;
        endScale = destScale;
        endX = dest;
        startX = transform.position;
        startScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if(moveIt) {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startX, endX, (elapsedTime / destTime));
            transform.localScale = Vector3.Lerp(startScale, endScale, (elapsedTime / destTime));
        }   
    }
}
