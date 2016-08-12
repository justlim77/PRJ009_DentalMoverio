using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
    [SerializeField] ARGUIPanel DetailPanel;
    [SerializeField] ARGUIPanel RadiographsPanel;
    [SerializeField] ARGUIPanel MovementPanel;
    [SerializeField] ARGUIPanel SimulationPanel;
    [SerializeField] ARGUIPanel VideoPanel;
    [SerializeField] ARGUIPanel CameraPanel;
    [SerializeField] ARGUIPanel HomePanel;
    [SerializeField] GameObject MidPanel;
    [SerializeField] ARGUIPanel MutePanel;
    [Header("Buttons")]
    [SerializeField] Button btnDetails;
    [SerializeField] Button btnRadiograph;
    [SerializeField] Button btnMovement;
    [SerializeField] Button btnSimulation;
    [SerializeField] Button btnVideo;
    [SerializeField] Button btnCamera;
    [SerializeField] Button btnGallery;
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

    WebCamDevice _device;
    WebCamTexture _camTex;
    CanvasGroup _topBarCanvasGroup, _botBarCanvasGroup;
    RectTransform _midPanelRectTrans, _homePanelRectTrans;
    ARDetailPanel _currentDetailPanel;
    int _currentPanelIdx;

    protected virtual void OnPanelChanged(PanelType type)
    {
        if (PanelChanged != null)
            PanelChanged(this, new PanelChangedEventArgs() { PanelType = type });
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
    }

    void OnEnable()
    {
        btnMenu.onClick.AddListener(() => HomePanel.OpenPanel());
        btnMenu.onClick.AddListener(() => OnBarsToggled(this, false));

        btnLoad.onClick.AddListener(() => DetailPanel.OpenPanel());
        btnLoad.onClick.AddListener(() => StartApp());

        btnDetails.onClick.AddListener(() => DetailPanel.OpenPanel());
        btnDetails.onClick.AddListener(() => StopFeed());

        btnRadiograph.onClick.AddListener(() => RadiographsPanel.OpenPanel());
        btnRadiograph.onClick.AddListener(() => StopFeed());

        btnMovement.onClick.AddListener(() => MovementPanel.OpenPanel());
        btnMovement.onClick.AddListener(() => StopFeed());

        btnSimulation.onClick.AddListener(() => SimulationPanel.OpenPanel());
        btnSimulation.onClick.AddListener(() => StopFeed());

        btnVideo.onClick.AddListener(() => VideoPanel.OpenPanel());
        btnVideo.onClick.AddListener(() => StopFeed());

        btnCamera.onClick.AddListener(() => CameraPanel.OpenPanel());
        btnCamera.onClick.AddListener(() => LaunchFeed());

        btnSnapshot.onClick.AddListener(() => SnapShot());
        btnGallery.onClick.AddListener(() => NativeToolkit.PickImage());

        Core.SubscribeEvent("OnPanelOpened", OnPanelOpened);
        Core.SubscribeEvent("OnToggleBars", OnBarsToggled);

        InputControls.GestureDetected += OnGestureDetected;     //Moverio 4.5 ~ 4.6 with Input.GetMouse events
        InputControls.BackButtonPressed += InputControls_BackButtonPressed;
        InputControls.SettingButtonPressed += InputControls_SettingButtonPressed;
        NativeToolkit.OnImageSaved += NativeToolkit_OnImageSaved;
        ARDirectoryManager.OnImageLoadComplete += ARDirectoryManager_OnImageLoadComplete;
    }
    void OnDisable()
    {
        btnLoad.onClick.RemoveAllListeners();
        btnMenu.onClick.RemoveAllListeners();
        btnDetails.onClick.RemoveAllListeners();
        btnRadiograph.onClick.RemoveAllListeners();
        btnMovement.onClick.RemoveAllListeners();
        btnSimulation.onClick.RemoveAllListeners();
        btnVideo.onClick.RemoveAllListeners();
        btnCamera.onClick.RemoveAllListeners();
        btnGallery.onClick.RemoveAllListeners();
        btnSnapshot.onClick.RemoveAllListeners();

        Core.UnsubscribeEvent("OnPanelOpened", OnPanelOpened);
        Core.UnsubscribeEvent("OnToggleBars", OnBarsToggled);

        InputControls.GestureDetected -= OnGestureDetected;     //Moverio 4.5 ~ 4.6 with Input.GetMouse events
        InputControls.BackButtonPressed -= InputControls_BackButtonPressed;
        InputControls.SettingButtonPressed -= InputControls_SettingButtonPressed;
        NativeToolkit.OnImageSaved -= NativeToolkit_OnImageSaved;
        ARDirectoryManager.OnImageLoadComplete -= ARDirectoryManager_OnImageLoadComplete;
    }

    private void InputControls_SettingButtonPressed(object source, EventArgs e)
    {
        ToggleMenu();
    }

    private void InputControls_BackButtonPressed(object source, EventArgs e)
    {
        btnMenu.onClick.Invoke();
    }

    private void ARDirectoryManager_OnImageLoadComplete(string obj)
    {
        btnLoad.gameObject.SetActive(true);
    }

    private void NativeToolkit_OnImageSaved(string obj)
    {
        NativeToolkit.ScheduleLocalNotification("DentalAR", "Image saved to " + obj, smallIcon:"", largeIcon:"dental_notification_large");
    }

    void Start ()
    {
        btnLoad.gameObject.SetActive(false);

        ToggleMenu();

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
            _device = WebCamTexture.devices[0];
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.Log(e.Message);
        }

        if(_device.name != null)
            _camTex = new WebCamTexture(_device.name, resolution.width, resolution.height);

        cameraPlane.material.mainTexture = _camTex;

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

        StartCoroutine(ARDirectoryManager.LoadLocalImages());
        // Setup detail panel
        //_currentDetailPanel = DetailPanel.GetComponent<ARDetailPanel>();
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
            case TouchType.Up:
                _currentDetailPanel.Scroll(ScrollType.Up);
                break;
            case TouchType.Down:
                _currentDetailPanel.Scroll(ScrollType.Down);
                break;
            //case TouchType.DoubleTap:
            //    MoverioCameraController controller = MoverioCameraController.Instance;
            //    bool muted = controller.GetCurrentMuteState();
            //    CanvasMuteType = !CanvasMuteType;
            //    controller.SetCurrentMuteType(!muted);
            //    break;
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
        //DetailsPanel.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = false;
        //DetailsPanel.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = true;
        //RadiographPanel.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = false;
        //RadiographPanel.GetComponent<HorizontalOrVerticalLayoutGroup>().enabled = true;

        //HomePanel.SetActive(false);
        _HomeTargetPos = HomePanel.GetInitialPosition();

        MidPanel.GetComponent<ScrollRect>().enabled = true;

        _started = true;
    }

    void LaunchFeed()
    {
        if (_camTex != null)
            if (!_camTex.isPlaying)
                _camTex.Play();
    }

    void StopFeed()
    {
        if(_camTex != null)
            if(_camTex.isPlaying)
                _camTex.Stop();
    }



    object OnPanelOpened(object sender, object args)
    {
        ARGUIPanel panel;
        if (args is ARGUIPanel)
        {
            panel = (ARGUIPanel)args;

            foreach (var ARPanel in ARGUIPanels)
            {
                ARPanel.SetActive(false);
            }

            panel.SetActive(true);

            switch (panel.panelType)
            {
                case PanelType.Camera:
                    _PanelTargetPos = panel.GetInversedInitialPos();
                    LaunchFeed();
                    break;
                case PanelType.PatientDetails:
                case PanelType.Radiographs:
                case PanelType.SurgeryMovement:
                case PanelType.SurgicalSimulation:
                    StopFeed();
                    _currentDetailPanel = panel.GetComponent<ARDetailPanel>();
                    _PanelTargetPos = panel.GetInversedInitialPos();
                    break;
                case PanelType.Video:
                    StopFeed();
                    _PanelTargetPos = panel.GetInversedInitialPos();
                    break;
                case PanelType.Home:
                    StopFeed();
                    _HomeTargetPos = Vector2.zero;
                    break;
            }

            _currentPanelIdx = Array.IndexOf(ARGUIPanels, panel);

            string msg = panel.panelType.ToString();
            Core.BroadcastEvent("OnUpdateHeader", this, msg);
        }

        return null;
    }

    Vector2 _PanelTargetPos, _HomeTargetPos;
    void Update()
    {
        //Slide content panels
        if (!InputControls.Holding)
        {            
            Vector2 panelCurrentPos = _midPanelRectTrans.anchoredPosition;
            if (panelCurrentPos != _PanelTargetPos)
            {
                Vector2 lerpToPos = Vector2.Lerp(panelCurrentPos, _PanelTargetPos, panelSlideSpeed * Time.deltaTime);
                _midPanelRectTrans.anchoredPosition = new Vector2(lerpToPos.x, _midPanelRectTrans.anchoredPosition.y);
            }

            if (_currentDetailPanel != null)
            {
                Vector2 detailCurrentPos = _currentDetailPanel.scrollRect.content.anchoredPosition;
                if (detailCurrentPos != _currentDetailPanel.targetPosition)
                {
                    Vector2 lerpToPos = Vector2.Lerp(detailCurrentPos, _currentDetailPanel.targetPosition, panelSlideSpeed * Time.deltaTime);
                    _currentDetailPanel.scrollRect.content.anchoredPosition = new Vector2(_currentDetailPanel.scrollRect.content.anchoredPosition.x, lerpToPos.y);
                }
            }
        }

        //Slide home panel
        Vector2 homeCurrentPos = _homePanelRectTrans.anchoredPosition;
        if (homeCurrentPos != _HomeTargetPos)
        {
            Vector2 lerpToPos = Vector2.Lerp(homeCurrentPos, _HomeTargetPos, panelSlideSpeed * Time.deltaTime);
            _homePanelRectTrans.anchoredPosition = lerpToPos;
        }

        //Button mapping
        //if (Input.GetKeyDown(KeyCode.Menu)/* || Input.GetKeyDown(KeyCode.Return)*/)
        //{
        //    HomePanel.OpenPanel();
        //    OnBarsToggled(this, false);
        //}

        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    PreviousPanel();
        //}

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PreviousPanel();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextPanel();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _currentDetailPanel.Scroll(ScrollType.Down);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _currentDetailPanel.Scroll(ScrollType.Up);
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

    string _cachedImagePath = "";
    void SnapShot()
    {
        //ScreenCapture capture = GetComponent<ScreenCapture>();

        //        string filePath;
        //        string subPath;
        //#if UNITY_ANDROID && !UNITY_EDITOR
        //        subPath = Constants.PhoneSDCardPath;
        //        filePath = subPath + capture.GetFileName(resolution.width, resolution.height);
        //#elif UNITY_EDITOR
        //        subPath = Application.dataPath + "/Screenshots/";
        //        if (!Directory.Exists(subPath))
        //            Directory.CreateDirectory(subPath);
        //        filePath = subPath + capture.GetFileName(resolution.width, resolution.height);
        //#endif
        //        Debug.Log("File path: " + filePath);
        //        capture.SaveScreenshot(CaptureMethod.ReadPixels_Asynch, filePath);

        //        Directory.GetFiles(filePath);

        //float x = (Screen.width * 0.5f) - (resolution.width * 0.5f);
        //float y = Screen.height - resolution.height;
        //Rect rect = new Rect(x, y, resolution.width, resolution.height);
        //NativeToolkit.SaveScreenshot(ScreenCapture.GetFileName(resolution.width, resolution.height), Application.productName, screenArea: rect);

        Texture2D tex = new Texture2D(_camTex.width, _camTex.height);
        tex.SetPixels(_camTex.GetPixels());
        tex.Apply();

        string fileName = ScreenCapture.GetFileName(resolution.width, resolution.height);
        NativeToolkit.SaveImage(tex, fileName);
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

    bool isMenuOpen
    {
        get
        {
            return btnMenu.image.color.a == 1;
        }
    }

    float _fadeDuration = 0.25f;
    void ToggleMenu()
    {
        float fadeTo = isMenuOpen ? 0 : 1;
        btnMenu.image.CrossFadeAlpha(fadeTo == 1 ? 0 : 1, _fadeDuration, true);
        btnDetails.image.CrossFadeAlpha(fadeTo, _fadeDuration, true);
        btnRadiograph.image.CrossFadeAlpha(fadeTo, _fadeDuration, true);
        btnMovement.image.CrossFadeAlpha(fadeTo, _fadeDuration, true);
        btnSimulation.image.CrossFadeAlpha(fadeTo, _fadeDuration, true);
        btnVideo.image.CrossFadeAlpha(fadeTo, _fadeDuration, true);
        btnCamera.image.CrossFadeAlpha(fadeTo, _fadeDuration, true);
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