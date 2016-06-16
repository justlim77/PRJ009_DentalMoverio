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
    }

    public void OpenPanel()
    {
        Core.BroadcastEvent("OnOpenPanel", this, this);
        _canvasGroup.alpha = 1;
    }

    public void SetAlpha(float alpha)
    {
        _canvasGroup.alpha = alpha;
    }
}
