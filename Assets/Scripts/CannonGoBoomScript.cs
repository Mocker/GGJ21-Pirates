using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonGoBoomScript : MonoBehaviour
{
    public float speedX = 3f;
    public float endDuration = 3f;
    public float duration = 0f;
    public float durationMultiplier = 2f;
    public float heightY = 1.2f;
    public float startY;
    public Vector3[] points = {
        Vector3.one,
        Vector3.one,
        Vector3.one
    };

    public bool CanHurtPlayer = false, IsPlayers = false;
    public int CurrentLane = 1;
    public PlayerMoveScript PlayerScript;

    public bool isActive = true;
    // Start is called before the first frame update
    void Start()
    {
        points[0] = transform.position;
        points[2] = new Vector3( transform.position.x+speedX, transform.position.y-1f, transform.position.z);
        points[1] = points[0] +(points[2] -points[0])/2 +Vector3.up *heightY; // Play with 5.0 to change the curve

    }

    public void SetStats(bool isPlayers, float height, float end, float multiplier, float speed) {
        IsPlayers = isPlayers; 
        heightY = height; 
        endDuration=end; 
        durationMultiplier = multiplier;
        speedX = speed;
    }

    // Update is called once per frame
    void Update()
    {
        if(isActive){
            if (duration < endDuration) {
                duration += Time.deltaTime;

                Vector3 lerpedX = Vector3.Lerp( points[0], points[2], (duration/endDuration) );
                lerpedX.y = lerpedX.y + ((float)Mathf.Sin((duration*durationMultiplier)/endDuration) * heightY);
                transform.position = lerpedX;
                
                // if this is an enemy cannon ball check if it hit the player
                // TODO:: magic numbers for player intersection :()
                if(PlayerScript && CanHurtPlayer && lerpedX.x > -0.2f && lerpedX.x < 1.1f){
                    if( PlayerScript.CannonInLane(CurrentLane, this.gameObject) ) {
                        //returns true on hit
                        Destroy(this.gameObject);
                    }
                } else if(PlayerScript && IsPlayers) {
                    // TODO:: check if it hits enemy ship
                    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                    for(int i=0; i<enemies.Length; i++){
                        if( enemies[i].GetComponent<MoveObstacleScript>().currentLane != CurrentLane) {
                            continue;
                        }
                        //only care about x position since we are tracking them by lane
                        if(lerpedX.x > enemies[i].transform.position.x-1f && lerpedX.x < enemies[i].transform.position.x+1f){
                            //Hit!
                            PlayerScript.DestroyedEnemy(enemies[i]);
                            Destroy(this.gameObject);
                            break;
                        }
                    }
                }

            } else {
                Destroy(this.gameObject);
            }
        }
    }
}
