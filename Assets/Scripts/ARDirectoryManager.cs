using UnityEngine;
using System.Collections;
using System.IO;

public class ARDirectoryManager : MonoBehaviour {

    static ARDirectoryManager _Instance;
    public static ARDirectoryManager Instance
    {
        get {
            return _Instance;
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
            }

            _VideoPath = path;
            return _VideoPath;
        }
    }

    void Awake()
    {
        _Instance = this;
    }

    void OnDestroy()
    {
        _Instance = null;
    }

	// Use this for initialization
	void Start ()
    {
        if(Application.platform == RuntimePlatform.Android)
            Debugger.Instance.Log("ARDirectoryManager: " + VideoPath);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
