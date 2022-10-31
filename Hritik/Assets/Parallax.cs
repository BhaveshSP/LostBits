using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
       
        PlayerScript hinge = GetComponent("PlayerScript") as PlayerScript;

        if (hinge != null)
            hinge.useSpring = false;

       

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
