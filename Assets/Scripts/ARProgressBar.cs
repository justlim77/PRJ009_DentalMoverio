using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ARProgressBar : MonoBehaviour
{
    public Text label;
    public float lerpSpeed = 2.0f;

    float _targetRatio;
    int _filesToLoad = 0;

    Image _image;
    Image image
    {
        get
        {
            if (_image == null)
            {
                _image = GetComponent<Image>();
            }

            return _image;
        }
    }
	// Use this for initialization
	void Start ()
    {
        Initialize();
	}

    public bool Initialize()
    {               
        return true;
    }

    void OnEnable()
    {
        Core.SubscribeEvent("OnUpdateProgress", OnUpdateProgress);

        ARDirectoryManager.OnDirectoryReadComplete += ARDirectoryManager_OnDirectoryReadComplete;
        ARDirectoryManager.OnImageLoadStart += ARDirectoryManager_OnImageLoadStart;
        ARDirectoryManager.OnImageLoadComplete += ARDirectoryManager_OnImageLoadComplete;
    }

    void OnDisable()
    {
        Core.UnsubscribeEvent("OnUpdateProgress", OnUpdateProgress);

        ARDirectoryManager.OnDirectoryReadComplete -= ARDirectoryManager_OnDirectoryReadComplete;
        ARDirectoryManager.OnImageLoadStart -= ARDirectoryManager_OnImageLoadStart;
        ARDirectoryManager.OnImageLoadComplete -= ARDirectoryManager_OnImageLoadComplete;
    }

    private void ARDirectoryManager_OnImageLoadComplete(string obj)
    {
        label.text = string.Empty;
    }

    private void ARDirectoryManager_OnImageLoadStart(ImageBlob obj)
    {
        //image.fillAmount = obj.Index / _filesToLoad;
        _targetRatio = obj.Index / _filesToLoad;
        label.text = string.Format("Loading {0}", obj.Path);
    }

    private void ARDirectoryManager_OnDirectoryReadComplete(int obj)
    {
        _filesToLoad = obj;
    }

    object OnUpdateProgress(object sender, object args)
    {
        if (args is float)
        {
            _targetRatio = (float)args;
        }

        return null;
    }

    // Update is called once per frame
    void Update ()
    {
        //if (image.fillAmount != _targetRatio)
        //{
        //    image.fillAmount = Mathf.Lerp(image.fillAmount, _targetRatio, Time.deltaTime * lerpSpeed);
        //}
	}
}
