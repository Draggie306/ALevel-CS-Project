using System.Collections;
using UnityEngine;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using TMPro;
using System.Threading.Tasks;


public class MainLoadingTextController : MonoBehaviour
{
    [Header("API URLs")]
    public string serverBaseDirectoryUrl = "https://client.draggie.games";
    [SerializeField]
    private string baseTextureUrl = "https://assets.draggie.games/saturnian-content/";
    [SerializeField]
    private string AssetsJsonHostname = "https://assets.draggie.games/";
    [SerializeField]
    private string ManifestFile = "saturnian-content/manifest.json";

    [Header("Game Objects")]
    [SerializeField]
    private GameObject loadingText;
    private GameObject CanvasRootForLogin = null;
    [SerializeField]
    private GameObject SpinnerIcon = null;

    void UpdateMainScreenText(string text)
    // This is the (smaller) text in the middle next to the loading spinner
    {
        loadingText.GetComponent<TMPro.TextMeshProUGUI>().text = text;

        Debug.Log($"[MainLoadingTxtCtrl/UpdateMainScreenText] Updated centre text to: {text}");
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
        Debug.Log($"[MainLoadingTxtCtrl/UpdateLegacyInfoText] Updated legacy text to: {text}");
    }

    public async Task EntrypointFunction()
{
    SpinnerIcon.SetActive(true);
    Debug.Log("[MainLoadingTxtCtrl/EntrypointFunction] Entry point function called");

    UpdateLegacyInfoText("Checking connectivity...", "00ACFF");
    bool connectionEstablished = await EstablishConnection();
    if (!connectionEstablished)
    {
        UpdateLegacyInfoText("Unable to connect to Draggie Games servers. Please check your internet connection and try again.", "FF0000");
        Debug.Log("[MainLoadingTxtCtrl/EntrypointFunction] Failed to establish connection.");
        return;
    }

    UpdateLegacyInfoText("[MainLoadingTxtCtrl/EntrypointFunction] Downloading textures...");
    bool texturesDownloaded = await DownloadRequiredTextures();
    if (!texturesDownloaded)
    {
        UpdateLegacyInfoText("Error downloading assets. Please try again later.", "FF0000");
        Debug.Log("[MainLoadingTxtCtrl/EntrypointFunction] Failed to download textures.");
        return;
    } else {
        UpdateMainScreenText("Textures downloaded successfully.");
    }
    Debug.Log("[MainLoadingTxtCtrl/EntrypointFunction] DownloadRequiredTextures() finished!");

    UpdateLegacyInfoText("Passing control to login screen...");
    UpdateMainScreenText(""); // Renu
    await MainLogin();
}

    public async Task MainLogin()
    {
        Debug.Log("[MainLoadingTxtCtrl] Logging in");
        UpdateLegacyInfoText("Logging in from MainLogin...", "00ACFF");
        UpdateMainScreenText("Logging in");

        // Call login function from another script
        CanvasRootForLogin.GetComponent<login>().LoginEvent();

    }


    async void Start()
    {
        Debug.Log($"MainLoadingTextController intialised on GameObject: {gameObject.name}");
        UpdateMainScreenText("Press any key to continue...");
    }

    bool hasPressedKey = false;

    async void Update()
    {
        if (Input.anyKeyDown && !hasPressedKey)
        {
            hasPressedKey = true;
            await EntrypointFunction();
        }
    }

    // Establish connection to the server
    public async Task<bool> EstablishConnection()
    {
        Debug.Log("[MainLoadingTxtCtrl/EstablishConnection] Function called");
        UpdateMainScreenText("Checking connectivity...");

        // Get https://client.draggie.games

        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(serverBaseDirectoryUrl);
        if (response.IsSuccessStatusCode)
        {
            UpdateMainScreenText("Connection successful, continuing...");
            return true;
        }
        else
        {
            UpdateMainScreenText("Connection failure");
            UpdateLegacyInfoText("Unable to connect to Draggie Games servers. Please check your internet connection and try again.", "FF0000");
            Debug.LogError("[MainLoadingTxtCtrl] Unable to connect to Draggie Games servers. Please check your internet connection and try again.");
            return false;
        }
    }

    /// START OF AI GENERATED CODE ///

    public async Task<bool> DownloadRequiredTextures()
    {
        UpdateLegacyInfoText("Downloading textures...", "00ACFF");
        UpdateMainScreenText("Fetching manifest...");

        using HttpClient client = new();
        var ManifestJsonNet = await client.GetAsync($"{AssetsJsonHostname}{ManifestFile}");
        var ManifestJsonNetContent = await ManifestJsonNet.Content.ReadAsStringAsync();

        Debug.Log($"[MainLoadingTxtCtrl/DownloadRequiredTextures] getManifest response received.");

        // Process the json
        if (ManifestJsonNet.StatusCode != HttpStatusCode.OK)
        {
            Debug.LogError($"[MainLoadingTxtCtrl/DownloadRequiredTextures] Error downloading manifest: {ManifestJsonNet.StatusCode}");
            UpdateLegacyInfoText($"Error {ManifestJsonNet.StatusCode} downloading manifest, please try again later.", "FF0000");
            return false;
        }
        else
        {
            try
            {
                Debug.Log("[MainLoadingTxtCtrl/DownloadRequiredTextures] Attempting to parse JSON.");
                dynamic parsedJsonResponse = JObject.Parse(ManifestJsonNetContent);
                Debug.Log($"[MainLoadingTxtCtrl/DownloadRequiredTextures] Parsed JSON: {parsedJsonResponse}");

                string CMSAssetBaseURL = parsedJsonResponse.assets_base_url;

                UpdateLegacyInfoText("Required textures need downloading...", "00ACFF");

                Debug.Log("[MainLoadingTxtCtrl/DownloadRequiredTextures] Starting coroutine ProcessAssets.");
                bool success = await ProcessAssetsAsync(parsedJsonResponse, CMSAssetBaseURL);

                if (success)
                {
                    UpdateMainScreenText("Processing assets...");
                    Debug.Log("[MainLoadingTxtCtrl/DownloadRequiredTextures] Returning from function.");
                    return true;
                }
                else
                {
                    UpdateLegacyInfoText("Error processing assets.", "FF0000");
                    return false;
                }
            }
            catch (JsonReaderException e)
            {
                Debug.LogError($"[MainLoadingTxtCtrl/DownloadRequiredTextures] Error parsing JSON: {e.Message}");
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[MainLoadingTxtCtrl/DownloadRequiredTextures] Error: {e.Message}");
                return false;
            }
        }
    }

    private Task<bool> ProcessAssetsAsync(dynamic parsedJsonResponse, string CMSAssetBaseURL)
    {
        var tcs = new TaskCompletionSource<bool>();

        StartCoroutine(ProcessAssetsCoroutine(parsedJsonResponse, CMSAssetBaseURL, tcs));

        return tcs.Task;
    }

    private IEnumerator ProcessAssetsCoroutine(dynamic parsedJsonResponse, string CMSAssetBaseURL, TaskCompletionSource<bool> tcs)
    {
        Debug.Log($"[MainLoadingTxtCtrl] ProcessAssets called with {parsedJsonResponse.num_items} items");
        int NumFilesIterated = 0;

        foreach (var Asset in parsedJsonResponse.files)
        {
            Debug.Log($"[MainLoadingTxtCtrl] Asset: {Asset}, Iteration: {NumFilesIterated}");

            CMSObject obj = new()
            {
                Name = Asset.name,
                Cacheable = Asset.cacheable,
                DisplayName = Asset.displayName
            };

            loadingText.GetComponent<TMPro.TextMeshProUGUI>().text = $"Downloading {obj.DisplayName} ({NumFilesIterated + 1}/{parsedJsonResponse.num_items})";

            Debug.Log($"Passing control to the object with name {obj.Name} - url {CMSAssetBaseURL}{obj.Name}");

            bool success = false;
            try
            {
                StartCoroutine(ProcessCMSObject(CMSAssetBaseURL, obj));
                success = true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MainLoadingTxtCtrl/ProcessAssets] Exception: {ex.Message}");
                tcs.SetResult(false);
                yield break;
            }

            if (!success)
            {
                tcs.SetResult(false);
                yield break;
            }

            NumFilesIterated++;
        }

        Debug.Log("[MainLoadingTxtCtrl/ProcessAssets] Coroutine completed.");
        tcs.SetResult(true);
        yield return null;
    }

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
        string filePath = $"{CMSPathDir}{obj.Name}";
    
        Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Checking if file exists at {filePath}");
    
        if (System.IO.File.Exists(filePath))
        {
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] File already exists. Not downloading {obj.Name}");
            UpdateMainScreenText($"Verified {obj.DisplayName}");
            yield break;
        }
        else
        {
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] File does not exist. Downloading {obj.Name}");
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Starting download for {obj.Name}");
            var www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Downloaded {www.downloadedBytes} bytes from {url}");
    
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[MainLoadingTxtCtrl/ProcessCMSObject] Error downloading from {url}: {www.error}");
                Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Exiting function. [f]");
                yield break;
            }
            else
            {
                Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Download for {obj.Name} complete");
    
                if (obj.Cacheable == "true")
                {
                    Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Caching object for {gameObject.name}");
    
                    if (!System.IO.Directory.Exists(CMSPathDir))
                        System.IO.Directory.CreateDirectory(CMSPathDir);
                    Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Created directory {CMSPathDir}");
    
                    System.IO.File.WriteAllBytes(filePath, www.downloadHandler.data);
    
                    Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Object saved to {filePath}");
                }
                else
                {
                    Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Not caching object for {gameObject.name} but download complete.");
                }
                Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Exiting function. [s]");
                yield return null;
            }
        }
        Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Exiting function. [e]");
        yield return null;
    }
}
