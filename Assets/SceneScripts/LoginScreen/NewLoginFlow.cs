using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// New login flow at launch of Project Saturnian.
/// Downloads required assets, logs in, and starts the game if all valid.
/// </summary>


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
    [SerializeField]
    private GameObject LoginButtonUIParent = null;
    [SerializeField]
    private GameObject LoginButton = null;

    private bool HasCliced = false;

    private void Start()
    {
        Debug.Log($"Initialised NewLoginFlow on {gameObject.name}");
        UpdateMainScreenText("Press any key to start");
    }

    private void Update()
    {
        // Dev setting
        if (!HasCliced) {
            StartCoroutine(StartSequence());
            HasCliced = true;
        }

        // Production setting
        /*
        if (Input.anyKey && !HasCliced)
        {
            
            HasCliced = true;
            Debug.Log("Let's go!");
            StartCoroutine(StartSequence());
        }
        */
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

    private void UpdateLegacyInfoText(string text, string color = "00ACFF")
    {
        LowerLargeText.GetComponent<TextMeshProUGUI>().text = text;
        LowerLargeText.GetComponent<TextMeshProUGUI>().color = ColorUtility.TryParseHtmlString($"#{color}", out Color newColor) ? newColor : Color.white;
    }

    private IEnumerator StartSequence()
    {
        UpdateMainScreenText("Starting sequence...");
        UpdateLegacyInfoText("Please wait");
        SpinnerIcon.SetActive(true);

        UpdateMainScreenText("Downloading assets...");
        UpdateLegacyInfoText("Downloading required assets...");
        yield return StartCoroutine(RunAsync(DownloadRequiredTextures));
        SpinnerIcon.SetActive(false);

        UpdateMainScreenText("");
        UpdateLegacyInfoText("");
        yield return new WaitForSeconds(1);
        // UpdateMainScreenText("Assets downloaded. Starting game...");

        //SceneManager.LoadScene("MainMenu");
        SpinnerIcon.SetActive(true);
        UpdateMainScreenText("Logging in...");
        UpdateLegacyInfoText("Authenticating with Draggie Games servers...");

        // Login sequence
        bool loginSuccess = false;
        yield return StartCoroutine(RunAsync(async () => loginSuccess = await NetLogin())); // <- AI generated

        if (loginSuccess)
        {
            UpdateMainScreenText("Logged in. Starting game...");
            UpdateLegacyInfoText("Starting game...");
            //yield return new WaitForSeconds(1);
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            UpdateMainScreenText("Error logging in.");
            UpdateLegacyInfoText("Error logging in. Please try again later.", "FF0000");
        }
    }

    private bool HasShownPleaseLogin = false;

    private async Task<bool> NetLogin() {
        // Attempt to detect if the user is already logged in and has a token saved in %appdata%
        
        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Draggie", "Saturnian");

        if (!Directory.Exists(directory))
        {
            if (!HasShownPleaseLogin)
            {
                UpdateLegacyInfoText("Please log in to continue.", "FF0000");
                HasShownPleaseLogin = true;
            }
            // Directory does not exist; the user has not logged in before via the Launcher.
            Debug.Log($"[NetLogin] Draggie directory does not exist at {directory}");
            bool showLoginButtonUi = await ShowLoginButtonUi();
            if (!showLoginButtonUi) {
                Debug.LogError("[NetLogin] Error showing login button UI.");
                return false;
            } else {
                Debug.Log("[NetLogin] Showed login button UI, waiting for user input...");
                initialUsernameJsonDict UserLoginData = await WaitForButtonPress();
                Debug.Log($"[NetLogin] Data: {UserLoginData}");

                string jsonString = JsonConvert.SerializeObject(UserLoginData); // JsonConvert.SerializeObject converts the object to a JSON string, which is what the server expects
                UpdateLegacyInfoText("Logging in to Draggie Games account...");

                using HttpClient client = new();
                var response = await client.PostAsync($"{serverBaseDirectoryUrl}/login", 
                new StringContent(jsonString, Encoding.UTF8, "application/json"));

                var responseString = await response.Content.ReadAsStringAsync();
                var responseStatusCode = response.StatusCode;

                if (responseStatusCode == HttpStatusCode.OK)
                {
                    Debug.Log($"[NetLogin] Login successful: {responseString}");
                    UpdateLegacyInfoText("Login successful!", "00ACFF");
                    LoginButtonUIParent.SetActive(false);
                    SpinnerIcon.SetActive(true);
                    UpdateMainScreenText("Finishing up...");
                    await Task.Delay(1200);
                    return true;
                }
                else
                {
                    Debug.LogError($"[NetLogin] Login failed: {responseString}");

                    try {
                        dynamic parsedJsonResponse = JObject.Parse(responseString);
                        string errorMessage = parsedJsonResponse.message;
                        UpdateLegacyInfoText(errorMessage, "FF0000");
                    } catch (JsonReaderException e) {
                        Debug.LogError($"[NetLogin] Error parsing JSON: {e.Message}");
                        UpdateLegacyInfoText("Login failed. Please try again later.", "FF0000");
                    }
                    
                    // Retry by calling this function again after 3 seconds
                    GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().text = "Log in";
                    await Task.Delay(3000);
                    return await NetLogin();
                }
            }
        } else {
            Debug.Log($"[NetLogin] Draggie directory exists at {directory}");
        }
        return true;
    }

    public async Task<initialUsernameJsonDict> WaitForButtonPress()
    {
        var tcs = new TaskCompletionSource<initialUsernameJsonDict>();
    
        // Assuming you have a reference to the login button
        Button loginButton = LoginButton.GetComponent<Button>();
        loginButton.onClick.AddListener(async () => {
            Debug.Log("[WaitForButtonPress] Login button clicked.");
            var result = await HandleButtonPressAsync();
            if (result != null)
            {
                tcs.SetResult(result);
            }
            else
            {
                Debug.Log("[WaitForButtonPress] Value is still null, retrying...");
            }
        });
    
        return await tcs.Task;
    }

    private async Task<initialUsernameJsonDict> HandleButtonPressAsync()
    {
        return await OnButtonPress();
    }


    public async Task<initialUsernameJsonDict> OnButtonPress()
    {
        Debug.Log("[OnButtonPress] Login button pressed, parsing email and password...");
        GameObject inputField = GameObject.Find("TMP_Password");  // This should be the input parent field and not the child text field.
        string email = GameObject.Find("emailText").GetComponent<TextMeshProUGUI>().text;

        //string password = GameObject.Find("PassText").GetComponent<TextMeshProUGUI>().text; // Comment this out when writeup done: this is not good!!

        string password = inputField.GetComponent<TMP_InputField>().text; // https://discussions.unity.com/t/how-to-get-text-from-textmeshpro-input-field/215584
        // string password = GameObject.Find("PassText").GetComponent<TMP_InputField>().text; 
        // 0.0.9: changed from InputField to TMP_InputField as the password field was returning "***" instead of the password
        // the TextMeshProUGUI component is just the visual representation of the text. When the TMP_InputField is set to the "password" mode, the TextMeshProUGUI component will only show ***s!
        // Taken from: https://forum.unity.com/threads/change-inputfield-input-from-standard-to-password-text-via-script.291897/

        GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().text = "Logging in...";
        Debug.Log($"[OnButtonPress] Email: '{email}', Password: '{password}'");

        // Sanitise email and password, remove u200b (zero width space) and trim. for some reason, the input field adds a zero width space to the end of the string
        email = email.Replace("\u200b", "").Trim();
        password = password.Replace("\u200b", "").Trim();

        // basic test for no email or password
        if (email == "" || password == "")
        {
            Debug.Log("[OnButtonPress] Email or password is empty.");
            //ChangeErrorMessage("Please enter an email and password");
            UpdateLegacyInfoText("Please enter an email and password!", "FF0000");
            return null;
        } else {
            Debug.Log("Email and password seem to have been entered");
            UpdateLegacyInfoText("Attempting to log in...", "00ACFF");
        }

        var loginAccount = new initialUsernameJsonDict // Defined in login.cs
        {
            email = email,
            password = password,
            scope = "unity/draggiegames-compsciproject" // this can also be called the user agent
        };
        return loginAccount;
    }

    private async Task<bool> ShowLoginButtonUi()
    {
        Debug.Log("[ShowLoginButtonUi] Showing login button UI.");
        LoginButtonUIParent.SetActive(true);
        CentreText.SetActive(false);
        SpinnerIcon.SetActive(false);

        return true;
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