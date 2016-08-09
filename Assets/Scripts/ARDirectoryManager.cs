using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System;

public class ImageBlob
{
    public string Name;
    public int Index;
    public string Path;

    public ImageBlob() { }
    public ImageBlob(string name, int index, string path)
    {
        Name = name;
        Index = index;
        Path = path;
    }
}

public class ARDirectoryManager : MonoBehaviour
{
    public static event Action<int> OnDirectoryReadComplete;
    public static event Action<ImageBlob> OnImageLoadStart;
    public static event Action<string> OnImageLoadComplete;

    static GameObject _go;
    static ARDirectoryManager _Instance = null;
    public static ARDirectoryManager Instance
    {
        get 
        {
            if (_Instance == null)
            {
                _go = new GameObject();
                _go.name = "ARDirectoryManager";
                _Instance = _go.AddComponent<ARDirectoryManager>();
            }

            return _Instance;
        }
    }


    public static string ImageFolderPath
    {
        get
        {
#if UNITY_EDITOR
            string path = Path.Combine(Application.streamingAssetsPath, "images");
#elif UNITY_ANDROID
            string path = Path.Combine(Application.persistentDataPath, "images");          
#endif
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log("Path not found! Creating folder at " + path);
            }

            return path;
        }
    }

    static string _VideoPath;
    public static string VideoPath
    {
        get
        {
#if UNITY_EDITOR
            string path = Path.Combine(Application.streamingAssetsPath, "videos");
#elif UNITY_ANDROID        
            string path = Path.Combine(Application.persistentDataPath, "videos");          
#endif
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debugger.Instance.Log("Video path not found! Creating folder: " + path);
            }

            _VideoPath = path;
            return _VideoPath;
        }
    }

    void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    public static Texture2D[] TextureArray;
    public static DetailedImage[] TextureDetails;
    public static string[] FilePaths;
    public static int FileAmount;
    static string _PathPrefix = "";

    public static IEnumerator LoadLocalImages()
    {
        Instance.Awake();

        string path = ImageFolderPath;

        _PathPrefix = @"file://";

#if UNITY_EDITOR
        FilePaths = Directory.GetFiles(path, "*.jpg").OrderBy(f => f).ToArray<string>();
#elif UNITY_ANDROID
        FilePaths = Directory.GetFiles(path);
#endif
        FileAmount = FilePaths.Length;

        Instance.DirectoryReadComplete(FileAmount);

        string[] titles = new string[FileAmount];

        //load all images in default folder as textures
        TextureDetails = new DetailedImage[FileAmount];

        for(int i = 0; i < FileAmount; i++)
        {
            string tempPath = _PathPrefix + FilePaths[i];
            string title = Path.GetFileNameWithoutExtension(tempPath);
            int index = i + 1;
            ImageBlob imageBlob = new ImageBlob(title, index, FilePaths[i]);
            Instance.ImageLoadStart(imageBlob);

            Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
            WWW www = new WWW(tempPath);
            yield return www;
            www.LoadImageIntoTexture(tex);

            DetailedImage image = new DetailedImage();
            image.Texture2D = tex;
            image.Title = title;
            TextureDetails[i] = image;

            if (index >= FileAmount)
            {
                Instance.ImageLoadComplete("Complete");
            }
        }
    }

    public void DirectoryReadComplete(int result)
    {
        if (OnDirectoryReadComplete != null)
        {
            OnDirectoryReadComplete(result);
        }
    }

    public void ImageLoadStart(ImageBlob result)
    {
        if (OnImageLoadStart != null)
        {
            OnImageLoadStart(result);
        }
    }

    public void ImageLoadComplete(string result)
    {
        if (OnImageLoadComplete != null)
        {
            OnImageLoadComplete(result);
        }
    }

    public string GetStreamingAssetsPath(string folderName)
    {
        // Put your file to "YOUR_UNITY_PROJ/Assets/StreamingAssets"
        // example: "YOUR_UNITY_PROJ/Assets/StreamingAssets/db.bytes"
        string dbPath = "";

        if (Application.platform == RuntimePlatform.Android)
        {
            // Android
            string oriPath = System.IO.Path.Combine(Application.streamingAssetsPath, folderName);
            string realPath = "";
            // Android only use WWW to read file
            WWW reader = new WWW(oriPath);
            while (!reader.isDone) { }

            realPath = Application.persistentDataPath + "/" + folderName;
            System.IO.File.WriteAllBytes(realPath, reader.bytes);

            dbPath = realPath;
        }
        else
        {
            // iOS
            dbPath = System.IO.Path.Combine(Application.streamingAssetsPath, "db.bytes");
        }

        return dbPath;
    }
}

public class DetailedImage
{
    public Texture2D Texture2D;
    public string Title;
}
