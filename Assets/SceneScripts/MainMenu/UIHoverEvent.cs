using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

/// <summary>
/// I have given up with trying to play a video from url as a texture on a specific object only if the mouse hovers over it. Too complicated.
/// </summary>

public class UIHoverEvent : MonoBehaviour
{
    [Header("Hover Settings")]
    public string HoverTextureDownloadURL = null; // optional. if not, then use a local texture
    public Texture2D HoverTexture = null; 
    public bool CacheMeIfYouCan = true; // Save to CMS directory on the disk
    public bool IsVideo = false;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log($"Initialised on {gameObject.name}; HoverTextureDownloadURL: {HoverTextureDownloadURL}; HoverTexture: {HoverTexture}");

        // downlaod the texture if the URL is not null
        if (HoverTextureDownloadURL != null)
        {
            StartCoroutine(GetTexture(HoverTextureDownloadURL));
        }
    }
    void OnPointerEnter(PointerEventData eventData)
    {
        // If your mouse hovers over the GameObject with the script attached, output this message
        Debug.Log($"[UIHoverEvent] Mouse is over {gameObject.name}");
        if (HoverTexture != null)
        {
            if (IsVideo)
            {
                // Change GameObject texture to the video. Using VideoPlayer component
                Debug.Log($"[UIHoverEvent/OnMouseOver] Changing texture to video for {gameObject.name}");

                // Check if VideoPlayer component already exists
                var videoPlayer = gameObject.GetComponent<UnityEngine.Video.VideoPlayer>();
                if (videoPlayer == null)
                {
                    videoPlayer = gameObject.AddComponent<UnityEngine.Video.VideoPlayer>();
                }

                // Play on awake defaults to true. Set it to false to avoid the url set
                // below to auto-start playback since we're in Start().
                videoPlayer.playOnAwake = false;

                // Create a new RenderTexture
                RenderTexture renderTexture = new RenderTexture(256, 256, 16);
                videoPlayer.targetTexture = renderTexture;

                // Set the video to play. URL supports local absolute or relative paths.
                // Here, using absolute.
                videoPlayer.url = HoverTextureDownloadURL;

                // Skip the first 100 frames.
                videoPlayer.frame = 100;

                // Restart from beginning when done.
                videoPlayer.isLooping = true;

                // Each time we reach the end, we slow down the playback by a factor of 10.
                videoPlayer.loopPointReached += EndReached;

                // Start playback. This means the VideoPlayer may have to prepare (reserve
                // resources, pre-load a few frames, etc.). To better control the delays
                // associated with this preparation one can use videoPlayer.Prepare() along with
                // its prepareCompleted event.  
                videoPlayer.Play();

                // Assign the RenderTexture to the GameObject's Image component
                gameObject.GetComponent<UnityEngine.UI.RawImage>().texture = renderTexture;
            }
            else
            {
                gameObject.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create(HoverTexture, new Rect(0, 0, HoverTexture.width, HoverTexture.height), new Vector2(0.5f, 0.5f));
            }
        }
        else
        {
            Debug.Log($"[UIHoverEvent/OnMouseOver] HoverTexture is null for {gameObject.name}");
        }
    }
    
    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        vp.playbackSpeed = vp.playbackSpeed / 10.0F;
    }



    public IEnumerator GetTexture(string url) {
        Debug.Log($"[UIHoverEvent/GetTexture] Starting download for {gameObject.name}");
        var initialStartTime = Time.time;

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url, nonReadable: false);
        yield return www.SendWebRequest();

        Debug.Log($"[UIHoverEvent/GetTexture] Texture download for {gameObject.name} complete"); 

        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError($"[UIHoverEvent/GetTexture] Error downloading texture from {url}: {www.error}");
        }
        else {
            Debug.Log($"[UIHoverEvent/GetTexture] Texture downloaded for {gameObject.name} with {www.downloadedBytes} bytes in {Time.time - initialStartTime} seconds");
            Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;

            gameObject.GetComponent<UnityEngine.UI.Image>().sprite = Sprite.Create((Texture2D)myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(0.5f, 0.5f));
            HoverTexture = (Texture2D)myTexture;

            if (CacheMeIfYouCan) {
                Debug.Log($"[UIHoverEvent/GetTexture] Caching texture for {gameObject.name}");
                // Save to disk. 
                // Firstly, check if there is a "CMS" directory. I saw this for similarly downloaded files for the game Fortnite, with the "CMS" fodler, so I copied it.
                if (!System.IO.Directory.Exists(Application.persistentDataPath + "/CMS/")) { 
                    System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/CMS/");
                }
                string path = Application.persistentDataPath + "/CMS/" + url.Substring(url.LastIndexOf('/') + 1); // <- This is the filename, i.e. after the last slash in the url
                System.IO.File.WriteAllBytes(path, www.downloadHandler.data);
                Debug.Log($"[UIHoverEvent/GetTexture] Texture saved to {path}");
        }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
