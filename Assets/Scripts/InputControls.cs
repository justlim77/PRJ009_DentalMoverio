﻿using UnityEngine;
using System.Collections;
using System;

//public class GestureDetectedEventArgs : EventArgs
//{
//    public TouchType TouchType { get; set; }
//}

public class InputControls : MonoBehaviour
{
    public delegate void GestureDetectedEventHandler(object source, GestureDetectedEventArgs e);
    public static event GestureDetectedEventHandler GestureDetected;

    [Header("Percentage")]
    [Range(1, 100)]
    [SerializeField]
    float dragRatio;                    //minimum distance for a swipe to be registered
    [SerializeField]
    float tapTimeWindow;                //time to indicate double-tap
    [SerializeField]
    float tapReactionTime;              //human reaction doubletap
    [SerializeField]
    float holdTime = 0.8f;              //hold time     
    private float touchDuration;        //accumulated held time
    private int tapCount;               //number of taps
    private Vector3 fp;                 //First touch position
    private Vector3 lp;                 //Last touch position
    float dragDistance;                 //minimum distance for a swipe to be registered
    private WaitForSeconds tapTimeWait;

    Touch touch;
    TouchType touchType = TouchType.None;

    bool swiping = false;
    Vector2 from, to;

    protected virtual void OnGestureDetected(TouchType type)
    {
        if (GestureDetected != null)
            GestureDetected(this, new GestureDetectedEventArgs() { TouchType = type });
    }

    void Start ()
    {
        dragDistance = Screen.height * dragRatio * 0.01f;   //dragDistance is 15% height of the screen
        tapTimeWait = new WaitForSeconds(tapTimeWindow);    //interval for registering 2nd tap
    }

    bool timerActive = false;
	void Update ()
    {
        if (timerActive)
            touchDuration += Time.deltaTime;

        //Check tap
        if (tapTimeWindow > 0)
            tapTimeWindow -= Time.deltaTime;
        else
            tapCount = 0;

        if (Input.GetButtonDown("Fire1"))   // TouchPhase.Began
        {
            timerActive = false;
            from = Input.mousePosition;
        }
        if (Input.GetButtonUp("Fire1")) //TouchPhase.Ended
        {
            //single/double tap
            if (tapTimeWindow > 0)
            {
                tapCount++;
                if(tapCount >= 2)
                    touchType = TouchType.DoubleTap;
            }
            else
            {
                tapCount = 1;
                touchType = TouchType.SingleTap;
                tapTimeWindow = tapReactionTime;
            }

            to = Input.mousePosition;

            if ((Mathf.Abs(to.x - from.x) > dragDistance))
            {
                if (to.x > from.x)
                    touchType = TouchType.Right;
                else
                    touchType = TouchType.Left;
            }
            else
            {//if not drag, check for single, double
                //if (touchDuration < tapTimeWindow)
                //{
                //    touchType = TouchType.DoubleTap;
                //    tapCount = 0;
                //}
            }

            //Reset parameters
            touchDuration = 0;

            // Fire GestureDetected event
            if (!touchType.Equals(TouchType.None))
                OnGestureDetected(touchType);
        }

        if (Input.GetButton("Fire1"))
        {
            touchDuration += Time.deltaTime;
        }

        //if(swiping == true && from == to)
        //{
        //    Debug.Log("not a swipe");
        //    swiping = false;
        //}
    }
}