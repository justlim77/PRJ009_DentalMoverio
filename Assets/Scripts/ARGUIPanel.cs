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

    CanvasGroup _CanvasGroup;
    public CanvasGroup canvasGroup
    {
        get
        {
            if (_CanvasGroup == null)
            {
                _CanvasGroup = GetComponent<CanvasGroup>();
                if (_CanvasGroup == null)
                {
                    _CanvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            return _CanvasGroup;
        }
    }

    RectTransform _RectTransform;
    public RectTransform rectTransform
    {
        get
        {
            if (_RectTransform == null)
                _RectTransform = GetComponent<RectTransform>();
            return _RectTransform;
        }
    }

    HorizontalOrVerticalLayoutGroup _LayoutGroup;
    public HorizontalOrVerticalLayoutGroup layoutGroup
    {
        get
        {
            if(_LayoutGroup == null)
                _LayoutGroup = GetComponent<HorizontalOrVerticalLayoutGroup>();
            return _LayoutGroup;
        }
    }

    void Awake()
    {
        _initialPos = rectTransform.anchoredPosition;
    }

    public void OpenPanel()
    {
        Core.BroadcastEvent("OnPanelOpened", this, this);
        SetActive(true);
    }

    public void SetActive(bool value)
    {
        canvasGroup.alpha = value ? 1 : 0;
        canvasGroup.blocksRaycasts = value;

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
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
            layoutGroup.enabled = true;
        }
    }
}
