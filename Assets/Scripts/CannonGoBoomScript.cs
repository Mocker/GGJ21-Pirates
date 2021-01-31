using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonGoBoomScript : MonoBehaviour
{
    public float speedX = 3f;
    public float endDuration = 3f;
    public float duration = 0f;
    public float heightY = 3f;
    public float startY;
    public Vector3[] points = {
        Vector3.one,
        Vector3.one,
        Vector3.one
    };

    public bool isActive = true;
    // Start is called before the first frame update
    void Start()
    {
        points[0] = transform.position;
        points[2] = new Vector3( transform.position.x+speedX, transform.position.y-1f, transform.position.z);
        points[1] = points[0] +(points[2] -points[0])/2 +Vector3.up *heightY; // Play with 5.0 to change the curve

    }

    // Update is called once per frame
    void Update()
    {
        if(isActive){
            if (duration < endDuration) {
                duration += Time.deltaTime;

                Vector3 lerpedX = Vector3.Lerp( points[0], points[2], (duration/endDuration) );
                lerpedX.y = lerpedX.y + ((float)Mathf.Sin(duration*1f) * heightY);
                transform.position = lerpedX;
                //Vector3 m1 = Vector3.Lerp( points[0], points[1], duration );
                //Vector3 m2 = Vector3.Lerp( points[1], points[2], duration );
                //transform.position = Vector3.Lerp(m1, m2, (duration/endDuration));
            } else {
                Destroy(this);
            }
        }
    }
}
