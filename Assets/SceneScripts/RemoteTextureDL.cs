using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

// Reference: https://docs.unity3d.com/Manual/UnityWebRequest-RetrievingTexture.html

public class MyBehaviour : MonoBehaviour {

    public string url = "https://assets.draggie.games/saturnian-content/tbd.png"; // public so that it can be set withing the editor after dragging script as a compoentne
    void Start() {
        Debug.Log($"[RemoteTextureDL] Downloading texture for game object {gameObject.name} from url {url}");
        StartCoroutine(GetTexture());
    }
 
    IEnumerator GetTexture() {
        var initialStartTime = Time.time;
        // lots of networking stuff from https://docs.unity3d.com/Manual/UnityWebRequest.html
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        Debug.Log($"[RemoteTextureDL] Texture download for {gameObject.name} complete");

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.Log("[RemoteTextureDL] Error downloading texture: " + www.error);
        }
        else {
            Debug.Log("[RemoteTextureDL] Texture downloaded");
            var endTime = Time.time;
            var timeTaken = endTime - initialStartTime;
            Debug.Log($"[RemoteTextureDL] Texture of {www.downloadedBytes} bytes downloaded in {timeTaken} seconds. Estimated download speed: {www.downloadedBytes / timeTaken / 1024} KB/s");
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            // Apply texture to button target graphic

            // Used: https://docs.unity3d.com/2018.2/Documentation/ScriptReference/UI.Image-sprite.html
            // Used: https://docs.unity3d.com/2018.2/Documentation/ScriptReference/Sprite.Create.html
            // This took a while to figure out https://forum.unity.com/threads/load-png-into-sprite-then-draw-sprite-onto-screen.433489/

            gameObject.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create((Texture2D)myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(0.5f, 0.5f));
        }
        // check if the spinner is acitive. change image type to simple if it is

        // GetComponent<LoadingCircle>().enabled = false;

        if (gameObject.GetComponent<LoadingCircle>().enabled == true) {
            Debug.Log("[RemoteTextureDL] LoadingCircle is enabled. Disabling");
            gameObject.GetComponent<LoadingCircle>().enabled = false;
            gameObject.GetComponent<UnityEngine.UI.Image>().type = UnityEngine.UI.Image.Type.Simple;
            // rotations 
            Debug.Log($"[RemoteTextureDL] Rotating {gameObject.name} to default rotation");
            // we dont use transform.rotate 0,0,0 as that movex relatively, 0 wouldnt do anything. quaternion euler is absolute (source: https://docs.unity3d.com/Manual/QuaternionAndEulerRotationsInUnity.html)
            gameObject.GetComponent<UnityEngine.UI.Image>().transform.rotation = Quaternion.Euler(0, 0, 0); 
        }


    }
}