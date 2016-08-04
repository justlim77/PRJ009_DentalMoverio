using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public enum ContentLoadMode
{
    Offline,
    Online
}

public class ARContentLoader : MonoBehaviour
{
    public static ARContentLoader Instance { get; private set; }
    public Patient patient;

    public ContentLoadMode LoadMode;

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
        //Load();
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
        switch (LoadMode)
        {
            case ContentLoadMode.Online:
                print("Fetching Content from: " + content.WebURL);
                string webURL = content.WebURL;
                Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
                WWW www = new WWW(webURL);
                yield return www;
                www.LoadImageIntoTexture(tex);
                content.Tex = tex;
                content.Image.texture = tex;
                break;
            case ContentLoadMode.Offline:
                print("Fetching Content from: " + content.LocalURL);
                string localURL = content.LocalURL;

                break;
        }


        itemsLoaded++;
        Core.BroadcastEvent("OnUpdateProgress", this, Progress);
    }

    IEnumerator LoadContent(VideoContent content)
    {
        print("Fetching Content from: " + content.WebURL);
        string url = content.WebURL;
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
    public string WebURL;
    public string LocalURL;
    public Texture2D Tex;
    public RawImage Image;

    public ImageContent()
    {
        WebURL = "";
        LocalURL = "";
        Tex = null;
    }

    public ImageContent(string webURL, string localURL, int width, int height)
    {
        WebURL = webURL;
        LocalURL = localURL;
        Tex = new Texture2D(width, height, TextureFormat.DXT1, false);
    }
}

[System.Serializable]
public class VideoContent
{
    public string WebURL;
    public string LocalURL;
    //public MovieTexture MovieTex;
}
