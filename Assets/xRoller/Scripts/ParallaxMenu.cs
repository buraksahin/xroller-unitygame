using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ParallaxMenu : MonoBehaviour {

    // Variables
    public RectTransform canvas;
    public float smoothTime = 0.3f;
    public float[] minMaxXAxis = new float[2]; //0 min, 1 max
    public float[] minMaxYAxis = new float[2]; //0 min, 1 max
    public float moveSensivity = 900.0f;
    private float xVelocity = 0.0f;
    private float yVelocity = 0.0f;
    private float x;
    private float y;
    private float xSmooth;
    private float ySmooth;

    void Awake(){
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = true;
    }

    void Update()
    {
        x -= Input.acceleration.x * moveSensivity * .2f;

        if(y > minMaxYAxis[1]) y = minMaxYAxis[1];
        if(y < minMaxYAxis[0]) y = minMaxYAxis[0];
        if(x > minMaxXAxis[1]) x = minMaxXAxis[1];
        if(x < minMaxXAxis[0]) x = minMaxXAxis[0];

        xSmooth = Mathf.SmoothDamp(xSmooth, x, ref xVelocity, smoothTime);

        Quaternion parallax = new Quaternion();
        parallax = Quaternion.Euler(ySmooth / 7,xSmooth/7,0);

        canvas.transform.localPosition = new Vector3(xSmooth, 0f, 0f);
        canvas.transform.localRotation = parallax;
    }
}
