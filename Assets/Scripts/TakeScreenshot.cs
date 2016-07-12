using UnityEngine;
using System.Collections;
using System.IO;

public class TakeScreenshot : MonoBehaviour
{
    public Vector2 Left_P = Vector2.zero;

    public void Capture(Resolution res)
    {
        StartCoroutine(ScreenshotEncode(res));
    }

    string GetFileName(int width, int height)
    {
        return string.Format("screen_{0}x{1}_{2}.png", width, height,
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    IEnumerator ScreenshotEncode(Resolution res)
    {
        yield return new WaitForEndOfFrame();
        Texture2D tex = new Texture2D(res.width - (int)Left_P.y, res.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0 + Left_P.y, 0, res.width - Left_P.y, res.height), 0, 0);
        tex.Apply();
        //for (int i = 0; i < hiders.Length; i++)
        //    hiders[i].SetActive(true);
        yield return 0;
        byte[] bytes = tex.EncodeToPNG();
#if UNITY_ANDROID
        File.WriteAllBytes("/mnt/sdcard/DCIM/Images/" + "DentalAR " + GetFileName(res.width, res.height), bytes);
        Debug.Log("New screenshot saved to: " + "/mnt/sdcard/DCIM/Images/" + "DentalAR " + GetFileName(res.width, res.height));
#elif UNITY_EDITOR
        File.WriteAllBytes(Application.dataPath + "/Screenshots/" + GetFileName(res.width, res.height), bytes);
        Debug.Log("New screenshot saved to: " + Application.dataPath + "/Screenshots/" + GetFileName(res.width, res.height));
#endif
    }
}