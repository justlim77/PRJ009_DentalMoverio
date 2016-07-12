using UnityEngine;
using System.Collections;

public class TouchControls : MonoBehaviour
{
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

    Touch touch;
    TouchType touchType = TouchType.None;

    void Start()
    {
        dragDistance = Screen.height * dragRatio / 100; //dragDistance is 15% height of the screen
    }

    void Update()
    {
        if (Input.touchCount == 1) // user is touching the screen with a single touch
        {
            touch = Input.GetTouch(0); // get the touch

            touchDuration += touch.deltaTime;

            if (touch.phase == TouchPhase.Began) //check for the first touch
            {
                fp = touch.position;
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Moved) // update the last position based on where they moved
            {
                lp = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended) //check if the finger is removed from the screen
            {
                if (touchDuration > 0.2f)
                {
                    StartCoroutine(SingleOrDouble());
                }

                lp = touch.position;  //last touch position. Ommitted if you use list

                //Check if drag distance is greater than 20% of the screen height
                if (Mathf.Abs(lp.x - fp.x) > dragDistance || Mathf.Abs(lp.y - fp.y) > dragDistance)
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
                }
                else
                {
                    //It's a tap as the drag distance is less than 20% of the screen height
                    Debug.Log("Tap");
                    touchType = TouchType.SingleTap;
                }
            }

            //Check for hold
            if (touchDuration >= holdTime)
            {
                Debug.Log("Hold");
                touchType = TouchType.Hold;
            }
        }
        else
        {
            touchDuration = 0.0f;
            touchType = TouchType.None;
        }
    }

    IEnumerator SingleOrDouble()
    {
        yield return new WaitForSeconds(0.3f);
        if (touch.tapCount == 1)
        {
            Debug.Log("Single tap");
            touchType = TouchType.SingleTap;
        }
        else if (touch.tapCount == 2)
        {
            StopCoroutine(SingleOrDouble());
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
