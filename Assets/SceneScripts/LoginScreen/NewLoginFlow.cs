using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
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

    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // hack fix for tabbing between fields - just select pasword lol
            GameObject.Find("TMP_Password").GetComponent<TMP_InputField>().Select();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            // Simulate click on login button
            LoginButton.GetComponent<Button>().onClick.Invoke();
        }

        // Dev setting (fast)

        // if (!HasCliced) {
        //     StartCoroutine(StartSequence());
        //     HasCliced = true;
        // }

        // Production setting

        if (Input.anyKey && !HasCliced)
        {
            
            HasCliced = true;
            Debug.Log("Let's go!");
            StartCoroutine(StartSequence());
        }
    }

    public class CMSObject
    {
        public string Name { get; set; } // To fetch the download URL.
        public string Cacheable { get; set; } // Todo: Make it bool
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
        yield return new WaitForSeconds(0.1f);
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
            Debug.LogError("[NewLoginFlow] Error logging in.");
            UpdateMainScreenText("Retrying in 5...");
            yield return new WaitForSeconds(1);
            UpdateMainScreenText("Retrying in 4...");
            yield return new WaitForSeconds(1);
            UpdateMainScreenText("Retrying in 3...");
            yield return new WaitForSeconds(1);
            UpdateMainScreenText("Retrying in 2...");
            yield return new WaitForSeconds(1);
            UpdateMainScreenText("Retrying in 1...");
            yield return new WaitForSeconds(1);
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
                UpdateMainScreenText("Error");
                UpdateLegacyInfoText("Error logging in. Please restart and try again.", "FF0000");
            }
        }
    }
    private bool AlwaysShowLoginUi = false;
    private bool HasShownPleaseLogin = false;

    private async Task<bool> NetLogin() {
        // Attempt to detect if the user is already logged in and has a token saved in %appdata%
        Debug.Log($"[NetLogin] triggered. Flags: AlwaysShowLoginUiTest: {AlwaysShowLoginUi}, HasShownPleaseLogin: {HasShownPleaseLogin}");
        
        string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Draggie", "Saturnian");
        // AlwaysShowLoginUi = false;
        bool DirExists = true;

        if (!Directory.Exists(directory) && !AlwaysShowLoginUi) // If the flag AlwaysShowLoginUiTest is set, ignore this selection statement.
        {
            if (!HasShownPleaseLogin)
            {
                UpdateLegacyInfoText("Please log in to continue.", "FF0000");
                HasShownPleaseLogin = true;
            }
            // Directory does not exist; the user has not logged in before via the Launcher.
            Debug.Log($"[NetLogin] Draggie directory does not exist at {directory}");
            AlwaysShowLoginUi = true;
            DirExists = false;
        } else {
            Debug.Log($"[NetLogin] Draggie directory exists at {directory}");
        }

        if (DirExists && !AlwaysShowLoginUi)
        {
            // Check if the user has a token saved in %appdata%
            string tokenPath = Path.Combine(directory, "token.bin");

            if (File.Exists(tokenPath))
            {
                AlwaysShowLoginUi = true;
                UpdateMainScreenText("Verifying credentials...");

                string username = Environment.UserName.ToLower() + ".3d060a9b-f248-4e2b-babd-e6d5d2c2ab8b";

                using SHA256 sha256 = SHA256.Create();
                byte[] hashKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(username));
                byte[] fernet_key = hashKey;

                string encryptedToken = File.ReadAllText(tokenPath);

                Debug.Log($"[NetLogin] Attempting to decrypt token {encryptedToken} with key {fernet_key}");
                UpdateMainScreenText("Decrypting token...");
                var decryptedToken = DecryptFernet(fernet_key, encryptedToken, out DateTime timestamp);
                Debug.Log($"[NetLogin] Decrypted token: {decryptedToken}");

                // Decrypt token and send to server
                string endpoint = $"{serverBaseDirectoryUrl}/token_login";
                string jsonString = JsonConvert.SerializeObject(new { token = decryptedToken, scope = "unity/draggiegames-compsciproject" });

                using HttpClient client = new();
                UpdateMainScreenText("Sending request...");
                var response = await client.PostAsync(endpoint, new StringContent(jsonString, Encoding.UTF8, "application/json"));

                var responseString = await response.Content.ReadAsStringAsync();
                var responseStatusCode = response.StatusCode;
                
                if (responseStatusCode == HttpStatusCode.OK)
                {
                    Debug.Log($"[NetLogin] Token login successful: {responseString}");
                    UpdateMainScreenText("Checking validity...");

                    string validation_endpoint = $"{serverBaseDirectoryUrl}/api/v1/saturnian/game/gameData/licenses/validation"; // GET request

                    using HttpClient validationClient = new();
                    validationClient.DefaultRequestHeaders.Add("token", decryptedToken);
                    validationClient.DefaultRequestHeaders.Add("User-Agent", "unity/draggiegames-compsciproject");
                    dynamic validationResponse = await validationClient.GetAsync(validation_endpoint);

                    if (validationResponse.StatusCode != HttpStatusCode.OK)
                    {
                        try {
                            string message = JObject.Parse(await validationResponse.Content.ReadAsStringAsync()).message;
                            Debug.LogError($"[NetLogin] Validation failed: {message}");
                            UpdateLegacyInfoText($"Token validation failed: {message}", "FF0000");
                            return false;
                        } catch (JsonReaderException e) {
                            Debug.LogError($"[NetLogin] Error parsing error JSON, likely server error: {e.Message}");
                        }
                        Debug.LogError($"[NetLogin] Validation failed: {validationResponse.StatusCode}");
                        UpdateLegacyInfoText("Validation failed. Please try again later.", "FF0000");
                        return false;
                    }
                    var validationResponseString = await validationResponse.Content.ReadAsStringAsync();
                    Debug.Log($"[NetLogin] Validation response: {validationResponseString}");

                    // Parse response. 
                    dynamic parsedValidationResponse = JObject.Parse(validationResponseString);
                    var entitlements = parsedValidationResponse.entitlements;

                    // saturnian_alpha_tester || saturnian_beta_tester are the entitlements we are looking for
                    bool isAlphaTester = false;
                    bool isBetaTester = false;
                    string TesterMsg = "";

                    Debug.Log($"[NetLogin] Entitlements:\n{entitlements}");

                    foreach (JProperty entitlement in entitlements)
                    {
                        string EntitlementName = entitlement.Name;
                        Debug.Log($"[NetLogin] Entitlement: {entitlement}");

                        if (EntitlementName == "saturnian_alpha_tester") {
                            isAlphaTester = true;
                            TesterMsg = "You are an ALPHA tester! Thank you for testing Saturnian! Don't forget to report bugs!";
                            break;
                        } else if (EntitlementName == "saturnian_beta_tester") {
                            isBetaTester = true;
                            TesterMsg = "You are a beta tester! Thank you for testing Saturnian! Don't forget to report bugs!";
                            break;
                        } else {
                            Debug.Log($"[NetLogin] No known tester entitlements: {entitlement}");
                        }
                    }

                    if (isAlphaTester || isBetaTester)
                    {
                        Debug.Log($"[NetLogin] User is alpha tester: {isAlphaTester}, beta tester: {isBetaTester}");
                        UpdateLegacyInfoText($"Welcome back, {parsedValidationResponse.username}! {TesterMsg}");
                        UpdateMainScreenText("Processing credentials...");
                        //await Task.Delay(634);
                        UpdateMainScreenText("Cleaning up...");
                        return true;
                    }
                    else
                    {
                        Debug.LogError("[NetLogin] User is not an alpha or beta tester.");
                        UpdateLegacyInfoText("You are not an alpha or beta tester.", "FF0000");
                        return false;
                    }
                }
            else
            {
                // Token does not exist; the user has not logged in before.
                Debug.Log($"[NetLogin] Token does not exist at {tokenPath}");
                AlwaysShowLoginUi = true;
            }
            AlwaysShowLoginUi = true;
        } else {
            Debug.Log($"[NetLogin] Draggie directory does not exist at {directory}");
            AlwaysShowLoginUi = true;
        }
        }

        if (AlwaysShowLoginUi)
        {
            Debug.Log("[NetLogin] Showing login UI.");
            bool showLoginButtonUi = await ShowLoginButtonUi();
            LoginButton.GetComponent<Button>().interactable = true;
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
                    UpdateMainScreenText("Processing credentials...");
                    await Task.Delay(634);
                    UpdateMainScreenText("Downloading keychain...");
                    await Task.Delay(123);
                    UpdateMainScreenText("Caching assets...");
                    await Task.Delay(1122);
                    UpdateMainScreenText("Initialising game...");
                    await Task.Delay(600);
                    UpdateMainScreenText("Cleaning up...");
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
                    LoginButton.GetComponent<Button>().interactable = false; // don't confuse the user by allowing spam for 3 seconds
                    await Task.Delay(3000);
                    return await NetLogin();
                }
            }
        }

        UpdateLegacyInfoText("A uncorrectable error occurred. Please try again later; report this to the developers.", "FF0000");
        return false;
    }
    
    /// 
    /// Token decryption from StackOverflow: https://stackoverflow.com/a/50844957
    /// 

    // Token is base64 url encoded
    public static string DecryptFernet(byte[] key, string token, out DateTime timestamp)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (key.Length != 32)
        {
            throw new ArgumentException(nameof(key));
        }

        if (token == null)
        {
            throw new ArgumentNullException(nameof(token));
        }

        byte[] token2 = Base64UrlDecode(token);

        if (token2.Length < 57)
        {
            throw new ArgumentException(nameof(token));
        }

        byte version = token2[0];

        if (version != 0x80)
        {
            throw new Exception("version");
        }

        // Check the hmac
        {
            byte[] signingKey = new byte[16];
            Buffer.BlockCopy(key, 0, signingKey, 0, 16);

            using (var hmac = new HMACSHA256(signingKey))
            {
                hmac.TransformFinalBlock(token2, 0, token2.Length - 32);
                byte[] hash2 = hmac.Hash;

                IEnumerable<byte> hash = token2.Skip(token2.Length - 32).Take(32);

                if (!hash.SequenceEqual(hash2))
                {
                    throw new Exception("Wrong HMAC!");
                }
            }
        }

        {
            // BigEndian to LittleEndian
            long timestamp2 = BitConverter.ToInt64(token2, 1);
            timestamp2 = IPAddress.NetworkToHostOrder(timestamp2);

            timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamp2).UtcDateTime;
        }

        byte[] decrypted;

        using (var aes = new AesManaged())
        {
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            byte[] encryptionKey = new byte[16];
            Buffer.BlockCopy(key, 16, encryptionKey, 0, 16);
            aes.Key = encryptionKey;

            byte[] iv = new byte[16];
            Buffer.BlockCopy(token2, 9, iv, 0, 16);
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor())
            {
                const int startCipherText = 25;
                int cipherTextLength = token2.Length - 32 - 25;
                decrypted = decryptor.TransformFinalBlock(token2, startCipherText, cipherTextLength);
            }
        }

        string FinalString = Encoding.UTF8.GetString(decrypted);
        Debug.Log($"[DecryptFernet] Decrypted token: {FinalString}");

        return FinalString;
    }

    public static byte[] Base64UrlDecode(string s)
    {
        // https://stackoverflow.com/a/26354677/613130
        // But totally rewritten :-)

        char[] chars;

        switch (s.Length % 4)
        {
            case 2:
                chars = new char[s.Length + 2];
                chars[chars.Length - 2] = '=';
                chars[chars.Length - 1] = '=';
                break;
            case 3:
                chars = new char[s.Length + 1];
                chars[chars.Length - 1] = '=';
                break;
            default:
                chars = new char[s.Length];
                break;
        }

        for (int i = 0; i < s.Length; i++)
        {
            switch (s[i])
            {
                case '_':
                    chars[i] = '/';
                    break;
                case '-':
                    chars[i] = '+';
                    break;
                default:
                    chars[i] = s[i];
                    break;
            }
        }

        byte[] result = Convert.FromBase64CharArray(chars, 0, chars.Length);
        return result;
    }


    /// 
    /// End of token decryption from StackOverflow ///
    /// 



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
            GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().text = "Logging in...";
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

        // TODO: fade in/out animations
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

            //CentreText.GetComponent<TMPro.TextMeshProUGUI>().text = $"Downloading {obj.DisplayName} ({NumFilesIterated + 1}/{parsedJsonResponse.num_items})";

            Debug.Log($"Passing control to the object with name {obj.Name} - url {CMSAssetBaseURL}{obj.Name}");

            int TotalFiles = parsedJsonResponse.num_items;

            bool success = await ProcessCMSObjectAsync(CMSAssetBaseURL, obj, TotalFiles, NumFilesIterated);

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

    private async Task<bool> ProcessCMSObjectAsync(string DownloadHost, CMSObject obj, int TotalFiles = 0, int CurrentFile = 0)
    {
        var watch = new System.Diagnostics.Stopwatch(); 
        CurrentFile++;
        Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Processing {obj.DisplayName}");
        // UpdateMainScreenText($"Processing {obj.DisplayName}");
        string url = $"{DownloadHost}{obj.Name}";

        watch.Start();
        string CMSPathDir = Application.persistentDataPath + "/CMS/";
        string filePath = $"{CMSPathDir}{obj.Name}";

        Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Checking if file exists at {filePath}");

        if (File.Exists(filePath) && obj.Cacheable == "True")
        {
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject/dl] Object: {obj.Name} exists? {System.IO.File.Exists(filePath)}. Is cacheable? {obj.Cacheable}. Skipping download.");
            UpdateMainScreenText($"Verified {obj.DisplayName}");
            watch.Stop();
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Time taken to process EXISTING {obj.Name}: {watch.ElapsedMilliseconds}ms");
            return true;
        }
        else
        {
            UpdateMainScreenText($"Downloading {obj.DisplayName} ({CurrentFile}/{TotalFiles})");
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject/dl] Object: {obj.Name} exists? {System.IO.File.Exists(filePath)}. (cacheable: {obj.Cacheable}). Downloading from {url}");

            UnityWebRequest www = UnityWebRequest.Get(url);
            await www.SendWebRequestAsync();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[MainLoadingTxtCtrl/ProcessCMSObject] Error downloading from {url}: {www.error}");
                return false;
            }

            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Download for {obj.Name} complete");

            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Caching object {obj.Name} if cacheable is true ('{obj.Cacheable}')");
            if (obj.Cacheable == "True")
            {
                Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Caching object for {obj.Name}");

                System.IO.File.WriteAllBytes(filePath, www.downloadHandler.data);

                Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Object saved to {filePath}");
            }
            else
            {
                Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Not caching object for {obj.Name} but download complete.");
            }
            watch.Stop();
            Debug.Log($"[MainLoadingTxtCtrl/ProcessCMSObject] Time taken to process NEW {obj.Name}: {watch.ElapsedMilliseconds}ms");
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