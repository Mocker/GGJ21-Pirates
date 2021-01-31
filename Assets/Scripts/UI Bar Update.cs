using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBarUpdate : MonoBehaviour
{
    // Need to edit GameStartScript to implement.
    // -Add ref to this script
    // -Call SetSpeed and SetMaxSpeed accordingly

    public GameObject speedBar;
    public Slider speedSlider;

    // Start is called before the first frame update
    void Start()
    {
        speedBar = GameObject.Find("Speed Bar");
    }

    public void SetMaxSpeed(float maxSpeed, float speed)
    {
        speedSlider.maxValue = maxSpeed;
        speedSlider.value = speed;
    }

    public void SetSpeed(float speed)
    {
        speedSlider.value = speed;
    }
}
