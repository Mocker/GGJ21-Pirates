using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobbingAnimationScript : MonoBehaviour
{
    public Vector2 floatY;
    public float originalY;

    public float floatStrength = 0.4f;
    public float floatSpeed = 2.0f;
    // Start is called before the first frame update
    void Start()
    {
        this.originalY = this.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(transform.position.x,
            originalY + ((float)Mathf.Sin(Time.time*floatSpeed) * floatStrength),
            transform.position.z);
    }
}
