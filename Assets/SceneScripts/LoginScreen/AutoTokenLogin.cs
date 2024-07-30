using UnityEngine;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System;
using System.Net; 
using System.Net.Http;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Reads the token file, encrypts it and POSTs it to the login endpoint
/// TODO: Currently is broken.
/// </summary>

// Used: https://support.unity.com/hc/en-us/articles/115000341143-How-do-I-read-and-write-data-from-a-text-file-


public class RuntimeText: MonoBehaviour
{
    private static string DraggieAppDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Draggie");
    private static string SaturnianAppDir = Path.Combine(DraggieAppDataDirectory, "Saturnian");

    private static byte[] fernetKey;

    static void DraggieApp()
    {
        string username = Environment.UserName.ToLower() + ".3d060a9b-f248-4e2b-babd-e6d5d2c2ab8b";
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(username));
            fernetKey = hashKey;
        }
    }

    private static string DecryptToken(string encryptedToken)
    {
        Debug.Log("[DecryptToken] Decrypting token");
        byte[] encryptedTokenBytes = Convert.FromBase64String(encryptedToken);
        using (Aes aes = Aes.Create())
        {
            aes.Key = fernetKey;
            using (MemoryStream ms = new MemoryStream(encryptedTokenBytes))
            {
                byte[] iv = new byte[aes.IV.Length];
                ms.Read(iv, 0, iv.Length);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }

    public static string ReadTokenFileContents()
    {
        Debug.Log("[ReadTokenFileContents] Reading token file contents...");
        string tokenFilePath = Path.Combine(SaturnianAppDir, "token.bin");

        if (File.Exists(tokenFilePath))
        {
            string cachedToken = File.ReadAllText(tokenFilePath);
            Debug.Log($"[ReadTokenFileContents] Found cached token: {cachedToken}");
            return cachedToken;
        }
        return null;
    }
}