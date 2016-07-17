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

    public ARGUIPanel[] ARGUIPanels;
    [Header("Bottom Panel")]
    [SerializeField] Button btnFacial;
    [SerializeField] Button btnRadiography;
    [SerializeField] Button btnVideo;
    [SerializeField] Button btnCamera;
    [Header("General")]
    [SerializeField] Button btnMenu;
    [SerializeField] Button btnLoad;
    [SerializeField] Button btnSnapshot;
    [SerializeField] GameObject TopBar;
    [SerializeField] GameObject BotBar;
    [SerializeField] ARGUIPanel HomePanel;
    [SerializeField] ARGUIPanel MutePanel;

    public Image cameraPlane;

    [SerializeField] Resolution resolution;
    WebCamDevice device;
    WebCamTexture camTex;

    CanvasGroup _topBarCanvasGroup, _botBarCanvasGroup, _muteCanvasGroup;
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
        btnMenu.onClick.AddListener(() => OnBarsToggled(this, false));
        btnLoad.onClick.AddListener(() => StartApp());
        btnFacial.onClick.AddListener(() => StopFeed());
        btnRadiography.onClick.AddListener(() => StopFeed());
        btnVideo.onClick.AddListener(() => StopFeed());
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

        if (MutePanel != null)
        {
            _muteCanvasGroup = MutePanel.GetComponent<CanvasGroup>();
            CanvasMuteType = false;
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

        HomePanel.OpenPanel();
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

            if (_muteCanvasGroup != null)
            {
                _muteCanvasGroup.alpha = value ? 1 : 0;
            }
            else
                Debug.LogWarning("No main canvas group found!");
        }
    }

    void StartApp()
    {
        ShowBar(_topBarCanvasGroup);
        ShowBar(_botBarCanvasGroup);

        HomePanel.SetAlpha(0);
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
        btnRadiography.onClick.RemoveAllListeners();
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
                ARPanel.SetAlpha(0);
                ARPanel.BlocksRaycasts(false);
            }

            panel.SetAlpha(1);
            panel.BlocksRaycasts(true);

            switch (panel.panelType)
            {
                case PanelType.Camera:
                    LaunchFeed();
                    break;
                case PanelType.Facial:
                case PanelType.Radiograph:
                case PanelType.Video:
                case PanelType.Home:
                    StopFeed();
                    break;
            }

            _currentPanelIdx = Array.IndexOf(ARGUIPanels, panel);
            Debug.Log("Current panel index: " + _currentPanelIdx);

            string msg = panel.panelType.ToString();
            Core.BroadcastEvent("OnUpdateHeader", this, msg);
        }

        return null;
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
        subPath = "/mnt/sdcard/DCIM/Camera/";
        filePath = subPath + "DentalAR_" + capture.GetFileName(resolution.width, resolution.height);
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