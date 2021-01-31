using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObstacleScript : MonoBehaviour
{
    public int currentLane = 1;
    public string oName = "Rocks";
    // Start is called before the first frame update
    void Start()
    {
        
        
    }

    public void setObstacleName(string newName){
        oName = newName;
        Debug.Log("obstacle name: "+oName);
        if(oName == "repair") {
            Debug.Log("Play repair");
            this.gameObject.GetComponent<Animator>().Play("Base Layer.PickupRepair");
        }
    }

    // Update is called once per frame
    void Update()
    {
       
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Name of the object: " + other.gameObject.name);
    }
}
