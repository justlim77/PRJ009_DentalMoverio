using UnityEngine;
using System.Collections;
using System;

public class ARDetailPanel : MonoBehaviour
{
    public GameObject SubPanelPrefab;
    public Resolution SubPanelDimensions;

    Texture2D _TextureArray;
    ARDetailSubPanel[] _SubPanels;

	void Start ()
    {
        StartCoroutine(Load());
	}

    public void LoadImages(Texture2D[] textureArray)
    {
        transform.Clear();

        int textureAmount = textureArray.Length;

        _SubPanels = new ARDetailSubPanel[textureAmount];

        for(int i = 0; i < textureAmount; i++)
        {
            GameObject panel = Instantiate(SubPanelPrefab);
            ARDetailSubPanel subPanel = panel.GetComponent<ARDetailSubPanel>();
            _SubPanels[i] = subPanel;

            subPanel.RectTransform.SetParent(this.transform);
            subPanel.RectTransform.localScale = Vector3.one;
            subPanel.RectTransform.pivot = new Vector2(0.5f, 0.5f);

            //subPanel.SetDimensions(0, SubPanelDimensions.height * i, SubPanelDimensions.width, SubPanelDimensions.height);
            subPanel.RawImage.texture = textureArray[i];
            subPanel.RectTransform.anchoredPosition = new Vector2(0, SubPanelDimensions.height * -i);
        }
    }

    public void LoadImages(DetailedImage[] textureDetails)
    {
        transform.Clear();

        int textureAmount = textureDetails.Length;

        _SubPanels = new ARDetailSubPanel[textureAmount];

        for (int i = 0; i < textureAmount; i++)
        {
            GameObject panel = Instantiate(SubPanelPrefab);
            ARDetailSubPanel subPanel = panel.GetComponent<ARDetailSubPanel>();
            _SubPanels[i] = subPanel;

            subPanel.RectTransform.SetParent(this.transform);
            subPanel.RectTransform.localScale = Vector3.one;
            subPanel.RectTransform.pivot = new Vector2(0.5f, 0.5f);

            //subPanel.SetDimensions(0, SubPanelDimensions.height * i, SubPanelDimensions.width, SubPanelDimensions.height);
            DetailedImage image = textureDetails[i];
            subPanel.RawImage.texture = image.Texture;
            subPanel.SetTitle(image.Title);
            subPanel.RectTransform.anchoredPosition = new Vector2(0, SubPanelDimensions.height * -i);
        }
    }

    IEnumerator Load()
    {
        yield return StartCoroutine(ARDirectoryManager.LoadLocalImages());

        LoadImages(ARDirectoryManager.TextureDetails);
    }
}
