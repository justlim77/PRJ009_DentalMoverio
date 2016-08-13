using UnityEngine;
using System.Collections;
using System;

public class GestureDetectedEventArgs : EventArgs
{
    public TouchType TouchType { get; set; }
}

public class InputControls : MonoBehaviour
{
    public delegate void GestureDetectedEventHandler(object source, GestureDetectedEventArgs e);
    public static event GestureDetectedEventHandler GestureDetected;

    public delegate void BackButtonPressedEventHandler(object source, EventArgs e);
    public static event BackButtonPressedEventHandler BackButtonPressed;

    public delegate void SettingButtonPressedEventHandler(object source, EventArgs e);
    public static event SettingButtonPressedEventHandler MenuButtonPressed;

    [Header("Button mapping")]
    public KeyCode FirstTouch = KeyCode.Mouse0;
    public KeyCode SecondTouch = KeyCode.Mouse1;
    public KeyCode ScrollRight = KeyCode.RightArrow;
    public KeyCode ScrollLeft = KeyCode.LeftArrow;
    public KeyCode ScrollUp = KeyCode.UpArrow;
    public KeyCode ScrollDown = KeyCode.DownArrow;
    public KeyCode Back = KeyCode.Escape;
    public KeyCode Menu = KeyCode.Menu;
    public KeyCode VolumeUp = (KeyCode)24;
    public KeyCode VolumeDown = (KeyCode)25;

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

    protected virtual void OnBackButtonPressed()
    {
        if (BackButtonPressed != null)
            BackButtonPressed(this, new EventArgs() { });
    }

    protected virtual void OnMenuButtonPressed()
    {
        if (MenuButtonPressed != null)
            MenuButtonPressed(this, new EventArgs() { });
    }

    void Start ()
    {
        dragDistance = Screen.height * dragRatio * 0.01f;   //dragDistance is 15% height of the screen
        tapTimeWait = new WaitForSeconds(tapTimeWindow);    //interval for registering 2nd tap
    }

    bool timerActive = false;
    public static bool Holding { get; private set; }

	void Update ()
    {
        /* For debugging key input
        //foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
        //{
        //    if (Input.GetKeyDown(kcode))
        //        Debugger.Instance.Log("KeyCode down: " + kcode.ToString());
        //}
        */

        if (timerActive)
            touchDuration += Time.deltaTime;

        //Check tap
        if (tapTimeWindow > 0)
            tapTimeWindow -= Time.deltaTime;
        else
            tapCount = 0;

        if (Input.GetKeyDown(FirstTouch))   // TouchPhase.Began
        {
            timerActive = false;
            from = Input.mousePosition;
            Holding = true;
        }
        if (Input.GetKeyUp(FirstTouch)) //TouchPhase.Ended
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

            //if ((Mathf.Abs(to.x - from.x) > dragDistance))
            //{
            //    if (to.x > from.x)
            //        touchType = TouchType.Right;
            //    else
            //        touchType = TouchType.Left;
            //}
            //else
            //{//if not drag, check for single, double
            //    //if (touchDuration < tapTimeWindow)
            //    //{
            //    //    touchType = TouchType.DoubleTap;
            //    //    tapCount = 0;
            //    //}
            //}

            //Reset parameters
            touchDuration = 0;

            // Fire GestureDetected event
            if (!touchType.Equals(TouchType.None))
                OnGestureDetected(touchType);

            Holding = false;    // release holding bool
        }

        if (Input.GetKey(FirstTouch))
        {
            touchDuration += Time.deltaTime;
        }

        if (Input.GetKeyDown(ScrollRight))
        {
            touchType = TouchType.Right;
        }

        if (Input.GetKeyDown(ScrollLeft))
        {
            touchType = TouchType.Left;
        }

        if (Input.GetKeyDown(Back))
        {
            OnBackButtonPressed();
        }

        if (Input.GetKeyDown(Menu))
        {
            OnMenuButtonPressed();
        }

        //if(swiping == true && from == to)
        //{
        //    Debug.Log("not a swipe");
        //    swiping = false;
        //}
    }
}
