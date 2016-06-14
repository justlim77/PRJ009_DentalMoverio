using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Text))]
public class ARHeaderText : MonoBehaviour {

    Text _label;

    void Awake()
    {
        _label = GetComponent<Text>();

        Core.SubscribeEvent("OnUpdateHeader", OnUpdateHeader);
    }

    object OnUpdateHeader(object sender, object args)
    {
        if (args is string)
        { 
            string header = (string)args;
            _label.text = header;
        }

        return null;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
