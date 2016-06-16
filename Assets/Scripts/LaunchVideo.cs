using UnityEngine;
using System.IO;
using System.Collections;

public class LaunchVideo : MonoBehaviour
{
    public string path;
    public string subPath;

	// Use this for initialization
	void Start () {
        path = string.Format("{0}/{1}", Application.streamingAssetsPath, subPath);
        Debug.Log(Path.GetFileName(path));

        foreach (string file in Directory.GetFiles(path))
        {
            if (file.Contains(".meta"))
                continue;
            Debug.Log(file);
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
