using UnityEngine;
using System.Collections;

[System.Serializable]
public enum PanelType
{
    Home,
    Facial,
    Radiograph,
    Video,
    Camera
}

[RequireComponent(typeof(CanvasRenderer))]
public class ARGUIPanel : MonoBehaviour 
{
    public PanelType panelType = PanelType.Home;

    CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = this.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = this.gameObject.AddComponent<CanvasGroup>();
    }

	// Use this for initialization
    void Start()
    {
        if(this.GetComponent<RectTransform>() != null)
            this.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    public void OpenPanel()
    {
        Core.BroadcastEvent("OnPanelOpened", this, this);
        _canvasGroup.alpha = 1;
        _canvasGroup.blocksRaycasts = true;
    }

    public void SetAlpha(float alpha)
    {
        _canvasGroup.alpha = alpha;
    }

    public void BlocksRaycasts(bool val)
    {
        _canvasGroup.blocksRaycasts = val;
    }
}
