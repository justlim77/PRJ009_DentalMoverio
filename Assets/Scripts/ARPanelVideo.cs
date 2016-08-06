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
        path = ARDirectoryManager.VideoPath;

        var dir = new DirectoryInfo(path);
        //Debugger.Instance.Log(dir.FullName);
        var dirFiles = Directory.GetFiles(path);
        var dirFileInfos = dir.GetFiles();
        Debugger.Instance.Log("ARPanelVideo | Files found: " + dirFiles.Length);
        for (int i = 0; i < dirFiles.Length; i++)
        {
            if(dirFiles[i].Contains(".meta"))
                continue;
            Debugger.Instance.Log("ARPanelVideo: " + dirFiles[i]);
            videoPath = dirFiles[i];
            videoInfo = dirFileInfos[i];
        }

        UpdateVideoInfo(videoInfo);
    }

    string GetVideoPath()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
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
        for (int i = 0; i < dirFiles.Length; i++)
        {
            if (dirFiles[i].Contains(".meta"))
                continue;
            Debugger.Instance.Log(dirFiles[i]);
            videoPath = dirFiles[i];
            videoInfo = dirFileInfos[i];
        }

        return videoPath;
    }

    void UpdateVideoInfo(FileInfo fileInfo)
    {
        string name = fileInfo.Name.Split('.')[0];
        string format = fileInfo.Extension.ToUpper().Substring(1);
        float size = fileInfo.Length / 1024 / 1024;
        VideoParams.text = string.Format(
            "<color=#00ffffff><b>Title</b></color>\n{0}\n\n<color=#00ffffff><b>Format</b></color>\n{1}\n\n<color=#00ffffff><b>Size</b></color>\n{2} MB",
            name, format, (int)size
            );
    }

    public void LaunchVideo()
    {
        string path = GetVideoPath();
        Handheld.PlayFullScreenMovie(path);
    }
}
