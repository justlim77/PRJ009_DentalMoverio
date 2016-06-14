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
    [SerializeField] Button btnFacial;
    [SerializeField] Button btnRadiography;
    [SerializeField] Button btnMiracast;

    public Text headerLabel;
    public Image planeRenderer;

    [SerializeField] Resolution resolution;
    string deviceName = "";
    WebCamTexture camTex;

    void Awake()
    {
        _Instance = this;

        Core.SubscribeEvent("OpenPanel", OpenPanel);
    }

    void Start ()
    {
        btnFacial.onClick.AddListener(() => StopFeed());
        btnRadiography.onClick.AddListener(() => StopFeed());
        btnMiracast.onClick.AddListener(() => LaunchFeed());

        deviceName = WebCamTexture.devices[0].name;

        camTex = new WebCamTexture(deviceName, resolution.width, resolution.height);

        planeRenderer.material.mainTexture = camTex;
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
        btnFacial.onClick.RemoveAllListeners();
        btnRadiography.onClick.RemoveAllListeners();
        btnMiracast.onClick.RemoveAllListeners();

        Core.UnsubscribeEvent("OpenPanel", OpenPanel);
    }

    object OpenPanel(object sender, object args)
    {
        ARGUIPanel panel;
        if (args is ARGUIPanel)
        {
            panel = (ARGUIPanel)args;

            foreach (var ARPanel in ARGUIPanels)
            {
                ARPanel.SetAlpha(0);
            }

            panel.SetAlpha(1);

            string msg = panel.header;
            Core.BroadcastEvent("OnUpdateHeader", this, msg);
        }

        return null;
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
