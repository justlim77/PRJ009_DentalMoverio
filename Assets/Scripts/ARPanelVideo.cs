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

        string name = videoInfo.Name.Split('.')[0];
        string format = videoInfo.Extension.ToUpper().Substring(1);
        float size = videoInfo.Length / 1024 / 1024;
        VideoParams.text = string.Format(
            "<color=#00ffffff><b>Title</b></color>\n{0}\n\n<color=#00ffffff><b>Format</b></color>\n{1}\n\n<color=#00ffffff><b>Size</b></color>\n{2} MB",
            name, format, (int)size
            );
    }

    public void LaunchVideo()
    {
        Handheld.PlayFullScreenMovie(videoPath);
    }
}
