using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    public GameObject helpCanvas;
    // Start is called before the first frame update
    void Start()
    {
    }


    public void ShowHelp() {
        Debug.Log("Show Help");
        if(helpCanvas){
            helpCanvas.SetActive(true);
        }
    }

    public void HideHelp() {
        Debug.Log("hide help");
        if(helpCanvas){
            helpCanvas.SetActive(false);
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
