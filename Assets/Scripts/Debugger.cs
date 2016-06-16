using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Debugger : MonoBehaviour{

    static Debugger _Instance;
    public static Debugger Instance
    {
        get
        {
            return _Instance;
        }
    }

    public bool enableDebug = true;
    public Text debugLabel;

    List<string> logList = new List<string>();


    void Awake()
    {
        _Instance = this;
        debugLabel.text = string.Empty;
        EnableDebug(enableDebug);
    }

    void Start()
    {
    }

    void OnDestroy()
    {
        _Instance = null;
    }

    public void Log(object obj)
    {
        string log = obj.ToString();
        logList.Add(log);
        debugLabel.text += string.Format(">{0}\n", log);
    }

    void EnableDebug(bool val)
    {
        debugLabel.enabled = val;
    }
}
