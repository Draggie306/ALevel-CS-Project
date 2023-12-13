// Used: https://support.unity.com/hc/en-us/articles/115000341143-How-do-I-read-and-write-data-from-a-text-file-

using UnityEngine;
using System.IO;
using System.Collections;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Security.Cryptography;
using System.Text.Json;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Net; 
using System.Net.Http;
using TMPro;
using UnityEngine.SceneManagement;

public class RuntimeText: MonoBehaviour
{
    static void WriteString(string initPath, string content)
    {
        string path = Application.persistentDataPath + initPath;
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(content);
        writer.Close();
        StreamReader reader = new StreamReader(path);
        //Print the text from the file
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }

    static void ReadString(string initPath)
    {
        string path = Application.persistentDataPath + initPath;
        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }


    static string DecodeEncryptedAuthToken(string initToken)
    // Decodes encrypted auth token
    // Taken from https://stackoverflow.com/questions/17128038/c-sharp-rsa-encryption-decryption-with-transmission
    {
        string initDecryptionKey = "MIIEoQIBAAKCAQBKDqhnbAbXU+ZvYQ+HY/Sk5roKCImfXKG0b1jtcgVNpJlfKc9pKIQ0eOJoSrgYbA6CvvG38NxM/WIcecHp2K4aD9OOJZC+c2FXQGN/eJKA67/w1E8QdpSK7u2hHiHA/bLUvU3QxCIx9EmghGnO94/cubtSYROjTZ1ZNlo3RvZ0UFZYEiixz3kx89DqbtOETxWfzWVZ5naBOg5Vhp7zlnVFRLbOqAs8ZYnHdFIkgNp4ArzyLmshgVyDzXvJaTV9gFi1KawLvpEbQEELNeM9ZLAGqA2wpxDYdjKQTsfgjnqp+DjiY3+kxiDWUK57ZCfNV6JEy9wKVQV4SJ2iWiRH6L6VAgMBAAECggEAA6eJg+D+zW1kd6aQf5vdHK4ODCSztdt6V08PUlhIDrbKormLdKL9MyGr+n2FbB8Y8Da/8tW96UavqrwPZ5y7qqmRRPhxlhRXf1i8EDRA9n+rrxSq+iL/3YNA0qyL+dwLERhuWJj1HwXxBCzEk+P/g2Le7YfA0lQoKZjXBtdHniGzJ7ZnRtpXXL8J8QqtV/OrTyPI5VD3ZpIQ1oh8/QAntm7K60ejNjOFqvEaJ4qHxjAqz29Y/0ItWgdeb+D1NKdCUQlV9vWav7knw6k0DBdXwFqxroR3T9xvFTRZF61+jzwRp4fv6SPKSsoHzAKMCcNmgmXuQoeXGqc9JXCJHePvAQKBgQCPCFMSvXBaX/BvJjnuv7R/YJv+EKCkz5qYq5KEOCUD6sJlVDJJIjkM7LlzQZoOkhYYl94B4ou/qJTkHYKP+JuqUQTQbIKs3dP6yEcx/1MY81HiVt1ot+j8JnGsWZC7VJSEnzOO+zXQKmhX8AzSEEC6CmNfuip4vqqNno+dP3lrYQKBgQCEjEJAwl+UDD4QOfI2F/ULfxEakr4+BRlAvQFIXkL/ywh2/nP6UV5eQyioDrOOQW/O+McdhaJykbRcNBosHr3RChawODP/rqC33nt1xVIJuL5W81goBaheVpgOs5cjfbX1hWV+56XkLYtb7IVmz/lHtbPzfXT89AJzs4yUxduztQKBgGNf2D/T6GSR9X1z5JoxDRnWqGqraME5D+L0iXZdf7Ip/+fgyJMxOMv3CJ3APWXUL4/kq7VJjeiaGuAmdJjMlHKPmbWpPl0WPfol/fkJuNVD8Gc+DNR9uly95Qpmq/zLNeiyp2K2vFulqxs1x+KGskHPh91xfaKobdy5BnStLhHhAoGAO0bytj13OyGCfAU+IhzytNJ91rF7rQbgtUgRk8i8E+M8BONlALR1DJjiTM5OVLdFBn3Rpo1GChDJZVbTwZl58Eufu+1Wq+jv7WrtMc0uFDTvwrkPWs607oOxexmr5O5qesPOkUONuA76PQD44/Lsm7pSIPmhYSLLGe1ZWItn5uUCgYAr7ynoHL3Hz6bEUVLxJ6pEHF5NiBPs+Gy0Z0rZN0e0xyyt810pljy1Vo+j35qXstiqFgWAqvNzcsCHd3BmcXtucFkUjgl8IW87ppRGpLW80BStUalAl2xUF+6Bu/lTkLR7FIdG/iAOEjq3bKUx24AmDIuUQrNk9388x4MoCL1+iQ==";
        byte[] token = Convert.FromBase64String(initToken);

        using (var rsa = new RSACryptoServiceProvider(2048))
        {
            try
            {
                rsa.FromXmlString(initDecryptionKey);
                var decryptedToken = rsa.Decrypt(token, RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(decryptedToken);
            }
            catch (Exception e)
            {
                Debug.Log($"[DecodeEncryptedAuthToken] Exception: {e}");
                return null;
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }
        }
    }

    bool initialCheckForFile()
    // Checks if auth file exists, if not, creates it
    {
        string path = Application.persistentDataPath + "/credentials.bin";
        if (!File.Exists(path))
        {
            Debug.Log("File does not exist, creating file");
            File.Create(path);
            return false;
        } else {
            Debug.Log("File exists");
            return true;
        }
    }

   void Start()
   {
        Debug.Log("Auto Token Login script loaded");
        var checker = initialCheckForFile();

        if (checker == false) {
            Debug.Log("No file found, skipping auto login");
            return;
        }

        string path = Application.persistentDataPath + "/credentials.bin";
        StreamReader reader = new StreamReader(path);
        string encryptedToken = reader.ReadToEnd();
        reader.Close();

        string decryptedToken = DecodeEncryptedAuthToken(encryptedToken);
   }
}