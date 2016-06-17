using UnityEngine;
using System.Collections;
using System.IO;

public class ARDirectoryManager : MonoBehaviour
{
    static ARDirectoryManager _Instance;
    public static ARDirectoryManager Instance
    {
        get 
        {
            return _Instance;
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
            string path = Path.Combine(Application.persistentDataPath, "videos");
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

	// Use this for initialization
	void Start ()
    {
	}
	
	// Update is called once per frame
	void Update () {
	
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
}
