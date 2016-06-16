using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class ARPanelVideo : MonoBehaviour 
{    
    public string path;
    public string subPath;

    public Text VideoParams;

    string videoPath;
    FileInfo videoInfo;

    // Use this for initialization
    void Start()
    {
        if(Application.platform == RuntimePlatform.WindowsEditor)
            path = string.Format("{0}/{1}", Application.streamingAssetsPath, subPath);
        else if (Application.platform == RuntimePlatform.Android)
        {
            Debugger.Instance.Log(ARDirectoryManager.VideoPath);
            path = ARDirectoryManager.VideoPath;
        }

        var dir = new DirectoryInfo(path);
        Debugger.Instance.Log(dir.FullName);
        var dirFiles = Directory.GetFiles(path);
        var dirFileInfos = dir.GetFiles();
        for(int i = 0; i < dirFiles.Length; i++)
        {
            if(dirFiles[i].Contains(".meta"))
                continue;
            Debugger.Instance.Log(dirFiles[i]);
            videoPath = dirFiles[i];
            videoInfo = dirFileInfos[i];
        }

        VideoParams.text = string.Format(
            "Title: {0}",
            videoInfo.Name
            );
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    public void LaunchVideo()
    {
        Handheld.PlayFullScreenMovie(videoPath);
    }
}
