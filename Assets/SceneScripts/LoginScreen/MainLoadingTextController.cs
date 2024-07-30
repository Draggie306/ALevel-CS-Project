using System.Collections;
using UnityEngine;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using System.Runtime.Serialization;
using System.Collections.Generic;
using TMPro;
using UnityEngine.PlayerLoop;


public class MainLoadingTextController : MonoBehaviour
{
    public string serverBaseDirectoryUrl = "https://client.draggie.games";
    [SerializeField]
    private GameObject loadingText;
    [SerializeField]
    private string baseTextureUrl = "https://assets.draggie.games/saturnian-content/";
    [SerializeField]
    private string AssetsJsonHostname = "https://assets.draggie.games/";
    [SerializeField]
    private string ManifestFile = "saturnian-content/manifest.json";
    [SerializeField]
    private GameObject CanvasRootForLogin = null;

    void UpdateMainScreenText(string text)
    // This is the (smaller) text in the middle next to the loading spinner
    {
        loadingText.GetComponent<TMPro.TextMeshProUGUI>().text = text;
    }

    void UpdateLegacyInfoText(string text, string optionalColour = "00ACFF")
    // The larger text below the loading spinner
    {
        var informationText = GameObject.Find("StatusText");

        string hex = optionalColour;
        Color colour; // this came from some stackoverflow post I can't remember
        if (ColorUtility.TryParseHtmlString($"#{hex}", out colour))
        {
            informationText.GetComponent<TextMeshProUGUI>().color = colour;
        }
        else
        {
            informationText.GetComponent<TextMeshProUGUI>().color = new Color32(0, 0, 0, 255);
        }

        informationText.GetComponent<TextMeshProUGUI>().text = text;
    }


    public async void EntrypointFunction()
    {
        Debug.Log("[MainLoadingTxtCtrl] Logging in");

        UpdateLegacyInfoText("Checking connectivity...", "00ACFF");
        EstablishConnection();

        UpdateLegacyInfoText("Downloading textures...");
        DownloadRequiredTextures();

        UpdateLegacyInfoText("Logging in...");
        MainLogin();
    }

    public async void MainLogin()
    {
        Debug.Log("[MainLoadingTxtCtrl] Logging in");
        UpdateLegacyInfoText("Logging in...", "00ACFF");
        UpdateMainScreenText("Logging in");

        // Call login function from another script
        CanvasRootForLogin.GetComponent<login>().LoginEvent();

    }


    async void Start()
    {
        Debug.Log($"MainLoadingTextController intialised on GameObject: {gameObject.name}");
        UpdateMainScreenText("Connecting to server...");
        EstablishConnection();
    }

    // Establish connection to the server
    public async void EstablishConnection()
    {
        Debug.Log("[MainLoadingTxtCtrl] Establishing connection to the server");
        UpdateMainScreenText("Checking connectivity...");

        // Get https://client.draggie.games

        using (HttpClient client = new())
        {
            HttpResponseMessage response = await client.GetAsync(serverBaseDirectoryUrl);
            if (response.IsSuccessStatusCode)
            {
                UpdateMainScreenText("Connection successful, continuing...");
                EntrypointFunction();
            }
            else
            {
                UpdateMainScreenText("Connection failure");
                UpdateLegacyInfoText("Unable to connect to Draggie Games servers. Please check your internet connection and try again.", "FF0000");
                Debug.LogError("[MainLoadingTxtCtrl] Unable to connect to Draggie Games servers. Please check your internet connection and try again.");
            }
        }
    }

    /// START OF AI GENERATED CODE ///

    public async void DownloadRequiredTextures()
    {
        loadingText.GetComponent<TMPro.TextMeshProUGUI>().text = "Downloading textures...";

        UpdateMainScreenText("Fetching manifest...");

        using HttpClient client = new();
        var ManifestJsonNet = await client.GetAsync($"{AssetsJsonHostname}{ManifestFile}");
        var ManifestJsonNetContent = await ManifestJsonNet.Content.ReadAsStringAsync();

        Debug.Log($"[MainLoadingTxtCtrl] Response: {ManifestJsonNetContent}");

        // Process the json
        if (ManifestJsonNet.StatusCode != HttpStatusCode.OK)
        {
            Debug.LogError($"[MainLoadingTxtCtrl] Error downloading manifest: {ManifestJsonNet.StatusCode}");
        }
        else
        {
            try
            {
                dynamic parsedJsonResponse = JObject.Parse(ManifestJsonNetContent);
                Debug.Log($"[MainLoadingTxtCtrl] Parsed JSON: {parsedJsonResponse}");

                string CMSAssetBaseURL = parsedJsonResponse.assets_base_url;

                UpdateLegacyInfoText("Required textures need downloading...", "00ACFF");

                StartCoroutine(ProcessAssets(parsedJsonResponse, CMSAssetBaseURL));
            }
            catch (JsonReaderException e)
            {
                Debug.LogError($"[MainLoadingTxtCtrl] Error parsing JSON: {e.Message}");
            }
        }
    }

    private IEnumerator ProcessAssets(dynamic parsedJsonResponse, string CMSAssetBaseURL)
    {
        int NumFilesIterated = 0;

        foreach (var Asset in parsedJsonResponse.files)
        {
            Debug.Log($"[MainLoadingTxtCtrl] Asset: {Asset}");

            CMSObject obj = new()
            {
                Name = Asset.name,
                Cacheable = Asset.cacheable,
                DisplayName = Asset.displayName
            };
            loadingText.GetComponent<TMPro.TextMeshProUGUI>().text = $"Downloading {obj.DisplayName} ({NumFilesIterated + 1}/{parsedJsonResponse.num_items})";

            Debug.Log($"Passing control to the object with name {obj.Name} - url {CMSAssetBaseURL}{obj.Name}");

            yield return StartCoroutine(ProcessCMSObject(CMSAssetBaseURL, obj));

            NumFilesIterated++;
        }
    }

    /// END OF AI GENERATED CODE ///

    // Below is the original code that was replaced by the AI generated code. Don't know how the AI generated code works, but it does. :/

    /*
    public async void DownloadRequiredTextures()
    {
        loadingText.GetComponent<TMPro.TextMeshProUGUI>().text = "Downloading textures...";

        using (HttpClient client = new()) {
            var ManifestJsonNet = await client.GetAsync($"{AssetsJsonHostname}{ManifestFile}");
            var ManifestJsonNetContent= await ManifestJsonNet.Content.ReadAsStringAsync();

            Debug.Log($"[MainLoadingTxtCtrl] Response: {ManifestJsonNetContent}");

            // Process the json
            if (ManifestJsonNet.StatusCode != HttpStatusCode.OK)
            { Debug.LogError($"[MainLoadingTxtCtrl] Error downloading manifest: {ManifestJsonNet.StatusCode}"); } else {
            try {
                int NumFilesIterated = 0;
                dynamic parsedJsonResponse = JObject.Parse(ManifestJsonNetContent);
                Debug.Log($"[MainLoadingTxtCtrl] Parsed JSON: {parsedJsonResponse}");

                string CMSAssetBaseURL = parsedJsonResponse.assets_base_url;

                foreach (var Asset in parsedJsonResponse.files)
                {
                    Debug.Log($"[MainLoadingTxtCtrl] Asset: {Asset}");
                    
                    CMSObject obj = new()
                    {
                        Name = Asset.name,
                        Cacheable = Asset.cacheable,
                        DisplayName = Asset.displayName
                    };
                    loadingText.GetComponent<TMPro.TextMeshProUGUI>().text = $"Downloading {obj.DisplayName} ({NumFilesIterated}/{parsedJsonResponse.num_items})";

                    Debug.Log($"Passing control to the object with name {obj.Name} - url {CMSAssetBaseURL}{obj.Name}");

                    StartCoroutine(ProcessCMSObject(CMSAssetBaseURL, obj));

                    NumFilesIterated++;
                }
            } catch (JsonReaderException e) {
                Debug.LogError($"[MainLoadingTxtCtrl] Error parsing JSON: {e.Message}");
            } 
            } // Cursed bracket placement
        }
    }


    */

    public class CMSObject
    {
        public string Name { get; set; } // To fetch the download URL.
        public string Cacheable { get; set; }
        public string DisplayName { get; set; }
    }

    private IEnumerator ProcessCMSObject(string DownloadHost, CMSObject obj)
    {
        Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Processing {obj.Name}");
        string url = $"{DownloadHost}{obj.Name}";

        string CMSPathDir = Application.persistentDataPath + "/CMS/";

        if (System.IO.File.Exists($"{CMSPathDir}{obj.Name}"))
        {
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] File already exists. Not downloading {obj.Name}");
            UpdateMainScreenText($"Verified {obj.DisplayName}");
            yield return new WaitForSeconds(0.03f);
            yield break;
        } else {
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] File does not exist. Downloading {obj.Name}");
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Starting download for {obj.Name}");
            var www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Downloaded {www.downloadedBytes} bytes from {url}");

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[MainLoadingTxtCtrl/ProcessCMSObject] Error downloading texture from {url}: {www.error}");
                yield return null;
            }
            else
            {
                Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Texture download for {obj.Name} complete");

                if (obj.Cacheable == "true")
                {
                    Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Caching texture for {gameObject.name}");

                    if (!System.IO.Directory.Exists(CMSPathDir))
                        System.IO.Directory.CreateDirectory(CMSPathDir);
                    Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Created directory {CMSPathDir}");

                    string path = $"{CMSPathDir}{obj.Name}";

                    System.IO.File.WriteAllBytes(path, www.downloadHandler.data);

                    Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Texture saved to {path}");
                    //yield return new WaitForSeconds(0.15f);
                }
                else
                {
                    Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Not caching texture for {gameObject.name} but download complete.");
                }
            }
        }
    }
}
