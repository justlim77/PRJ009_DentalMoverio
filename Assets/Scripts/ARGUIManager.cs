using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;

public class PanelChangedEventArgs : EventArgs
{
    public PanelType PanelType { get; set; }
}

public class ARGUIManager : MonoBehaviour
{
    public delegate void PanelChangedEventHandler(object sender, PanelChangedEventArgs e);
    public static event PanelChangedEventHandler PanelChanged;
    public static ARGUIManager Instance { get; private set; }

    [SerializeField] ARGUIPanel[] ARGUIPanels;
    [Header("Content Panels")]
    [SerializeField] ARGUIPanel FacialPanel;
    [SerializeField] ARGUIPanel RadiographPanel;
    [SerializeField] ARGUIPanel VideoPanel;
    [SerializeField] ARGUIPanel CameraPanel;
    [SerializeField] ARGUIPanel HomePanel;
    [SerializeField] GameObject MidPanel;
    [SerializeField] ARGUIPanel MutePanel;
    [Header("Buttons")]
    [SerializeField] Button btnFacial;
    [SerializeField] Button btnRadiograph;
    [SerializeField] Button btnVideo;
    [SerializeField] Button btnCamera;
    [SerializeField] Button btnMenu;
    [Header("General")]
    [SerializeField] Button btnLoad;
    [SerializeField] GameObject TopBar;
    [SerializeField] GameObject BotBar;
    [SerializeField] float panelSlideSpeed = 5.0f;
    [Header("Camera")]
    [SerializeField] Image cameraPlane;
    [SerializeField] Resolution resolution;
    [SerializeField] Button btnSnapshot;
    [SerializeField] string phoneImagePath;

    WebCamDevice device;
    WebCamTexture camTex;
    CanvasGroup _topBarCanvasGroup, _botBarCanvasGroup;
    RectTransform _midPanelRectTrans, _homePanelRectTrans;
    int _currentPanelIdx;

    protected virtual void OnPanelChanged(PanelType type)
    {
        if (PanelChanged != null)
            PanelChanged(this, new PanelChangedEventArgs() { PanelType = type });
    }

    void Awake()
    {
        if(Instance == null)
            Instance = this;

        Core.SubscribeEvent("OnPanelOpened", OnPanelOpened);
        Core.SubscribeEvent("OnToggleBars", OnBarsToggled);
    }

    void Start ()
    {
        btnMenu.onClick.AddListener(() => HomePanel.OpenPanel());
        btnMenu.onClick.AddListener(() => OnBarsToggled(this, false));

        btnLoad.onClick.AddListener(() => StartApp());

        btnFacial.onClick.AddListener(() => FacialPanel.OpenPanel());
        btnFacial.onClick.AddListener(() => StopFeed());

        btnRadiograph.onClick.AddListener(() => RadiographPanel.OpenPanel());
        btnRadiograph.onClick.AddListener(() => StopFeed());

        btnVideo.onClick.AddListener(() => VideoPanel.OpenPanel());
        btnVideo.onClick.AddListener(() => StopFeed());

        btnCamera.onClick.AddListener(() => CameraPanel.OpenPanel());
        btnCamera.onClick.AddListener(() => LaunchFeed());

        btnSnapshot.onClick.AddListener(() => SnapShot());

        //TouchControls.GestureDetected += OnGestureDetected;
        InputControls.GestureDetected += OnGestureDetected;     //Moverio 4.5 ~ 4.6 with Input.GetMouse events

        if (TopBar != null)
        { 
            _topBarCanvasGroup = TopBar.GetComponent<CanvasGroup>();
            _topBarCanvasGroup.alpha = 0;
        }

        if (BotBar != null)
        {
            _botBarCanvasGroup = BotBar.GetComponent<CanvasGroup>();
            _botBarCanvasGroup.alpha = 0;   
        }

        try
        {
            device = WebCamTexture.devices[0];
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.Log(e.Message);
        }

        if(device.name != null)
            camTex = new WebCamTexture(device.name, resolution.width, resolution.height);

        cameraPlane.material.mainTexture = camTex;

        HomePanel.transform.SetAsLastSibling();
        MutePanel.transform.SetAsLastSibling();

        //HomePanel.OpenPanel();
        _homePanelRectTrans = HomePanel.GetComponent<RectTransform>();
        _homePanelRectTrans.anchoredPosition = Vector2.zero;            //Set home panel to center

        MutePanel.SetActive(false);
        MutePanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;    //Set mute panel to center

        //Setup scrollable mid panel
        _midPanelRectTrans = MidPanel.GetComponent<RectTransform>();
        MidPanel.GetComponent<ScrollRect>().enabled = false;
    }

    private void OnGestureDetected(object source, GestureDetectedEventArgs e)
    {
        Debug.Log(e.TouchType.ToString() + " detected.");

        switch (e.TouchType)
        {
            case TouchType.Left:
                NextPanel();
                break;
            case TouchType.Right:
                PreviousPanel();
                break;
            case TouchType.DoubleTap:
                MoverioCameraController controller = MoverioCameraController.Instance;
                bool muted = controller.GetCurrentMuteState();
                CanvasMuteType = !CanvasMuteType;
                controller.SetCurrentMuteType(!muted);
                break;
        }
    }

    bool _canvasMuteType = false;
    public bool CanvasMuteType
    {
        get {
            return _canvasMuteType;
        }
        set {
            _canvasMuteType = value;

            MutePanel.SetActive(value);
        }
    }

    bool _started = false;
    void StartApp()
    {
        ShowBar(_topBarCanvasGroup);
        ShowBar(_botBarCanvasGroup);

        //Refresh automatic layout for images
        FacialPanel.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = false;
        FacialPanel.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = true;
        RadiographPanel.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = false;
        RadiographPanel.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = true;

        //HomePanel.SetActive(false);
        _homeTargetPos = HomePanel.GetInitialPosition();

        MidPanel.GetComponent<ScrollRect>().enabled = true;

        _started = true;
    }

    void LaunchFeed()
    {
        if (camTex != null)
            if (!camTex.isPlaying)
                camTex.Play();
    }

    void StopFeed()
    {
        if(camTex != null)
            if(camTex.isPlaying)
                camTex.Stop();
    }

    void OnDisable()
    {
        btnMenu.onClick.RemoveAllListeners();
        btnFacial.onClick.RemoveAllListeners();
        btnRadiograph.onClick.RemoveAllListeners();
        btnVideo.onClick.RemoveAllListeners();
        btnCamera.onClick.RemoveAllListeners();

        TouchControls.GestureDetected -= OnGestureDetected;

        Core.UnsubscribeEvent("OnOpenPanel", OnPanelOpened);
    }

    object OnPanelOpened(object sender, object args)
    {
        ARGUIPanel panel;
        if (args is ARGUIPanel)
        {
            panel = (ARGUIPanel)args;

            foreach (var ARPanel in ARGUIPanels)
            {
                //ARPanel.SetActive(false);
            }

            panel.SetActive(true);

            switch (panel.panelType)
            {
                case PanelType.Camera:
                    _targetPos = panel.GetInversedInitialPos();
                    LaunchFeed();
                    break;
                case PanelType.Facial:
                case PanelType.Radiograph:
                case PanelType.Video:
                    StopFeed();
                    _targetPos = panel.GetInversedInitialPos();
                    break;
                case PanelType.Home:
                    StopFeed();
                    _homeTargetPos = Vector2.zero;
                    break;
            }

            _currentPanelIdx = Array.IndexOf(ARGUIPanels, panel);
            Debug.Log("Current panel index: " + _currentPanelIdx);

            string msg = panel.panelType.ToString();
            Core.BroadcastEvent("OnUpdateHeader", this, msg);
        }

        return null;
    }

    Vector2 _targetPos, _homeTargetPos;
    void Update()
    {
        //Slide content panels
        if (!InputControls.Holding)
        {
            Vector2 currentPos = _midPanelRectTrans.anchoredPosition;
            if (currentPos != _targetPos)
            {
                Vector2 lerpToPos = Vector2.Lerp(currentPos, _targetPos, panelSlideSpeed * Time.deltaTime);
                _midPanelRectTrans.anchoredPosition = lerpToPos;
            }
        }

        //Slide home panel
        Vector2 homeCurrentPos = _homePanelRectTrans.anchoredPosition;
        if (homeCurrentPos != _homeTargetPos)
        {
            Vector2 lerpToPos = Vector2.Lerp(homeCurrentPos, _homeTargetPos, panelSlideSpeed * Time.deltaTime);
            _homePanelRectTrans.anchoredPosition = lerpToPos;
        }

        //Button mapping
        if (Input.GetKeyDown(KeyCode.Menu)/* || Input.GetKeyDown(KeyCode.Return)*/)
        {
            HomePanel.OpenPanel();
            OnBarsToggled(this, false);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PreviousPanel();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousPanel();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextPanel();
        }
    }

    object OnBarsToggled(object sender, object args)
    {
        if (args is bool)
        {
            bool val = (bool)args;

            ShowBar(_topBarCanvasGroup, val);
            ShowBar(_botBarCanvasGroup, val);
        }

        return null;
    }

    void ShowBar(CanvasGroup cg, bool show = true)
    {
        if (cg == null)
            return;

        cg.alpha = show ? 1 : 0;
    }

    void SnapShot()
    {
        ScreenCapture capture = GetComponent<ScreenCapture>();

        string filePath;
        string subPath;
#if UNITY_ANDROID && !UNITY_EDITOR
        subPath = phoneImagePath;
        filePath = subPath + capture.GetFileName(resolution.width, resolution.height);
#elif UNITY_EDITOR
        subPath = Application.dataPath + "/Screenshots/";
        if (!Directory.Exists(subPath))
            Directory.CreateDirectory(subPath);
        filePath = subPath + capture.GetFileName(resolution.width, resolution.height);
#endif
        Debug.Log("File path: " + filePath);
        capture.SaveScreenshot(CaptureMethod.ReadPixels_Asynch, filePath);
    }

    void NextPanel()
    {
        if (!_started)
            return;

        int nextIdx = _currentPanelIdx;
        nextIdx++;
        if (nextIdx > ARGUIPanels.Length - 1)
        {
            nextIdx = ARGUIPanels.Length - 1;
            return;
        }

        ARGUIPanel panel = ARGUIPanels[nextIdx];
        if (panel != null)
        {
            OnPanelOpened(this, panel);
            PanelType panelType = panel.panelType;
            OnPanelChanged(panelType);
        }
    }
    void PreviousPanel()
    {
        if (!_started)
            return;

        int nextIdx = _currentPanelIdx;
        nextIdx--;
        if (nextIdx < 0)
        {
            nextIdx = 0;
            return;
        }

        ARGUIPanel panel = ARGUIPanels[nextIdx];
        if (panel != null)
        {
            OnPanelOpened(this, panel);
            PanelType panelType = panel.panelType;
            OnPanelChanged(panelType);
        }
    }
}

[System.Serializable]
public struct Resolution
{
    public int width;
    public int height;
}

public enum ImageType
{
    JPG,
    PNG
}