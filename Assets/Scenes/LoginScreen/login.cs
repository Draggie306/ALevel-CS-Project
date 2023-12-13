using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net; 
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq; // talk about nuget for unity in writeup
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.IO; 

public class initialUsernameJsonDict {
    // TODO: naming rule violation fix
    public string email { get; set; }
    public string password { get; set; }
    #nullable enable
    public string? scope { get; set; } = null; 
    #nullable disable
    // As attribute is optional, we can nullify it to improve static flow analysis
    // https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references
}
public class SecureTokenRequest {
    public string token { get; set; }
    public string email { get; set; }
}

public class login : MonoBehaviour
{
    public InputField emailInput;
    public InputField passwordInput;
    public string serverBaseDirectoryUrl = "https://client.draggie.games";

    public void ChangeErrorMessage(string message)
    {
        var errorText = GameObject.Find("ErrorText");
        errorText.GetComponent<TextMeshProUGUI>().text = message;
        Debug.Log($"[ChangeErrorMessage] Displayed {message}");
        // TODO: make this a coroutine so it disappears after a few seconds
        // wait 5 seconds:
    }

    public void WriteEncryptedAuthToken(dynamic initToken)
    {
        try
        {
            Debug.Log($"[WriteEncryptedAuthToken] Attempting to write encrypted auth token to file");
            string path = Application.persistentDataPath + "/credentials.bin";
            Debug.Log($"[WriteEncryptedAuthToken] Path: {path}");
            string encryptionKey = "MIIBITANBgkqhkiG9w0BAQEFAAOCAQ4AMIIBCQKCAQBKDqhnbAbXU+ZvYQ+HY/Sk5roKCImfXKG0b1jtcgVNpJlfKc9pKIQ0eOJoSrgYbA6CvvG38NxM/WIcecHp2K4aD9OOJZC+c2FXQGN/eJKA67/w1E8QdpSK7u2hHiHA/bLUvU3QxCIx9EmghGnO94/cubtSYROjTZ1ZNlo3RvZ0UFZYEiixz3kx89DqbtOETxWfzWVZ5naBOg5Vhp7zlnVFRLbOqAs8ZYnHdFIkgNp4ArzyLmshgVyDzXvJaTV9gFi1KawLvpEbQEELNeM9ZLAGqA2wpxDYdjKQTsfgjnqp+DjiY3+kxiDWUK57ZCfNV6JEy9wKVQV4SJ2iWiRH6L6VAgMBAAE=";
            byte[] token = Encoding.UTF8.GetBytes(initToken);

            using (var rsa = new RSACryptoServiceProvider(2048))
            // abridged from https://stackoverflow.com/questions/17128038/c-sharp-rsa-encryption-decryption-with-transmission
            {
                try
                {
                    rsa.FromXmlString(encryptionKey);
                    var encryptedToken = rsa.Encrypt(token, RSAEncryptionPadding.Pkcs1);

                    // Serialize to JSON
                    string json = JsonConvert.SerializeObject(new { token = Convert.ToBase64String(encryptedToken) });

                    // Write to file
                    using (StreamWriter writer = new StreamWriter(path, true))
                    {
                        writer.WriteLine(json);
                    }
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[WriteEncryptedAuthToken] Exception: {e}");
            ChangeErrorMessage($"[Non-fatal] Unable to write encrypted auth token to file. Please contact the developer: {e}");
        }

    }

    public void updateInformationMessage(string message, string optionalColour = "00ACFF")
    {
        var informationText = GameObject.Find("StatusText");
        informationText.GetComponent<TextMeshProUGUI>().text = message;

        string hex = optionalColour;
        Color colour;
        if (ColorUtility.TryParseHtmlString($"#{hex}", out colour))
        {
            informationText.GetComponent<TextMeshProUGUI>().color = colour;
        }

        // informationText.GetComponent<TextMeshProUGUI>().color = new Color32(0, 0, 0, 255);
        Debug.Log($"[updateInformationMessage] Displayed {message}");
    }

    public async void OnLoginButtonClocked() // called when the login button is clicked
    {
        //Text statusText = GameObject.Find("StatusText").GetComponent<Text>();

        //var errorText = GameObject.Find("ErrorText");
        //Debug.Log($"line 34: {errorText}");

        try {
            Debug.Log("Login button clicked");
    
            string email = GameObject.Find("emailText").GetComponent<TextMeshProUGUI>().text;
            string password = GameObject.Find("PassText").GetComponent<TextMeshProUGUI>().text;

            GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().text = "Logging in...";
            

            Debug.Log($"Email: {email}, Password: {password}");

            // Sanitise email and password, remove u200b (zero width space) and trim
            // for some reason, the input field adds a zero width space to the end of the string
            email = email.Replace("\u200b", "");
            password = password.Replace("\u200b", "");
            email = email.Trim();
            password = password.Trim();

            // if no email or password
            if (email == "" || password == "")
            {
                //ChangeErrorMessage("Please enter an email and password");
                updateInformationMessage("Please enter an email and password!");
                GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().text = "Login";
                GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().enabled = true;
                return;
            } else {
                Debug.Log("Email and password seem to have been entered");
            }

            var loginAccount = new initialUsernameJsonDict
            {
                email = email,
                password = password,
                scope = "unity/draggiegames-compsciproject" // this can also be called the user agent
            };

            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(loginAccount); // JsonConvert.SerializeObject converts the object to a JSON string, which is what the server expects
            updateInformationMessage("Logging in to Draggie Games account...");

            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{serverBaseDirectoryUrl}/login", new StringContent(jsonString, Encoding.UTF8, "application/json"));

                var responseString = await response.Content.ReadAsStringAsync();
                var responseStatusCode = response.StatusCode;

                if (responseStatusCode != HttpStatusCode.OK)
                {
                    try
                    {
                        // these are handled by the server
                        dynamic parsedJsonResponse = JObject.Parse(responseString);
                        ChangeErrorMessage($"An error has occurred.\n\nHTTP status code: [error {responseStatusCode}]\nDetailed message: {parsedJsonResponse.message}");
                        updateInformationMessage($"{parsedJsonResponse.message}. Please try again.");
                    }
                    catch (Exception e)
                    {
                        // server can be tempramental so we need to handle unhandled server errors (does that make sense?)
                        ChangeErrorMessage($"It looks like the login server is currently experiencing issues. Please try again later!\n\n\nTechnical details: {e}");
                        updateInformationMessage("Login server error. Please try again later.", "FF0000");
                    }
                    GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().text = "Login";
                    GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().enabled = true;
                    return;
                }
                // If response status code IS 200 (OK)
                Debug.Log($"[OnLoginButtonClicked] it seems like it worked! {responseString}");
                
                dynamic parsedResponse = JObject.Parse(responseString);
                updateInformationMessage($"Checking user entitlements...");
                var accessToken = parsedResponse.auth_token; // This is very important, as it acts as a session cookie. If someone gets this, they can log in as you.
                Debug.Log($"[OnLoginButtonClicked] Access token: {accessToken}");
                // PlayerPrefs.SetString("accessToken", accessToken);

                // Check for user access to the game (alpha test entitlement check)

                var secureTokenRequest = new SecureTokenRequest
                {
                    token = accessToken,
                    email = email
                };

                async void checkEntitlements() 
                {
                    string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(secureTokenRequest);
                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetAsync($"{serverBaseDirectoryUrl}/api/v1/saturnian/game/gameData/licenses/validation?token={accessToken}");
                        var responseString = await response.Content.ReadAsStringAsync();
                        var responseStatusCode = response.StatusCode;
                        if (responseStatusCode != HttpStatusCode.OK) {
                            try {
                                dynamic parsedJsonResponse = JObject.Parse(responseString);
                                ChangeErrorMessage($"An error has occurred.\n\nHTTP status code: [error {responseStatusCode}]\nDetailed message: {parsedJsonResponse.message}");
                                updateInformationMessage($"{parsedJsonResponse.message}. Please try again.");
                            } catch (Exception e) {
                                ChangeErrorMessage($"The server had a difficulty handling your request! Sorry about that.\n\nRaw error: {e}");
                                updateInformationMessage($"Technical error. Please try again.");
                            }
                            GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().text = "Login";
                            GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().enabled = true;
                            return;
                        }
                        // else it is okay
                        updateInformationMessage($"Saving credentials...");

                        dynamic parsedResponse = JObject.Parse(responseString);
                        var entitlements = parsedResponse.type;

                        if (entitlements == "alpha") {
                            // allow in
                            WriteEncryptedAuthToken(accessToken);
                            updateInformationMessage($"Loading game...");
                            //StartCoroutine(DelayScene(6));

                            // https://forum.unity.com/threads/calling-function-from-other-scripts-c.57072/
                            // myObject.GetComponent<MyScript>().MyFunction();

                            var LoadingObjectInsideLoadingScene = GameObject.Find("LoadingScene");
                            LoadingObjectInsideLoadingScene.GetComponent<LoadingScreen>().LoadScene(1);

                            // SceneManager.LoadScene("MainScene", LoadSceneMode.Single); // this will load the main scene whilst unloading the login scene
                            
                            var LoggedInAS = GameObject.Find("LoggedInAs");
                            LoggedInAS.GetComponent<TextMeshProUGUI>().text = $"Logged in as: {parsedResponse.account} ({parsedResponse.email})";
                        } else {
                            ChangeErrorMessage("You do not have access to the alpha test. Please contact the developer for more information.");
                            updateInformationMessage("You do not have access to the alpha test. Please contact the developer for more information.");
                            Application.Quit();
                        }
                    }
                }
                checkEntitlements();
            }
        } catch (Exception e) {
            GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().text = "Login";
            GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().enabled = true;
            Debug.LogError($"An error has occurred: {e}");
            var error_object = GameObject.Find("ErrorText");
            Debug.LogError(error_object);
            // use textmeshpro
            error_object.GetComponent<TextMeshProUGUI>().text = $"An error has occurred: {e}";
        }
    }

    IEnumerator DelayScene(int seconds = 2)
    {
        yield return new WaitForSeconds(seconds);
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Initialised login.cs in loading scene");
        // Password input field from https://forum.unity.com/threads/changing-inputfield-contenttype-via-script-does-not-update-text.935525/
        var passwordInput = GameObject.Find("PassText");
        Debug.Log($"[Start] passwordInput: {passwordInput}");
        passwordInput.GetComponent<TMP_InputField>().contentType = TMP_InputField.ContentType.Password;
        Debug.Log($"[Start] changed passwordInput.contentType to password");

        // now we need to passText.textComponent.SetAllDirty();

        var passwordInputField = GameObject.Find("PassText");
        if (passwordInputField != null)
        {
            var pInputted = passwordInput.GetComponent<TMP_InputField>();
            if (pInputted != null && pInputted.textComponent != null)
            {
                pInputted.contentType = TMP_InputField.ContentType.Password;
                pInputted.textComponent.SetAllDirty();
            }
            else
            {
                Debug.LogError("TMP_InputField or its textComponent is missing");
            }
        }
        else
        {
            Debug.LogError("PassText GameObject not found");
        }


    }
}
