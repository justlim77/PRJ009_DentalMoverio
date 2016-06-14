using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasRenderer))]
public class ARGUIPanel : MonoBehaviour 
{
    public string header;

    CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = this.GetComponent<CanvasGroup>();
    }

	// Use this for initialization
    void Start()
    { }

    public void OpenPanel()
    {
        Core.BroadcastEvent("OpenPanel", this, this);
        canvasGroup.alpha = 1;
        this.transform.SetAsLastSibling();
    }

    public void SetAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
    }
}
