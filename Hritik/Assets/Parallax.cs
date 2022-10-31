using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    float depth = 1;
    PlayerScript playerScript;
    // Start is called before the first frame update
    private void Awake()
    {
        playerScript = GameObject.Find("PlayerScript").GetComponent<PlayerScript>();
    }
    void Start()
    {
        

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float realVelocity = playerScript.velocity.x / depth;
        Vector2 pos = transform.position;

        pos.x -= realVelocity *Time.fixedDeltaTime; 

        transform.position = pos;
        
    }
}
