using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public class Debugger : MonoBehaviour
{
    static Debugger _Instance;
    public static Debugger Instance
    {
        get
        {
            return _Instance;
        }
    }

    public bool enableDebug = true;
    public GameObject debugPanel;
    public Text debugLabel;
    public Scrollbar scrollbar;

    List<string> logList = new List<string>();
    StringBuilder _sb = new StringBuilder();

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
        if (!enableDebug)
            return;

        string log = obj.ToString();
        logList.Add(log);
        _sb.AppendLine(log);

        if (logList.Count > 20)
        {
            logList.RemoveRange(0, 20);
        }

        debugLabel.text = "";

        for (int i = 0; i < logList.Count; ++i)
        {
            debugLabel.text += string.Format(">{0}\n", logList[i]);
        }

        //debugLabel.text += string.Format(">{0}\n", log);
        scrollbar.value = 0;
    }

    void EnableDebug(bool val)
    {
        if (debugPanel != null)
            debugPanel.SetActive(val);
    }

    public void EnableDebug()
    {
        if (debugPanel != null)
            debugPanel.SetActive(!debugPanel.activeSelf);
    }
}
