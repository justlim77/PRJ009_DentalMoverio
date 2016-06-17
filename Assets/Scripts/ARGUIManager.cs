using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ARGUIManager : MonoBehaviour
{
    static ARGUIManager _Instance = null;
    public static ARGUIManager Instance
    { 
        get 
        { 
            return _Instance; 
        } 
    }

    public ARGUIPanel[] ARGUIPanels;
    [SerializeField] Button btnMenu;
    [SerializeField] Button btnFacial;
    [SerializeField] Button btnRadiography;
    [SerializeField] Button btnVideo;
    [SerializeField] Button btnMiracast;
    [SerializeField] GameObject TopBar;
    [SerializeField] GameObject BotBar;
    [SerializeField] GameObject StartPanel;

    public Image planeRenderer;

    [SerializeField] Resolution resolution;
    string deviceName = "";
    WebCamTexture camTex;

    CanvasGroup _topBarCanvasGroup, _botBarCanvasGroup;

    void Awake()
    {
        _Instance = this;

        Core.SubscribeEvent("OnOpenPanel", OnOpenPanel);
        Core.SubscribeEvent("OnToggleBars", OnToggleBars);
    }

    void Start ()
    {
        btnMenu.onClick.AddListener(() => OnToggleBars(this, false));
        btnFacial.onClick.AddListener(() => StopFeed());
        btnRadiography.onClick.AddListener(() => StopFeed());
        btnVideo.onClick.AddListener(() => StopFeed());
        btnMiracast.onClick.AddListener(() => LaunchFeed());

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

        deviceName = WebCamTexture.devices[0].name;

        camTex = new WebCamTexture(deviceName, resolution.width, resolution.height);

        planeRenderer.material.mainTexture = camTex;

        StartPanel.transform.SetAsLastSibling();
	}

    void LaunchFeed()
    {
        if(!camTex.isPlaying)
            camTex.Play();
    }

    void StopFeed()
    {
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

        Core.UnsubscribeEvent("OnOpenPanel", OnOpenPanel);
    }

    object OnOpenPanel(object sender, object args)
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

            ShowTopBar(val);
            ShowBotBar(val);            
        }

        return null;
    }

    public void ShowTopBar(bool show = true)
    {
        if (_topBarCanvasGroup != null)
            if (show)
                _topBarCanvasGroup.alpha = 1;
            else
                _topBarCanvasGroup.alpha = 0;
    }

    public void ShowBotBar(bool show = true)
    {
        if (_botBarCanvasGroup != null)
            if (show)
                _botBarCanvasGroup.alpha = 1;
            else
                _botBarCanvasGroup.alpha = 0;
    }
}

[System.Serializable]
struct Resolution
{
    public int width;
    public int height;
}

public class PanelObject
{
    public ARGUIPanel Panel;
    public string Header;
}
