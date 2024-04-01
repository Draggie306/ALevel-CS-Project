using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net; 
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq; // writeup: talk about nuget for unity in writeup
using Newtonsoft.Json;
using TMPro;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.IO; 

/// <summary>
/// The brand new login script!
/// When the login button is clicked, this script is called
/// </summary>

public class initialUsernameJsonDict {
    // TODO: naming rule violation fix - PascalCase (future development)
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
	// writeup: talk about this - use SerialiseField from what Harry said in class?
	
	// "... to account for this, a good rule of thumb is to only expose properties and methods, not fields."
	// see https://forum.unity.com/threads/reason-s-to-serializefield-a-private-variable.513806/ for further 		
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
            // TODO: I have absolutely no idea how to do this
            Debug.Log($"[WriteEncryptedAuthToken] Attempting to write encrypted auth token to file");
            string path = Application.persistentDataPath + "/credentials.bin";
            Debug.Log($"[WriteEncryptedAuthToken] Path: {path}");
            string encryptionKey = "MIIBITANBgkqhkiG9w0BAQEFAAOCAQ4AMIIBCQKCAQBKDqhnbAbXU+ZvYQ+HY/Sk5roKCImfXKG0b1jtcgVNpJlfKc9pKIQ0eOJoSrgYbA6CvvG38NxM/WIcecHp2K4aD9OOJZC+c2FXQGN/eJKA67/w1E8QdpSK7u2hHiHA/bLUvU3QxCIx9EmghGnO94/cubtSYROjTZ1ZNlo3RvZ0UFZYEiixz3kx89DqbtOETxWfzWVZ5naBOg5Vhp7zlnVFRLbOqAs8ZYnHdFIkgNp4ArzyLmshgVyDzXvJaTV9gFi1KawLvpEbQEELNeM9ZLAGqA2wpxDYdjKQTsfgjnqp+DjiY3+kxiDWUK57ZCfNV6JEy9wKVQV4SJ2iWiRH6L6VAgMBAAE=";
            string initTokenString = initToken.ToString();
            byte[] token = Encoding.UTF8.GetBytes(initTokenString);

            var rsa = new RSACryptoServiceProvider(2048);
            // abridged from https://stackoverflow.com/questions/17128038/c-sharp-rsa-encryption-decryption-with-transmission
            {
                try
                {
                    rsa.FromXmlString(encryptionKey);
                    var encryptedToken = rsa.Encrypt(token, RSAEncryptionPadding.Pkcs1);

                    // json makes it easy for server to do things
                    string json = JsonConvert.SerializeObject(new { token = Convert.ToBase64String(encryptedToken) });
    
                    // Write to file (TODO: Can We jus tuse playerprefs when I get time)
                    using (StreamWriter writer = new StreamWriter(path, true))
                    {
                        writer.WriteLine($"{json}");
                        Debug.Log($"[WriteEncryptedAuthToken] Wrote encrypted auth token to file successfully");
                    }
                }
                finally
                {
                    // no idea what this does but it was in the example code
					// https://gist.github.com/gashupl/27e4de6bd8f021f3d61b3122e6bbf775
					// todo: read more into https://stackoverflow.com/questions/13486715/net-rsa-createephemeralkey-vs-persistkeyincsp
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
        Color colour; // this came from some stackoverflow post I can't remember
        if (ColorUtility.TryParseHtmlString($"#{hex}", out colour))
        {
            informationText.GetComponent<TextMeshProUGUI>().color = colour;
        }

        // informationText.GetComponent<TextMeshProUGUI>().color = new Color32(0, 0, 0, 255);
        Debug.Log($"[updateInformationMessage] Displayed {message}");
    }

    // Check out the typo below:
    public async void OnLoginButtonClocked() // called when the login button is clicked
    {
        //Text statusText = GameObject.Find("StatusText").GetComponent<Text>();

        //var errorText = GameObject.Find("ErrorText");
        //Debug.Log($"line 34: {errorText}");

        try {
			// Todo: use private attributes or ScriptingAPI [SerializeField]
            Debug.Log("Login button clicked!");

            GameObject inputField = GameObject.Find("TMP_Password");  // This should be the input parent field and not the child text field.
            string email = GameObject.Find("emailText").GetComponent<TextMeshProUGUI>().text;

            //string password = GameObject.Find("PassText").GetComponent<TextMeshProUGUI>().text; // Comment this out when writeup done: this is not good!!

            string password = inputField.GetComponent<TMP_InputField>().text; // https://discussions.unity.com/t/how-to-get-text-from-textmeshpro-input-field/215584
            // string password = GameObject.Find("PassText").GetComponent<TMP_InputField>().text; 
            // 0.0.9: changed from InputField to TMP_InputField as the password field was returning "***" instead of the password
            // the TextMeshProUGUI component is just the visual representation of the text. When the TMP_InputField is set to the "password" mode, the TextMeshProUGUI component will only show ***s!
            // Taken from: https://forum.unity.com/threads/change-inputfield-input-from-standard-to-password-text-via-script.291897/

            GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().text = "Logging in...";
            Debug.Log($"Email: {email}, Password: {password}");

            // Sanitise email and password, remove u200b (zero width space) and trim
            // for some reason, the input field adds a zero width space to the end of the string
			// writeup: talk about this
            email = email.Replace("\u200b", "");
            password = password.Replace("\u200b", "");
			
			// Now removes extra trailing or leading white spaces, (https://learn.microsoft.com/en-us/dotnet/api/system.string.trim?view=net-8.0)
			// which may not have been caught in the zero width space incident?
            email = email.Trim(); 
            password = password.Trim();
			// whywhywhywhywhywhywhywhywhywhywhywhy

            // basic test for no email or password
            if (email == "" || password == "")
            {
                //ChangeErrorMessage("Please enter an email and password");
                updateInformationMessage("Please enter an email and password!");
                GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().text = "Login";
                GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().enabled = true;
                return; // early return to prevent further execution of fnction
            } else {
                Debug.Log("Email and password seem to have been entered");
            }

            var loginAccount = new initialUsernameJsonDict
            {
                email = email,
                password = password,
                scope = "unity/draggiegames-compsciproject" // this can also be called the user agent
            };

            string jsonString = JsonConvert.SerializeObject(loginAccount); // JsonConvert.SerializeObject converts the object to a JSON string, which is what the server expects
            updateInformationMessage("Logging in to Draggie Games account...");

            using (HttpClient client = new())
            // TODO: Use UnityWebRequest instead of HttpClient because it's probably better
            {
                var response = await client.PostAsync($"{serverBaseDirectoryUrl}/login", 
                new StringContent(jsonString, Encoding.UTF8, "application/json"));

                var responseString = await response.Content.ReadAsStringAsync();
                var responseStatusCode = response.StatusCode;

                
                if (responseStatusCode != HttpStatusCode.OK)
                {
                    try
                    {
                        // these are handled by the server
                        dynamic parsedJsonResponse = JObject.Parse(responseString); // Todo: change this to a defined class attribute or something from known server)
						// docs: "The dynamic type is a static type, but dynamic objects bypass static type checking"
                        ChangeErrorMessage($"An error has occurred.\n\nHTTP status code: [error {responseStatusCode}]\nDetailed message: {parsedJsonResponse.message}");
                        updateInformationMessage($"{parsedJsonResponse.message}. Please try again.");
                    }
                    catch (Exception e)
					// If the server is not online, then there may be 501 errors, and the response from e.g. Cloudflare may not include a JSON object to parse, throwing this
					// This may also be thrown if the user is offline.
					// todo: test offline functionality more. -> use AutoTokenlogin
                    {
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
				// docs: "The dynamic type is a static type, but dynamic objects bypass static type checking"
                updateInformationMessage($"Checking user entitlements...");
                var accessToken = parsedResponse.auth_token; // This is very important, as it acts as a session cookie. If someone gets this, they can log in as you.
                var username = $"{parsedResponse.account}";
                Debug.Log($"[OnLoginButtonClicked] Access token: {accessToken}");
                // PlayerPrefs.SetString("accessToken", accessToken);

                // Check for user access to the game (alpha test entitlement check)

                var secureTokenRequest = new SecureTokenRequest
                {
                    token = accessToken,
                    email = email
                };

                // TODO: use unity web request instead of httpclient?!
                async void checkEntitlements() 
                {
                    string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(secureTokenRequest);
                    using HttpClient client = new();

                    // use Headers for Servers Version 0.8.6 and above
                    // Nicked from https://stackoverflow.com/questions/29801195/adding-headers-when-using-httpclient-getasync
                    const string ValidationPath = "/api/v1/saturnian/game/gameData/licenses/validation";
                    client.DefaultRequestHeaders.Add("Authorisation", $"{accessToken}"); // TODO: Maybe change to American spelling for proper standardiZation of RESTful API
                    client.DefaultRequestHeaders.Add("User-Agent", "unity/draggiegames-compsciproject");

                    var response = await client.GetAsync($"{serverBaseDirectoryUrl}{ValidationPath}"); // Must use await in an async function
                    var responseString = await response.Content.ReadAsStringAsync();
                    var responseStatusCode = response.StatusCode;
                    if (responseStatusCode != HttpStatusCode.OK)
                    {
                        try
                        {
                            dynamic parsedJsonResponse = JObject.Parse(responseString);
                            ChangeErrorMessage($"An error has occurred.\n[{responseStatusCode}]\n\nDetailed message: {parsedJsonResponse.message}");
                            updateInformationMessage($"{parsedJsonResponse.message}. Please try again.");
                        }
                        catch (Exception e)
                        {
                            ChangeErrorMessage($"The server had a difficulty handling your request! Sorry about that.\n\nRaw error: {e}");
                            updateInformationMessage($"Technical error. Please try again.");
                        } // todo: add finally? 
                        GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().text = "Login";
                        GameObject.Find("LoginText").GetComponent<TextMeshProUGUI>().enabled = true;
                        return;
                    }
                    // The try catch block above means that only if the server has handled the message one way or another, it is okay to be processed below
                    updateInformationMessage($"Saving credentials...");

                    dynamic parsedResponse = JObject.Parse(responseString);
                    Debug.Log($"[DEBUG/JSONDUMP] {parsedResponse}");
                    var entitlements = parsedResponse.type;

                    // new entitlements: iterate over each entitlement and check if it is valid
                    // if it is, then allow in


                    Debug.Log($"Checking entitlements for saturnian_alpha_tester or saturnian_beta_tester");
                    bool access = false;

                    // Use new entitlements api vs single old json
                    foreach (var entitlement in parsedResponse.entitlements)
                    {
                        Debug.Log($"[DEBUG/JSONDUMP] Entitlement: {entitlement}");
                        var EntitlementId = entitlement.Name; // this is the entitlement ID
                        if (EntitlementId == "saturnian_alpha_tester")
                        {
                            access = true;
                        }
                        if (EntitlementId == "saturnian_beta_tester")
                        {
                            access = true;
                        }
                    }

                    if (access)
                    {
                        // allow in
                        WriteEncryptedAuthToken(accessToken);
                        // writeup: note: had difficultu seeing where there was an issue here. must stringify the token else it can't write to playerprefs.
                        updateInformationMessage($"Loading game...");
                        PlayerPrefs.SetString("DraggieGamesEmail", email);
                        Debug.Log($"[SavePlayerPrefs] Saved email to PlayerPrefs ({email})");
                        PlayerPrefs.SetString("SaturnianUsername", $"{username}");
                        Debug.Log($"[SavePlayerPrefs] Saved username to PlayerPrefs ({username})");
                        PlayerPrefs.SetString("accessToken", $"{accessToken}"); // todo: make this more secure, playerprefs not seciure
                        Debug.Log($"[SavePlayerPrefs] Saved accessToken to PlayerPrefs ({accessToken})");

                        // SceneManager.LoadScene("MainScene", LoadSceneMode.Single); // this will load the main scene whilst unloading the login scene
                        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single); // this will load the main scene whilst unloading the login scene

                        // var LoggedInAS = GameObject.Find("LoggedInAs");
                        // LoggedInAS.GetComponent<TextMeshProUGUI>().text = $"Logged in as: {username} ({email})";
                    }
                    else
                    {
                        ChangeErrorMessage("You do not have access to the alpha test. Please visit the website to redeem your test key.");
                        updateInformationMessage("You do not have access to the alpha test. Please visit the website to redeem your test key.", "FF0000");
                        Application.Quit();
                    }
                }
                checkEntitlements();
            }
        } catch (Exception e) {
			// writeup: somewhere about textmeshpro in dev of written solutoin
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
        Debug.Log($"Initialised login.cs from GameObject {gameObject.name}");
        // Password input field from https://forum.unity.com/threads/changing-inputfield-contenttype-via-script-does-not-update-text.935525/
        var passwordInput = GameObject.Find("PassText");
        Debug.Log($"[Start] passwordInput: {passwordInput}");
		// docs: writeup: This is important to show stars in the password (*****) instead of the actual password (security risk)
		// writeup: End user requested this feature
        passwordInput.GetComponent<TMP_InputField>().contentType = TMP_InputField.ContentType.Password;
        Debug.Log($"[Start] changed passwordInput.contentType to password");
		// todo: codereview: Just a thought, is passwordfield.cs now needed any more? Doesn't it do the same as the above?

        // now we need to passText.textComponent.SetAllDirty();

        var passwordInputField = GameObject.Find("PassText");
        if (passwordInputField != null) // Just checking... not sure when this won't be True but good practice.
        {
            var pInputted = passwordInput.GetComponent<TMP_InputField>();
            if (pInputted != null && pInputted.textComponent != null)
            {
                pInputted.contentType = TMP_InputField.ContentType.Password;
                pInputted.textComponent.SetAllDirty();
				
				// writeup: docs: Note that despite the sheer lack of documentation about SetAllDirty func it marks the graphic as having been changed. 
				// The Scripting API literally just says: "Mark the Graphic as dirty."
				// Found this out in https://github.com/Pinkuburu/Unity-Technologies-ui/blob/master/UnityEngine.UI/UI/Core/Graphic.cs. Go to line 251 and it's on line 266
            }
            else
            {
                Debug.LogError("TMP_InputField or its textComponent is missing on PassText (if it is not in the scene this is ezpected)");
				// We need the password input field to have a text component to actually get it.
            }
        }
        else
        {
            Debug.LogError("PassText GameObject not found");
			// Most likely only in scenes where there is this specific script loaded but 
        }
    }
}
