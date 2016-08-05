using UnityEngine;
using System.Collections;

public static class ExtensionMethods
{
    public static void Clear(this Transform trans)
    {
        for (int i = 0; i < trans.childCount; i++)
        {
            GameObject.Destroy(trans.GetChild(i));
        }
    }
}
