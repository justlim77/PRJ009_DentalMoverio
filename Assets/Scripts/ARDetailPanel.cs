using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public enum ScrollType
{
    Up,
    Down,
    Left,
    Right
}

public class DetailPanelChangedEventArgs : EventArgs
{
    public int DetailPanelIndex { get; set; }
    public int DetailPanelTotal { get; set; }
}

public class ARDetailPanel : MonoBehaviour
{
    public delegate void DetailPanelChangedEventHandler(object sender, DetailPanelChangedEventArgs e);
    public static event DetailPanelChangedEventHandler DetailPanelChanged;

    public FileType fileType;

    public GameObject subPanelPrefab;
    public Resolution subPanelResolution;
    public float panelSlideSpeed = 5.0f;

    public Button previousButton;
    public Button nextButton;

    public Text slideLabel;

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

    protected virtual void OnDetailPanelChanged()
    {
        if (DetailPanelChanged != null)
            DetailPanelChanged(this, new DetailPanelChangedEventArgs() { DetailPanelIndex = _currentPanelIndex, DetailPanelTotal = _textureAmount });
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
            //subPanel.rawImage.texture = textureArray[i];
            subPanel.rectTransform.anchoredPosition = new Vector2(0, subPanelResolution.height * -i);
            subPanel.SetInitialPosition();

            _subPanelArray[i] = subPanel;
        }
    }

    public void LoadImages(FileType fileType)
    {
        scrollRect.content.transform.Clear();
        DetailedImage[] textureDetails = new DetailedImage[0];

        switch (fileType)
        {
            case FileType.Details:
                textureDetails = ARDirectoryManager.DetailTextures;
                break;
            case FileType.Radiograph:
                textureDetails = ARDirectoryManager.RadiographTextures;
                break;
            case FileType.Movement:
                textureDetails = ARDirectoryManager.MovementTextures;
                break;
            case FileType.Simulation:
                textureDetails = ARDirectoryManager.SimulationTextures;
                break;
        }        

        _textureAmount = textureDetails.Length;

        _subPanelArray = new ARDetailSubPanel[_textureAmount];

        scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Abs(486 * _textureAmount));

        for (int i = 0; i < _textureAmount; i++)
        {
            DetailedImage image = textureDetails[i];

            GameObject panel = Instantiate(subPanelPrefab);
            ARDetailSubPanel subPanel = panel.GetComponent<ARDetailSubPanel>();
            subPanel.rectTransform.SetParent(scrollRect.content.transform);

            Sprite sprite = Sprite.Create(image.Texture2D, new Rect(0, 0, image.Texture2D.width, image.Texture2D.height), Vector2.zero);
            subPanel.SetSprite(sprite);
            subPanel.SetTitle(image.Title);
            subPanel.rectTransform.anchoredPosition = new Vector2(0, subPanelResolution.height * -i);
            subPanel.SetInitialPosition();

            _subPanelArray[i] = subPanel;
        }

        _currentPanelIndex = 0;
        _cachedPanelIndex = _currentPanelIndex;
        targetPosition = _subPanelArray[0].initialPosition;
    }

    IEnumerator Load()
    {
        //yield return StartCoroutine(ARDirectoryManager.LoadLocalImages());
        while (!ARDirectoryManager.Initialized)
            yield return null;

        LoadImages(fileType);

        previousButton.onClick.AddListener(delegate { Scroll(ScrollType.Down); });
        nextButton.onClick.AddListener(delegate { Scroll(ScrollType.Up); });

        UpdateIndexLabel();
        UpdateButtonVisuals();
    }

    int _cachedPanelIndex = 0;
    public void Scroll(ScrollType scrollType)
    {
        switch (scrollType)
        {
            case ScrollType.Up:
                _currentPanelIndex += 1;
                break;
            case ScrollType.Down:
                _currentPanelIndex -= 1;
                break;
        }

        _currentPanelIndex = Mathf.Clamp(_currentPanelIndex, 0, _textureAmount - 1);

        if (_currentPanelIndex != _cachedPanelIndex)
        {
            targetPosition = _subPanelArray[_currentPanelIndex].initialInversedPosition;
            OnDetailPanelChanged();
            _cachedPanelIndex = _currentPanelIndex;

            UpdateIndexLabel();
            UpdateButtonVisuals();
        }
    }

    public void UpdateIndexLabel()
    {
        slideLabel.text = string.Format("{0} of {1}", _currentPanelIndex + 1, _textureAmount);
    }

    public void UpdateButtonVisuals()
    {
        if (_textureAmount == 1)
        {
            previousButton.image.CrossFadeAlpha(0, 0.25f, true);
            nextButton.image.CrossFadeAlpha(0, 0.25f, true);
        }
        else if (_currentPanelIndex <= 0)
        {
            previousButton.image.CrossFadeAlpha(0, 0.25f, true);
        }
        else if (_currentPanelIndex >= _textureAmount - 1)
        {
            nextButton.image.CrossFadeAlpha(0, 0.25f, true);
        }
        else
        {
            previousButton.image.CrossFadeAlpha(1, 0.5f, true);
            nextButton.image.CrossFadeAlpha(1, 0.5f, true);
        }
    }
}
