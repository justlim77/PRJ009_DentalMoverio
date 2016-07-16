using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasRenderer))]
public class ARGUIPanel : MonoBehaviour 
{
    public bool showOnStart = false;
    public string header;

    CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = this.GetComponent<CanvasGroup>();
    }

	// Use this for initialization
    void Start()
    {
        if(this.GetComponent<RectTransform>() != null)
            this.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        _canvasGroup.alpha = showOnStart ? 1 : 0;
        _canvasGroup.blocksRaycasts = showOnStart ? true : false;
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
