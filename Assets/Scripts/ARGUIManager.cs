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
    //[SerializeField] ARGUIPanel RadiographPanel;
    [SerializeField] ARGUIPanel VideoPanel;
    [SerializeField] ARGUIPanel CameraPanel;
    [SerializeField] ARGUIPanel HomePanel;
    [SerializeField] GameObject MidPanel;
    [SerializeField] ARGUIPanel MutePanel;
    [Header("Buttons")]
    [SerializeField] Button btnFacial;
    //[SerializeField] Button btnRadiograph;
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
    ARDetailPanel _detailPanel;
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
        Core.SubscribeEvent("OnPanelOpened", OnPanelOpened);
        Core.SubscribeEvent("OnToggleBars", OnBarsToggled);

        InputControls.GestureDetected += OnGestureDetected;     //Moverio 4.5 ~ 4.6 with Input.GetMouse events

        NativeToolkit.OnImageSaved += NativeToolkit_OnImageSaved;
        ARDirectoryManager.OnImageLoadComplete += ARDirectoryManager_OnImageLoadComplete;
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
        btnMenu.onClick.AddListener(() => HomePanel.OpenPanel());
        btnMenu.onClick.AddListener(() => OnBarsToggled(this, false));

        btnLoad.onClick.AddListener(() => DetailPanel.OpenPanel());
        btnLoad.onClick.AddListener(() => StartApp());
        btnLoad.gameObject.SetActive(false);

        btnFacial.onClick.AddListener(() => DetailPanel.OpenPanel());
        btnFacial.onClick.AddListener(() => StopFeed());

        btnVideo.onClick.AddListener(() => VideoPanel.OpenPanel());
        btnVideo.onClick.AddListener(() => StopFeed());

        btnCamera.onClick.AddListener(() => CameraPanel.OpenPanel());
        btnCamera.onClick.AddListener(() => LaunchFeed());

        btnSnapshot.onClick.AddListener(() => SnapShot());
        btnGallery.onClick.AddListener(() => NativeToolkit.PickImage());

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

        // Setup detail panel
        _detailPanel = DetailPanel.GetComponent<ARDetailPanel>();
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
                _detailPanel.Scroll(ScrollType.Up);
                break;
            case TouchType.Down:
                _detailPanel.Scroll(ScrollType.Down);
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

    void OnDisable()
    {
        btnLoad.onClick.RemoveAllListeners();
        btnMenu.onClick.RemoveAllListeners();
        btnFacial.onClick.RemoveAllListeners();
        btnVideo.onClick.RemoveAllListeners();
        btnCamera.onClick.RemoveAllListeners();
        btnGallery.onClick.RemoveAllListeners();
        btnSnapshot.onClick.RemoveAllListeners();

        TouchControls.GestureDetected -= OnGestureDetected;
        NativeToolkit.OnImageSaved -= NativeToolkit_OnImageSaved;
        ARDirectoryManager.OnImageLoadComplete -= ARDirectoryManager_OnImageLoadComplete;

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
                ARPanel.SetActive(false);
            }

            panel.SetActive(true);

            switch (panel.panelType)
            {
                case PanelType.Camera:
                    _PanelTargetPos = panel.GetInversedInitialPos();
                    LaunchFeed();
                    break;
                case PanelType.Details:
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

            Vector2 detailCurrentPos = _detailPanel.scrollRect.content.anchoredPosition;
            if (detailCurrentPos != _detailPanel.targetPosition)
            {
                Vector2 lerpToPos = Vector2.Lerp(detailCurrentPos, _detailPanel.targetPosition, panelSlideSpeed * Time.deltaTime);
                _detailPanel.scrollRect.content.anchoredPosition = new Vector2(_detailPanel.scrollRect.content.anchoredPosition.x, lerpToPos.y);
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

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _detailPanel.Scroll(ScrollType.Down);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _detailPanel.Scroll(ScrollType.Up);
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