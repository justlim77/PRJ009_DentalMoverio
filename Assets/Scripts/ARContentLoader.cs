using UnityEngine;
using System.Collections;

public class ARContentLoader : MonoBehaviour
{
    public Patient patient;

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(FetchPatientData());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    IEnumerator FetchPatientData()
    {
        string url = "";

        url = patient.FaceFront.URL;
        while (true)
        {
            WWW www = new WWW(url);
            yield return www;
            www.LoadImageIntoTexture(patient.FaceFront.Tex);
        }

        url = patient.FaceOblique.URL;
        while (true)
        {
            WWW www = new WWW(url);
            yield return www;
            www.LoadImageIntoTexture(patient.FaceOblique.Tex);
        }

        url = patient.FaceSide.URL;
        while (true)
        {
            WWW www = new WWW(url);
            yield return www;
            www.LoadImageIntoTexture(patient.FaceSide.Tex);
        }

        url = patient.RadioOPG.URL;
        while (true)
        {
            WWW www = new WWW(url);
            yield return www;
            www.LoadImageIntoTexture(patient.RadioOPG.Tex);
        }

        url = patient.RadioLat.URL;
        while (true)
        {
            WWW www = new WWW(url);
            yield return www;
            www.LoadImageIntoTexture(patient.RadioLat.Tex);
        }
    }
}

[System.Serializable]
public class Patient
{
    public string Name;
    public ImageContent FaceFront;
    public ImageContent FaceOblique;
    public ImageContent FaceSide;
    public ImageContent RadioOPG;
    public ImageContent RadioLat;
    public VideoContent Video;

    public Patient()
    { }
}

[System.Serializable]
public struct ImageContent
{
    public string URL;
    public Texture2D Tex;
}

[System.Serializable]
public struct VideoContent
{
    public string URL;
    public Texture2D[] MovieTex;
}
