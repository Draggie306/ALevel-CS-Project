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
    public long clientId = 899342308372254840;
    public string state = "Playing";
    public string details = "techDemo";
    public string largeImage = "https://cdn.jsdelivr.net/gh/Draggie306/iBaguette@main/media/cats/CatswirlOptimised";
    public string largeText = "Unity";

    private Discord.Discord discord;
    private ActivityManager activityManager;

    void Start()
    {
        discord = new Discord.Discord(clientId, (UInt64)CreateFlags.Default);
        activityManager = discord.GetActivityManager();
        UpdateActivity();
    }

    public void UpdateActivity()
    {
        var activity = new Activity
        {
            State = state,
            Details = details,
            Timestamps = { Start = DateTimeOffset.Now.ToUnixTimeSeconds() },
            Assets = { LargeImage = largeImage, LargeText = largeText }
        };

        activityManager.UpdateActivity(activity, (res) => 
        {
            if (res == Result.Ok)
            {
                Debug.Log("Successfully updated activity.");
            }
            else
            {
                Debug.LogError("Failed to update activity.");
            }
        });
    }

    void Update()
    {
        discord.RunCallbacks();
    }
}
