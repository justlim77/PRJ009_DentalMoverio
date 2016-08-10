using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ARDetailSubPanel : MonoBehaviour
{
    public Text text;
    public Vector2 initialPosition { get; private set; }
    public Vector2 initialInversedPosition { get; private set; }

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

    Image _rawImage;
    public Image rawImage
    {
        get
        {
            if (_rawImage == null)
                _rawImage = GetComponent<Image>();
            return _rawImage;
        }
    }

    public void SetDimensions(float x, float y, float width, float height)
    {
        rectTransform.rect.Set(x, y, width, height);
    }

    public void SetSprite(Sprite sprite)
    {
        rawImage.sprite = sprite;
    }

    public void SetTitle(string title)
    {
        //text.text = title;
    }

    public void SetInitialPosition()
    {
        initialPosition = rectTransform.anchoredPosition;
        initialInversedPosition = new Vector2(initialPosition.x, -initialPosition.y);
    }
}
