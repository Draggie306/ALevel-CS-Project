using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
var discord = new Discord.Discord(1140698648179658824, Discord.CreateFlags.NoRequireDiscord);


public class DiscordRPC : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
