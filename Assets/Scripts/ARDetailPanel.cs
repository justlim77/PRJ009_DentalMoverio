using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ARDetailPanel : MonoBehaviour
{
    public GameObject subPanelPrefab;
    public Resolution subPanelResolution;
    public float panelSlideSpeed = 5.0f;

    Texture2D _textureArray;    // Deprecated
    ARDetailSubPanel[] _subPanelArray;
    int _currentPanelIndex = 0;
    int _textureAmount = 0;

    public Vector2 targetPosition { get; set; }

    ScrollRect _scrollRect;
    public ScrollRect scrollRect
    {
        get
        {
            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }
            return _scrollRect;
        }
    }

    RectTransform _rectTransform;
    public RectTransform rectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    void Start ()
    {
        StartCoroutine(Load());
	}

    public void LoadImages(Texture2D[] textureArray)
    {
        scrollRect.content.transform.Clear();

        int textureAmount = textureArray.Length;

        _subPanelArray = new ARDetailSubPanel[textureAmount];

        scrollRect.content.rect.Set(0, 0, scrollRect.content.rect.width, subPanelResolution.height * textureAmount);

        for(int i = 0; i < textureAmount; i++)
        {
            GameObject panel = Instantiate(subPanelPrefab);

            ARDetailSubPanel subPanel = panel.GetComponent<ARDetailSubPanel>();
            subPanel.rectTransform.SetParent(scrollRect.content.transform);
            subPanel.rawImage.texture = textureArray[i];
            subPanel.rectTransform.anchoredPosition = new Vector2(0, subPanelResolution.height * -i);
            subPanel.SetInitialPosition();

            _subPanelArray[i] = subPanel;
        }
    }

    public void LoadImages(DetailedImage[] textureDetails)
    {
        scrollRect.content.transform.Clear();

        _textureAmount = textureDetails.Length;

        _subPanelArray = new ARDetailSubPanel[_textureAmount];

        scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(486 * _textureAmount));

        for (int i = 0; i < _textureAmount; i++)
        {
            DetailedImage image = textureDetails[i];

            GameObject panel = Instantiate(subPanelPrefab);
            ARDetailSubPanel subPanel = panel.GetComponent<ARDetailSubPanel>();
            subPanel.rectTransform.SetParent(scrollRect.content.transform);
            subPanel.rawImage.texture = image.Texture;
            subPanel.SetTitle(image.Title);
            subPanel.rectTransform.anchoredPosition = new Vector2(0, subPanelResolution.height * -i);
            subPanel.SetInitialPosition();

            _subPanelArray[i] = subPanel;
        }

        _currentPanelIndex = 0;
        targetPosition = _subPanelArray[0].initialPosition;
    }

    IEnumerator Load()
    {
        yield return StartCoroutine(ARDirectoryManager.LoadLocalImages());

        LoadImages(ARDirectoryManager.TextureDetails);
    }

    public void ScrollUp()
    {
        _currentPanelIndex += 1;
        _currentPanelIndex = Mathf.Clamp(_currentPanelIndex, 0, _textureAmount - 1);
        targetPosition = _subPanelArray[_currentPanelIndex].initialInversedPosition;
        Debug.Log(targetPosition);
    }

    public void ScrollDown()
    {
        _currentPanelIndex -= 1;
        _currentPanelIndex = Mathf.Clamp(_currentPanelIndex, 0, _textureAmount);
        targetPosition = _subPanelArray[_currentPanelIndex].initialInversedPosition;
        Debug.Log(targetPosition);
    }
}
