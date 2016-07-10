using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ARProgressBar : MonoBehaviour
{
    public float lerpSpeed = 2.0f;

    Image _image;

    public float _targetRatio;

	// Use this for initialization
	void Start ()
    {
        _image = GetComponent<Image>();
        _image.fillAmount = 0;

        Core.SubscribeEvent("OnUpdateProgress", OnUpdateProgress);
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
        if (_image.fillAmount != _targetRatio)
        {
            _image.fillAmount = Mathf.Lerp(_image.fillAmount, _targetRatio, Time.deltaTime * lerpSpeed);
        }
	}


}
