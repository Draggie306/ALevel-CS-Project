using System;
using System.Collections.Generic;
using System.Net; // for http
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json; // backup for newtonsoft
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO; // for file input/output

/// <summary>
/// Old login script
/// Domnt use this please
/// </summary>

public class initialUsernameJsonDict1 {
    public string email { get; set; }
    public string password { get; set; }
    #nullable enable
    public string? scope { get; set; } = null;
    #nullable disable
    // As attribute is optional, we can nullify it to improve static flow analysis
    // https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references
}
public class SecureTokenRequest1 {
    public string token { get; set; }
    public string email { get; set; }
}

class MainProgram
{
    public static void print(string message) { // Python is print() so this name makes it nicer to translate
        // DateTime from https://www.bytehide.com/blog/datetime-now-vs-datetime-utcnow-csharp
        DateTime currentTime = DateTime.Now;
        Console.Write($"[{currentTime}] {message}");
    }
    public static string serverBaseDirectoryUrl = "https://client.draggie.games";
    static async Task Main(string[] args)
    {
        Console.WriteLine("initialised\n");
        Console.Write("Enter Draggie Games account email: ");
        var email = Console.ReadLine();
        Console.Write("Enter Draggie Games account password: ");
        var pass = Console.ReadLine();

        using (HttpClient client = new HttpClient())
        {
            // https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/how-to?pivots=dotnet-8-0
            string scope = "csharp/draggiegames-compsciproject";
            
            var loginAccount = new initialUsernameJsonDict
            {
                email = email,
                password = pass,
                scope = scope,
            };

            string jsonString = System.Text.Json.JsonSerializer.Serialize(loginAccount);

            // https://stackoverflow.com/questions/9145667/how-to-post-json-to-a-server-using-c

            Console.Write($"Posting data to server... content = {jsonString}\n");
            Console.Write($"Posting data to server... email = {email}, password = {pass}\n");
            
            var response = await client.PostAsync(
                $"{serverBaseDirectoryUrl}/login", 
                new StringContent(jsonString, Encoding.UTF8, "application/json")
            );
        
            var responseString = await response.Content.ReadAsStringAsync();
            var responseStatusCode = response.StatusCode;
            int newIntResponseStatusCode = (int) responseStatusCode; // Type casting https://www.w3schools.com/cs/cs_type_casting.php
            // Console.WriteLine(newIntResponseStatusCode); // only for debug
            if (newIntResponseStatusCode != 200) {
                Console.WriteLine($"[Debug] An error has occurred: {responseString}");
                // https://stackoverflow.com/questions/17617594/how-to-get-some-values-from-a-json-string-in-c

                try {
                    dynamic parsed_json_response = JObject.Parse(responseString);
                    //Console.Write(parsed_json_response);
                    Console.WriteLine($"An error has occurred.\n\nHTTP status code:    [error {newIntResponseStatusCode}]\nDetailed message:    {parsed_json_response.message}"); // this grabs the key 'message' from json response
                    // server-side code in login function
                } catch (Exception e) {
                    Console.Write($"\nThe server had a difficulty handling the request.\n\nRaw error: {e}");
                }

                // now restart the main func
                await Main(new string[0]);
            }
            // If response status code IS 200 (OK)
            Console.WriteLine(responseString);
            dynamic jsonSuccessfullyParsedResponse = JObject.Parse(responseString);

            print($"{jsonSuccessfullyParsedResponse.message}");
            
            
        }
    }
}