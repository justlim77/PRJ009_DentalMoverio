using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ARDetailSubPanel : MonoBehaviour
{
    RectTransform _RectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (_RectTransform == null)
                _RectTransform = GetComponent<RectTransform>();
            return _RectTransform;
        }
    }

    RawImage _RawImage;
    public RawImage RawImage
    {
        get
        {
            if (_RawImage == null)
                _RawImage = GetComponent<RawImage>();
            return _RawImage;
        }
    }

    void Awake()
    {
        _RectTransform = GetComponent<RectTransform>();
        _RawImage = GetComponent<RawImage>();
    }

    // Use this for initialization
    void Start ()
    {
	
	}

    public void SetDimensions(float x, float y, float width, float height)
    {
        _RectTransform.rect.Set(x, y, width, height);
    }

    public void SetImage(Texture2D tex)
    {
        _RawImage.texture = tex;
    }
}
