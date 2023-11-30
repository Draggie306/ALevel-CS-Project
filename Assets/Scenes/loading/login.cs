using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net; 
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq; // talk about nuget for unity in writeup
using TMPro;
public class initialUsernameJsonDict {
    public string email { get; set; }
    public string password { get; set; }
    public string? scope { get; set; } = null;
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

    public void updateInformationMessage(string message)
    {
        var informationText = GameObject.Find("StatusText");
        informationText.GetComponent<TextMeshProUGUI>().text = message;
        Debug.Log($"[updateInformationMessage] Displayed {message}");
    }

    public async void OnLoginButtonClocked()
    {
        //Text statusText = GameObject.Find("StatusText").GetComponent<Text>();

        //var errorText = GameObject.Find("ErrorText");
        //Debug.Log($"line 34: {errorText}");

        try {
            Debug.Log("Login button clicked");
    
            string email = GameObject.Find("emailText").GetComponent<TextMeshProUGUI>().text;
            string password = GameObject.Find("passText").GetComponent<TextMeshProUGUI>().text;

            Debug.Log($"Email: {email}, Password: {password}");

            // Sanitise email and password, remove u200b (zero width space) and trim
            email = email.Replace("\u200b", "");
            password = password.Replace("\u200b", "");
            email = email.Trim();
            password = password.Trim();

            // if no email or password
            if (email == "" || password == "")
            {
                ChangeErrorMessage("Please enter an email and password");
                return;
            } else {
                Debug.Log("Email and password seem to habe been entered");
            }

            var loginAccount = new initialUsernameJsonDict
            {
                email = email,
                password = password,
                scope = "unity/draggiegames-compsciproject"
            };

            string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(loginAccount);

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
                    }
                    catch (Exception e)
                    {
                        // server can be tempramental so we need to handle unhandled server errors (does that make sense?)
                        ChangeErrorMessage($"The server had a difficulty handling your request! Sorry about that.\n\nRaw error: {e}");
                    }
                    return;
                }
                // If response status code IS 200 (OK)
                Debug.Log($"[OnLoginButtonClicked] it seems like it worked! {responseString}");
                
                dynamic parsedResponse = JObject.Parse(responseString);
                updateInformationMessage($"Successfully logged in to Draggie Games account {parsedResponse.account}!");
                var accessToken = parsedResponse.token; // This is very important, as it acts as a session cookie. If someone gets this, they can log in as you.
                Debug.Log($"[OnLoginButtonClicked] Access token: {accessToken}");

            }
        } catch (Exception e) {
            Debug.LogError($"An error has occurred: {e}");
            var error_object = GameObject.Find("ErrorText");
            Debug.LogError(error_object);
            // use textmeshpro
            error_object.GetComponent<TextMeshProUGUI>().text = $"An error has occurred: {e}";
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Started");
    }
}
