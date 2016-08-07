using UnityEngine;
using System.Collections;
using System.IO;
//using ICSharpCode;

public class ARDirectoryManager : MonoBehaviour
{
    static ARDirectoryManager _Instance;
    public static ARDirectoryManager Instance
    {
        get 
        {
            if (_Instance == null)
                Debug.Log("ARDirectoryManager not yet initialized.");
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
            //Debugger.Instance.Log("Image Folder path located at " + path);

            return path;
        }
    }

    static string _FacialPath;
    public static string FacialPath
    {
        get
        {
            string path = Path.Combine(Application.persistentDataPath, "facial");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debugger.Instance.Log("Facial Path not found! Creating folder: " + path);
            }

            _FacialPath = path;
            return _FacialPath;
        }
    }

    static string _FacialFrontPath;
    public static string FacialFrontPath
    {
        get
        {
            string path = Path.Combine(FacialPath, "front");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debugger.Instance.Log("Facial Front Path not found! Creating folder: " + path);
            }

            _FacialFrontPath = path;
            return _FacialFrontPath;
        }
    }

    static string _FacialObliquePath;
    public static string FacialObliquePath
    {
        get
        {
            string path = Path.Combine(FacialPath, "oblique");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debugger.Instance.Log("Facial Oblique Path not found! Creating folder: " + path);
            }

            _FacialObliquePath = path;
            return _FacialObliquePath;
        }
    }

    static string _FacialSidePath;
    public static string FacialSidePath
    {
        get
        {
            string path = Path.Combine(FacialPath, "side");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debugger.Instance.Log("Facial Side Path not found! Creating folder: " + path);
            }

            _FacialSidePath = path;
            return _FacialSidePath;
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
            //string path = Application.persistentDataPath +  "/videos";          
            string path = Path.Combine(Application.persistentDataPath, "videos");          
#endif
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debugger.Instance.Log("Video path not found! Creating folder: " + path);
            }

            //Debugger.Instance.Log("Video Folder Path located at : " + path);

            _VideoPath = path;
            return _VideoPath;
        }
    }

    void Awake()
    {
        _Instance = this;

        //if (Application.platform == RuntimePlatform.Android)
        //{ 
        //    if (InitializeDirectory() == false)
        //        Debugger.Instance.Log("ARDirectoryManager.InitializeDirectory failed to initialize!");
        //    else
        //        Debugger.Instance.Log("ARDirectoryManager.InitializeDirectory succeeded!");
        //}
    }


    void OnDestroy()
    {
        _Instance = null;
    }

    public static Texture2D[] TextureArray;
    public static DetailedImage[] TextureDetails;
    public static string[] FilePaths;
    static string _PathPrefix = "";

    public static IEnumerator LoadLocalImages()
    {
        string path = ImageFolderPath;
#if UNITY_EDITOR
        _PathPrefix = @"file://";
#elif UNITY_ANDROID
        _PathPrefix = "";
#endif
        Debugger.Instance.Log(string.Format("LoadLocalImages image path: {0}", path));
#if UNITY_EDITOR
        var filePaths = Directory.GetFiles(path, "*.jpg");
#elif UNITY_ANDROID
        var filePaths = Directory.GetFiles(path);
#endif
        FilePaths = filePaths;
        Debugger.Instance.Log(string.Format("LoadLocalImages from {0}: {1}", path, FilePaths[0]));

        string[] titles = new string[FilePaths.Length];

        //load all images in default folder as textures
        TextureDetails = new DetailedImage[FilePaths.Length];

        int fileAmount = FilePaths.Length;
        for(int i = 0; i < fileAmount; i++)
        {
            string tempPath = _PathPrefix + FilePaths[i];
            Debugger.Instance.Log(tempPath);
            Debug.Log(tempPath);
            WWW www = new WWW(tempPath);
            yield return www;
            Texture2D tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
            www.LoadImageIntoTexture(tex);

            DetailedImage image = new DetailedImage();

            string title = Path.GetFileNameWithoutExtension(tempPath);
            image.Texture = tex;
            image.Title = title;
            TextureDetails[i] = image;
        }
    }

    bool InitializeDirectory()
    {
        bool result = true;

        if (FacialPath == null)
            result = false;

        if (FacialFrontPath == null)
            result = false;

        if (FacialObliquePath == null)
            result = false;

        if (FacialSidePath == null)
            result = false;

        if (VideoPath == null)
            result = false;

        return result;
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
    public Texture Texture;
    public string Title;
}
