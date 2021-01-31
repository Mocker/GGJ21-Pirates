using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShakeScript : MonoBehaviour
{
    public Camera mainCam;

    public float shakeAmount = 0;

    private Vector3 _startingPos;
    void Awake() {
        if(mainCam == null)
            mainCam = Camera.main;
        _startingPos = mainCam.transform.position;
    }

    void BeginShake()
    {
        if(shakeAmount > 0) {
            Vector3 camPos = mainCam.transform.position;
            float shakeAmtX = Random.value * shakeAmount * 2 - shakeAmount;
            float shakeAmtY = Random.value * shakeAmount * 2 - shakeAmount;
            camPos.x += shakeAmtX;
            camPos.y += shakeAmtY;

            mainCam.transform.position = camPos;
        }
    }

    void StopShake()
    {
        CancelInvoke("BeginShake");
        mainCam.transform.localPosition = _startingPos;
    }

    public void Shake (float amt, float length)
    {
        shakeAmount = amt;
        InvokeRepeating("BeginShake", 0, 0.01f);
        Invoke("StopShake", length);
    }

}
