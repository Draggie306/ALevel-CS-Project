using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Discord;

/*
    Grab that Client ID from earlier
    Discord.CreateFlags.Default will require Discord to be running for the game to work
    If Discord is not running, it will:
    1. Close your game
    2. Open Discord
    3. Attempt to re-open your game
    Step 3 will fail when running directly from the Unity editor
    Therefore, always keep Discord running during tests, or use Discord.CreateFlags.NoRequireDiscord
*/

public class DiscordRPC : MonoBehaviour
{
    private long clientId = 899342308372254840;

    [SerializeField]
    private string ThirdLine = "Playing"; // actually "state" in the SDK
    public string SecondLine = "Inside the main menu";  // really the "Details" in sdk
    public string largeImage = "https://assets.draggie.games/saturnian-content/Saturnian-1080.png";
    public string largeText = "Follow @SaturnianGame for more!";

    private Discord.Discord discord;
    private ActivityManager activityManager;

    void Start()
    {
        Debug.Log($"[DiscordRPC] Initialising Discord rich presence on object: \"{gameObject.name}\"");
        discord = new Discord.Discord(clientId, (UInt64)Discord.CreateFlags.Default);// we DO require discord or there will be loads of exceptions (NullReferenceException: Object reference not set to an instance of an object DiscordRPC.Update () (at Assets/SceneScripts/DiscordRPC.cs:81))
        activityManager = discord.GetActivityManager();
        UpdateActivity();
    }

    public void UpdateActivity()
    {
        var activity = new Activity
        {
            State = ThirdLine,
            Details = SecondLine,
            Timestamps = { Start = DateTimeOffset.Now.ToUnixTimeSeconds() },
            Assets = { LargeImage = largeImage, LargeText = largeText },
            // Secrets = { Join = "https://ibaguette.com", Spectate = "https://draggiegames.com" }, // TODO: Make these work
        };


        // Code copied from the example on discord game sdk docs
        activityManager.UpdateActivity(activity, (res) => 
        {
            if (res == Result.Ok)
            {
                Debug.Log("[DiscordRPC] Successfully updated activity.");
            }
            else
            {
                Debug.LogError($"[DiscordRPC] Failed to update activity: {res}");
            }
        });
    }


    // Project extra: Call from onclick event
    public void UpdateSecondLine(string newSecondLine)
    {
        SecondLine = newSecondLine;
        UpdateActivity();
    }

    public void UpdateThirdLine(string newThirdLine)
    {
        ThirdLine = newThirdLine;
        UpdateActivity();
    }

    void Update()
    {
        discord.RunCallbacks();
    }
}
