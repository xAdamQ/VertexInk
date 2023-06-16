using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// I used to use an old obj files importer, and used javascript bolb url to obtains the files
/// this is not used anymore
/// </summary>
public class JsMessenger : MonoBehaviour
{
    private IEnumerator GetTexture(string url)
    {
        var www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(www.error);
        }
        else
        {
            var texture2D = DownloadHandlerTexture.GetContent(www);
            var sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height),
                new Vector2(.5f, .5f));
        }
    }

    [ContextMenu("tst texture download")]
    public void TestLoad()
    {
        StartCoroutine(GetTexture("https://dressthenines.com/wp-content/uploads/2020/01/Dua-Lipa.jpg"));
    }

    public void GetFile(string url)
    {
        ObjectLoader.I.Load(url);
    }

    // private IEnumerator GetFileRoutine(string url)
    // {
    //     var www = new UnityWebRequest(url);
    //     www.downloadHandler = new DownloadHandlerBuffer();
    //     yield return www.SendWebRequest();
    //
    //     if (www.result != UnityWebRequest.Result.Success)
    //     {
    //         Debug.Log(www.error);
    //     }
    //     else
    //     {
    //         ObjectLoader.I.Load(www.downloadHandler.data);
    //     }
    // }
}