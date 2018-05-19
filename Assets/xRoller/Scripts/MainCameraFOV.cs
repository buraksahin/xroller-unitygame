using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraFOV : MonoBehaviour {
    public float fixedHorizontalFOV = 50;
    void Awake(){
        GetComponent<Camera>().fieldOfView = 2 * Mathf.Atan(Mathf.Tan(fixedHorizontalFOV * Mathf.Deg2Rad * 0.5f) / GetComponent<Camera>().aspect) * Mathf.Rad2Deg;
    }
}
