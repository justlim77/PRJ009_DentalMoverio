using UnityEngine;
using System.Collections;

public class ARDetailPanel : MonoBehaviour
{
    public GameObject SubPanelPrefab;
    public Resolution SubPanelDimensions;

    Texture2D _TextureArray;

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(Load());
	}

    public void LoadImages(Texture2D[] textureArray)
    {
        for(int i = 0; i < textureArray.Length; i++)
        {
            GameObject panel = (GameObject)Instantiate(SubPanelPrefab);
            ARDetailSubPanel subPanel = panel.GetComponent<ARDetailSubPanel>();

            subPanel.RectTransform.SetParent(this.transform);
            subPanel.RectTransform.localScale = Vector3.one;
            subPanel.RectTransform.pivot = new Vector2(0.5f, 0.5f);

            //subPanel.SetDimensions(0, SubPanelDimensions.height * i, SubPanelDimensions.width, SubPanelDimensions.height);
            subPanel.RectTransform.anchoredPosition = new Vector2(0, SubPanelDimensions.height * i);
            subPanel.RawImage.texture = textureArray[i];
        }
    }

    IEnumerator Load()
    {
        yield return StartCoroutine(ARDirectoryManager.LoadLocalImages());

        LoadImages(ARDirectoryManager.TextureArray);
    }
}
