using MoodMe;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


public class AutoRotate:MonoBehaviour {
    public int WaitCounter = 3;
	// Use this for initialization
	void Start () {
#if UNITY_ANDROID&&!UNITY_EDITOR
        if (gameObject.name == "Webcam Plane")
        {
            transform.localRotation=Quaternion.Euler(0, -90, 90);
        }
        else
        {
            transform.localPosition = new Vector3(-320, -240, 1);
            transform.localScale = new Vector3(1, -1, 1);
        }

#endif      
#if UNITY_IOS && !UNITY_EDITOR
        if (gameObject.name == "Webcam Plane")
        {
            transform.localRotation=Quaternion.Euler(0, 90, -90);
            GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(-1, 1);
        }
        else
        {
            transform.localPosition = new Vector3(-320, -240, 1);
            transform.localScale = new Vector3(1, -1, 1);
        }

#endif        


        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }


    // Update is called once per frame
    void Update () {
        
       

    }
}
