using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ARContentLoader : MonoBehaviour
{
    public static ARContentLoader Instance { get; private set; }
    public Patient patient;

    public int itemsToLoad;
    public int itemsLoaded;

    public bool Loaded
    {
        get; private set;
    }

    void Awake()
    {
        if (Instance = null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        Load();
    }

    public void Load()
    {
        Loaded = false;
        itemsLoaded = 0;
        StartCoroutine(LoadContent(patient.FaceFront));
        StartCoroutine(LoadContent(patient.FaceOblique));
        StartCoroutine(LoadContent(patient.FaceSide));
        StartCoroutine(LoadContent(patient.RadioOPG));
        StartCoroutine(LoadContent(patient.RadioLat));
        StartCoroutine(LoadContent(patient.Video));
    }

    IEnumerator LoadContent(ImageContent content)
    {
        print("Fetching Content from: " + content.URL);
        string url = content.URL;
        Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
        WWW www = new WWW(url);
        yield return www;
        www.LoadImageIntoTexture(tex);
        content.Tex = tex;
        content.Image.texture = tex;

        itemsLoaded++;
        Core.BroadcastEvent("OnUpdateProgress", this, Progress);
    }

    IEnumerator LoadContent(VideoContent content)
    {
        print("Fetching Content from: " + content.URL);
        string url = content.URL;
        //MovieTexture tex = new MovieTexture();
        WWW www = new WWW(url);
        while (www.isDone == false)
        {
            yield return null;
        }
        //tex = www.movie;        
        //while (tex.isReadyToPlay == false)
        //{
        //    yield return 0;
        //}
          
        //content.MovieTex = tex;

        itemsLoaded++;
        Core.BroadcastEvent("OnUpdateProgress", this, Progress);
    }


    public float Progress
    {
        get
        {
            float percentage = (float)itemsLoaded / (float)itemsToLoad;
            return percentage;
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
public class ImageContent
{
    public string URL;
    public Texture2D Tex;
    public RawImage Image;

    public ImageContent()
    {
        URL = "";
        Tex = null;
    }

    public ImageContent(string url, int width, int height)
    {
        URL = url;
        Tex = new Texture2D(width, height, TextureFormat.DXT1, false);
    }
}

[System.Serializable]
public class VideoContent
{
    public string URL;
    //public MovieTexture MovieTex;
}
