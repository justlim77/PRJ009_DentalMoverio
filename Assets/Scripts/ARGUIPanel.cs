using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public enum PanelType
{
    Blocker,
    Home,
    Details,
    Video,
    Camera
}

[RequireComponent(typeof(CanvasRenderer))]
public class ARGUIPanel : MonoBehaviour 
{
    public PanelType panelType = PanelType.Home;

    Vector2 _initialPos;
    CanvasGroup _canvasGroup;
    RectTransform _rectTrans;
    HorizontalOrVerticalLayoutGroup _layoutGroup;

    void Awake()
    {
        _canvasGroup = this.GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = this.gameObject.AddComponent<CanvasGroup>();

        _rectTrans = this.GetComponent<RectTransform>();
        _initialPos = _rectTrans.anchoredPosition;

        _layoutGroup = this.GetComponent<HorizontalOrVerticalLayoutGroup>();
    }

    void Start()
    {
        //if(this.GetComponent<RectTransform>() != null)
        //    this.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
    }

    public void OpenPanel()
    {
        Core.BroadcastEvent("OnPanelOpened", this, this);
        SetActive(true);
    }

    public void SetActive(bool value)
    {
        _canvasGroup.alpha = value ? 1 : 0;
        _canvasGroup.blocksRaycasts = value;

        //Refresh();
    }

    public Vector2 GetInitialPosition()
    {
        return _initialPos;
    }

    public Vector2 GetInversedInitialPos()
    {
        return new Vector2(_initialPos.x * -1, _initialPos.y);
    }

    public void Refresh()
    {
        //Perform quick re-layout
        if (_layoutGroup != null)
        {
            _layoutGroup.enabled = false;
            _layoutGroup.enabled = true;
        }
    }

}
