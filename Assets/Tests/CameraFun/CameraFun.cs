using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFun : MonoBehaviour {

    public Camera cam;
    // Use this for initialization
    void Start () {
	    
    }
	
    // Update is called once per frame
    void Update () {
	Debug.Log ( cam.projectionMatrix );
    }
}
