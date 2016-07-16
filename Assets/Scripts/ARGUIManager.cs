using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ARGUIManager : MonoBehaviour
{
    public static ARGUIManager Instance { get; private set; }

    public ARGUIPanel[] ARGUIPanels;
    [Header("Bottom Panel")]
    [SerializeField] Button btnFacial;
    [SerializeField] Button btnRadiography;
    [SerializeField] Button btnVideo;
    [SerializeField] Button btnMiracast;
    [Header("General")]
    [SerializeField] Button btnMenu;
    [SerializeField] Button btnLoad;
    [SerializeField] Button btnSnapshot;
    [SerializeField] GameObject TopBar;
    [SerializeField] GameObject BotBar;
    [SerializeField] GameObject StartPanel;

    public Image planeRenderer;

    [SerializeField] Resolution resolution;
    WebCamDevice device;
    WebCamTexture camTex;

    CanvasGroup _topBarCanvasGroup, _botBarCanvasGroup;
    int _currentPanelIdx;

    void Awake()
    {
        if(Instance == null)
            Instance = this;

        Core.SubscribeEvent("OnPanelOpened", OnPanelOpened);
        Core.SubscribeEvent("OnToggleBars", OnToggleBars);
    }

    void Start ()
    {
        btnMenu.onClick.AddListener(() => OnToggleBars(this, false));
        btnLoad.onClick.AddListener(() => StartApp());
        btnFacial.onClick.AddListener(() => StopFeed());
        btnRadiography.onClick.AddListener(() => StopFeed());
        btnVideo.onClick.AddListener(() => StopFeed());
        btnMiracast.onClick.AddListener(() => LaunchFeed());
        btnSnapshot.onClick.AddListener(() => SnapShot());

        TouchControls.GestureDetected += OnGestureDetected;

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

        planeRenderer.material.mainTexture = camTex;

        StartPanel.transform.SetAsLastSibling();
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
                controller.SetCurrentMuteType(!muted);
                Debug.Log("Display/Audio mute: " + muted);
                break;
        }
    }

    void StartApp()
    {
        ShowBar(_topBarCanvasGroup);
        ShowBar(_botBarCanvasGroup);

        ARGUIPanel panel = StartPanel.GetComponent<ARGUIPanel>();
        if (panel != null)
        {
            panel.SetAlpha(0);
        }
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
        btnMiracast.onClick.RemoveAllListeners();

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

            _currentPanelIdx = Array.IndexOf(ARGUIPanels, panel);
            Debug.Log("Current panel index: " + _currentPanelIdx);

            string msg = panel.header;
            Core.BroadcastEvent("OnUpdateHeader", this, msg);
        }

        return null;
    }

    object OnToggleBars(object sender, object args)
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
#if UNITY_ANDROID
        filePath = "/mnt/sdcard/DCIM/Images/" + "DentalAR_" + capture.GetFileName(resolution.width, resolution.height);
#elif UNITY_EDITOR
        filePath = Application.dataPath + "/Screenshots/" + capture.GetFileName(resolution.width, resolution.height);
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
        }
    }
}

[System.Serializable]
public struct Resolution
{
    public int width;
    public int height;
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      