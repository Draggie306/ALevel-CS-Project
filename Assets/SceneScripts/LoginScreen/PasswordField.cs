using UnityEngine;
using TMPro;

/// <summary>
/// This script sets the password field to be a password field
/// Took a while to figure out to get this to work correctly
/// </summary>

// TODO: Probably can just set this in the editor
public class PasswordFields : MonoBehaviour {
    public TMP_InputField password;
    void Start () {
        Debug.Log($"[PasswordField] Set password field to password type on GameObject {gameObject.name}");
        password.contentType = TMP_InputField.ContentType.Password;
    }
}



/*
// old code taken from https://forum.unity.com/threads/change-inputfield-input-from-standard-to-password-text-via-script.291897/

 using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowPassword : MonoBehaviour
{
    [SerializeField] private TMP_InputField userPassword;
 
    public void ShowUserPassword()
    {
        if (userPassword.contentType == TMP_InputField.ContentType.Password)
        {
            userPassword.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            userPassword.contentType = TMP_InputField.ContentType.Password;
        }
        userPassword.ForceLabelUpdate();
    }
}
*/