using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Net.Http;

// Reference: https://docs.unity3d.com/Manual/UnityWebRequest-RetrievingTexture.html

public class DraggieGamesCDNDownload : MonoBehaviour {

    [SerializeField]
    private string url = "https://assets.draggie.games/saturnian-content/tbd.png"; // public so that it can be set withing the editor after dragging script as a compoentne

    [SerializeField]
    private bool CacheMeIfYouCan = true; // Save to CMS directory on the disk
    void Start() {
        Debug.Log($"[RemoteTextureDL] Downloading for '{gameObject.name}' from url {url}");

        /*
        // Add a random delay to try and fix the issue where some images would time out
        int delay = UnityEngine.Random.Range(1, 5);

        WaitForSeconds wait = new WaitForSeconds(delay);
        Debug.Log($"[RemoteTextureDL] Waiting for {delay} seconds before downloading texture for {gameObject.name}");
        */

        Debug.Log($"[RemoteTextureDL] Starting coroutine for {gameObject.name}");
        StartCoroutine(GetTexture());
    }

    void CheckForSpinnyCircle() {
        // Created in v0.1.5 to accomodate cache without copying more code
        if (gameObject.GetComponent<LoadingCircle>().enabled == true) {
            Debug.Log("[RemoteTextureDL] LoadingCircle is enabled. Disabling");
            gameObject.GetComponent<LoadingCircle>().enabled = false;
            gameObject.GetComponent<UnityEngine.UI.Image>().type = UnityEngine.UI.Image.Type.Simple;
            // rotations 
            Debug.Log($"[RemoteTextureDL] Rotating {gameObject.name} to default rotation");
            // we dont use transform.rotate 0,0,0 as that movex relatively, 0 wouldnt do anything. quaternion euler is absolute (source: https://docs.unity3d.com/Manual/QuaternionAndEulerRotationsInUnity.html)
            gameObject.GetComponent<UnityEngine.UI.Image>().transform.rotation = Quaternion.Euler(0, 0, 0); 
        } else {
            Debug.Log("[RemoteTextureDL] LoadingCircle is disabled. Not doing anything");
        }
    }
 
    IEnumerator GetTexture() {
        Debug.Log($"[RemoteTextureDL] Starting download for {gameObject.name}");
        var initialStartTime = Time.time;
        // lots of networking stuff from https://docs.unity3d.com/Manual/UnityWebRequest.html

        // Specifically unity get texure docs are https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequestTexture.GetTexture.html

        if (CacheMeIfYouCan) {
            Debug.Log($"[RemoteTextureDL] Checking if texture is cached for {gameObject.name}...");
            string path = Application.persistentDataPath + "/CMS/" + url.Substring(url.LastIndexOf('/') + 1);
            if (System.IO.File.Exists(path)) {
                Debug.Log($"[RemoteTextureDL] Texture is cached for {gameObject.name}");
                byte[] fileData = System.IO.File.ReadAllBytes(path);
                Texture2D tex = new Texture2D(2, 2); // it says 2, 2 in the docs, not sure why https://docs.unity3d.com/530/Documentation/ScriptReference/Texture2D.LoadImage.html
                tex.LoadImage(fileData); // ahh this is probably when it resisez before uploading to gpu
                gameObject.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                CheckForSpinnyCircle(); // nice work separating this into its own function :)
                yield break; // This is to exit coroutine early
            } else {
                Debug.Log($"[RemoteTextureDL] Result: Not cached for {gameObject.name}");
            }
        }

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url, nonReadable: false); // non readable is true as we dont need to read the texture data, just apply it to the image
        yield return www.SendWebRequest(); /// aaaargh im so confused why is it only working for some images and not others
        // this woks:   https://assets.draggie.games/saturnian-content/locker-assets/xstep-small.png
        // this doesnt: https://assets.draggie.games/saturnian-content/locker-assets/timemachine2.png

        Debug.Log($"[RemoteTextureDL] Texture download for {gameObject.name} complete");

        if (CacheMeIfYouCan) {
            Debug.Log($"[RemoteTextureDL] Caching texture for {gameObject.name}");
            // Save to disk. 
            // Firstly, check if there is a "CMS" directory. I saw this for similarly downloaded files for the game Fortnite, with the "CMS" fodler, so I copied it.
            if (!System.IO.Directory.Exists(Application.persistentDataPath + "/CMS/")) { 
                System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/CMS/");
            }
            string path = Application.persistentDataPath + "/CMS/" + url.Substring(url.LastIndexOf('/') + 1); // <- This is the filename, i.e. after the last slash in the url
            System.IO.File.WriteAllBytes(path, www.downloadHandler.data);
            Debug.Log($"[RemoteTextureDL] Texture saved to {path}");
        }

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError($"[RemoteTextureDL] Error downloading texture from {url}: {www.error}");
        }
        else {
            Debug.Log($"[RemoteTextureDL] Texture downloaded for {gameObject.name} with {www.downloadedBytes} bytes in {Time.time - initialStartTime} seconds");
            Debug.Log($"[RemoteTextureDL] Estimated download speed: {www.downloadedBytes / Time.time - initialStartTime / 1024} KB/s");
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            // Apply texture to button target graphic

            // Used: https://docs.unity3d.com/2018.2/Documentation/ScriptReference/UI.Image-sprite.html, https://docs.unity3d.com/2018.2/Documentation/ScriptReference/Sprite.Create.html
            // This took a while to figure out https://forum.unity.com/threads/load-png-into-sprite-then-draw-sprite-onto-screen.433489/

            gameObject.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create((Texture2D)myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(0.5f, 0.5f));
        }
        // check if the spinner is acitive. change image type to simple if it is

        // GetComponent<LoadingCircle>().enabled = false;

        CheckForSpinnyCircle();
    }
}