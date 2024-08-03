using System;
using System.Collections;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NewLoginFlow : MonoBehaviour
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
    private GameObject CanvasRootForLogin = null;
    [SerializeField]
    private GameObject SpinnerIcon = null;
    [SerializeField]
    private GameObject CentreText = null;
    [SerializeField]
    private GameObject LowerLargeText = null;
    private bool HasCliced = false;

    private void Start()
    {
        Debug.Log($"Initialised NewLoginFlow on {gameObject.name}");
        UpdateMainScreenText("Press any key to start");
    }

    private void Update()
    {
        if (Input.anyKey && !HasCliced)
        {
            
            HasCliced = true;
            Debug.Log("Let's go!");
            StartCoroutine(StartLogin());
        }
    }

    public class CMSObject
    {
        public string Name { get; set; } // To fetch the download URL.
        public string Cacheable { get; set; }
        public string DisplayName { get; set; }
    }

    private void UpdateMainScreenText(string text)
    {
        CentreText.GetComponent<TextMeshProUGUI>().text = text;
    }

    private void UpdateLegacyInfoText(string text, string color)
    {
        LowerLargeText.GetComponent<TextMeshProUGUI>().text = text;
        LowerLargeText.GetComponent<TextMeshProUGUI>().color = ColorUtility.TryParseHtmlString($"#{color}", out Color newColor) ? newColor : Color.white;
    }

    private IEnumerator StartLogin()
    {
        CentreText.GetComponent<TextMeshProUGUI>().text = "Loading...";
        LowerLargeText.GetComponent<TextMeshProUGUI>().text = "Please wait";
        SpinnerIcon.SetActive(true);

        CentreText.GetComponent<TextMeshProUGUI>().text = "Downloading assets...";
        yield return StartCoroutine(RunAsync(DownloadRequiredTextures));

        yield return new WaitForSeconds(1);
        UpdateMainScreenText("Assets downloaded. Starting game...");
        SceneManager.LoadScene("MainMenu"); 
    }




    // texture download.
    // before you ask, yes, this is a mess. I'm sorry.
    // I had no clue how to mix async with coroutines so it is AI generated. im sorry.

    public async Task<bool> DownloadRequiredTextures()
    {
        UpdateLegacyInfoText("Downloading textures...", "00ACFF");
        UpdateMainScreenText("Fetching manifest...");

        using HttpClient client = new();
        HttpResponseMessage ManifestJsonNet;
        string ManifestJsonNetContent;

        try
        {
            ManifestJsonNet = await client.GetAsync($"{AssetsJsonHostname}{ManifestFile}");
            ManifestJsonNetContent = await ManifestJsonNet.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"[MainLoadingTxtCtrl/DownloadRequiredTextures] Error downloading manifest: {e.Message}");
            UpdateLegacyInfoText($"Error downloading manifest, please try again later.", "FF0000");
            return false;
        }

        Debug.Log("[MainLoadingTxtCtrl/DownloadRequiredTextures] getManifest response received.");

        if (ManifestJsonNet.StatusCode != HttpStatusCode.OK)
        {
            Debug.LogError($"[MainLoadingTxtCtrl/DownloadRequiredTextures] Error downloading manifest: {ManifestJsonNet.StatusCode}");
            UpdateLegacyInfoText($"Error {ManifestJsonNet.StatusCode} downloading manifest, please try again later.", "FF0000");
            return false;
        }

        try
        {
            Debug.Log("[MainLoadingTxtCtrl/DownloadRequiredTextures] Attempting to parse JSON.");
            dynamic parsedJsonResponse = JObject.Parse(ManifestJsonNetContent);
            Debug.Log($"[MainLoadingTxtCtrl/DownloadRequiredTextures] Parsed JSON: {parsedJsonResponse}");

            string CMSAssetBaseURL = parsedJsonResponse.assets_base_url;

            UpdateLegacyInfoText("Required textures need downloading...", "00ACFF");

            Debug.Log("[MainLoadingTxtCtrl/DownloadRequiredTextures] Starting ProcessAssetsAsync.");
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

    private async Task<bool> ProcessAssetsAsync(dynamic parsedJsonResponse, string CMSAssetBaseURL)
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

            CentreText.GetComponent<TMPro.TextMeshProUGUI>().text = $"Downloading {obj.DisplayName} ({NumFilesIterated + 1}/{parsedJsonResponse.num_items})";

            Debug.Log($"Passing control to the object with name {obj.Name} - url {CMSAssetBaseURL}{obj.Name}");

            bool success = await ProcessCMSObjectAsync(CMSAssetBaseURL, obj);

            if (!success)
            {
                Debug.LogError($"[MainLoadingTxtCtrl/ProcessAssets] Error processing {obj.Name}");
                return false;
            }

            NumFilesIterated++;
        }

        Debug.Log("[MainLoadingTxtCtrl/ProcessAssets] Completed processing assets.");
        return true;
    }

    private async Task<bool> ProcessCMSObjectAsync(string DownloadHost, CMSObject obj)
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
            return true;
        }
        else
        {
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] File does not exist. Starting download for {obj.Name} from {url}");

            UnityWebRequest www = UnityWebRequest.Get(url);
            await www.SendWebRequestAsync();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[MainLoadingTxtCtrl/ProcessCMSObject] Error downloading from {url}: {www.error}");
                return false;
            }

            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Download for {obj.Name} complete");

            if (obj.Cacheable == "true")
            {
                Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Caching object for {obj.Name}");

                if (!System.IO.Directory.Exists(CMSPathDir))
                    System.IO.Directory.CreateDirectory(CMSPathDir);

                System.IO.File.WriteAllBytes(filePath, www.downloadHandler.data);

                Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Object saved to {filePath}");
            }
            else
            {
                Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Not caching object for {obj.Name} but download complete.");
            }

            return true;
        }
    }


    // Wrapper to run async methods in coroutines
    private IEnumerator RunAsync(Func<Task> asyncAction)
    {
        var task = asyncAction();
        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.Exception != null)
        {
            throw task.Exception;
        }
    }
}


public static class UnityWebRequestAsyncExtensions
{
    public static Task<UnityWebRequest> SendWebRequestAsync(this UnityWebRequest request)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();

        request.SendWebRequest().completed += operation =>
        {
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                tcs.SetException(new Exception(request.error));
            }
            else
            {
                tcs.SetResult(request);
            }
        };

        return tcs.Task;
    }
}