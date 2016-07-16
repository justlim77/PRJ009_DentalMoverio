using UnityEngine;
using System.Collections;
using System;

public class GestureDetectedEventArgs : EventArgs
{
    public TouchType TouchType { get; set; }
}

public class TouchControls : MonoBehaviour
{
    public delegate void GestureDetectedEventHandler(object source, GestureDetectedEventArgs e);    
    public static event GestureDetectedEventHandler GestureDetected; 

    [Header("Percentage")]
    [Range(1, 100)]
    [SerializeField] float dragRatio;       //minimum distance for a swipe to be registered
    [SerializeField] float tapTimeWindow;   //time to indicate double-tap
    [SerializeField] float holdTime = 0.8f; //hold time     
    private float touchDuration;            //accumulated held time
    private int tapCount;                   //number of taps
    private Vector3 fp;                     //First touch position
    private Vector3 lp;                     //Last touch position
    float dragDistance;                     //minimum distance for a swipe to be registered
    private WaitForSeconds tapTimeWait;

    Touch touch;
    TouchType touchType = TouchType.None;

    protected virtual void OnGestureDetected(TouchType type)
    {
        if (GestureDetected != null)
            GestureDetected(this, new GestureDetectedEventArgs() { TouchType = type });
    }

    void Start()
    {
        dragDistance = Screen.height * dragRatio * 0.01f;   //dragDistance is 15% height of the screen
        tapTimeWait = new WaitForSeconds(tapTimeWindow);    // interval for registering 2nd tap
    }

    void Update()
    {
        if (Input.touchCount == 1) // user is touching the screen with a single touch
        {
            touch = Input.GetTouch(0); // get the touch

            touchDuration += touch.deltaTime;

            if (touch.phase == TouchPhase.Began) //check for the first touch
            {
                fp = lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved) // update the last position based on where they moved
            {
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Stationary)
            {
                //Check for hold
                //if (touchDuration >= holdTime)
                //{
                //    Debug.Log("Hold");
                //    touchType = TouchType.Hold;
                //}
            }
            else if (touch.phase == TouchPhase.Ended) //check if the finger is removed from the screen
            {
                lp = touch.position;  //last touch position. Ommitted if you use list

                //Check if drag distance is greater than 20% of the screen height
                if ((Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance) /*&& touchDuration < holdTime*/)
                {//It's a drag
                 //check if the drag is vertical or horizontal
                    if (Mathf.Abs(lp.x - fp.x) > Mathf.Abs(lp.y - fp.y))
                    {   //If the horizontal movement is greater than the vertical movement...
                        if ((lp.x > fp.x))  //If the movement was to the right)
                        {   //Right swipe
                            Debug.Log("Right Swipe");
                            touchType = TouchType.Right;
                        }
                        else
                        {   //Left swipe
                            Debug.Log("Left Swipe");
                            touchType = TouchType.Left;
                        }
                    }
                    else
                    {   //the vertical movement is greater than the horizontal movement
                        if (lp.y > fp.y)  //If the movement was up
                        {   //Up swipe
                            Debug.Log("Up Swipe");
                            touchType = TouchType.Up;
                        }
                        else
                        {   //Down swipe
                            Debug.Log("Down Swipe");
                            touchType = TouchType.Down;
                        }
                    }
                }   //end of drag check
                else if (touchDuration < holdTime)
                {
                    StartCoroutine(SingleOrDouble());
                }
            }//end of touchphase.ended
        }//if no touch detected
        else
        {
            touchDuration = 0.0f;
            touchType = TouchType.None;
        }

        // Fire GestureDetected event
        if(!touchType.Equals(TouchType.None) && !touchType.Equals(TouchType.SingleTap))
            OnGestureDetected(touchType);
    }

    IEnumerator SingleOrDouble()
    {
        yield return tapTimeWait;
        if (touch.tapCount == 1)
        {
            Debug.Log("Single tap");
            touchType = TouchType.SingleTap;
        }
        else if (touch.tapCount == 2)
        {
            StopAllCoroutines();
            Debug.Log("Double tap");
            touchType = TouchType.DoubleTap;
        }
    }
}

public enum TouchType
{
    None,
    Up,
    Down,
    Left,
    Right,
    SingleTap,
    DoubleTap,
    Hold
}
