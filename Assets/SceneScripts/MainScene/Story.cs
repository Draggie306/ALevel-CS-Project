using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays the dialogue in text form on the screen, as well as how to play
/// </summary>



public class Story : MonoBehaviour
{
    // todo: use scriptable objects for this and "externalise" the text and VO
    public string HowToPlay = "You can use your typical WASD keys to move around, and the mouse to look around. Use the spacebar to jump, the left mouse button to grapple onto objects, and \"E\" to interact with objects. Good luck out there.";
    public string StarterDialogue1 = "Welcome to the simulation! Or, as we like to call it, the \"game!\"";
    public string Dialogue2_AfterStarter = "Oh, did I mention that water and fluids will be constantly rising? You'll need to keep moving around to push the water back down.";
    public string Dialogue3_HowToWinMsg = "You can collect powerful tokens scattered seldomly around the map to fight back against the water.";
    public string Dialogue4_TokenExplanation = "A word of warning, every time you collect a token, the physics of this simulation - I mean, game - will change. Be careful!";
    public string Dialogue5_WinCondition = "The simulation will end and you will be extracted back to reality in ten minutes.";
    public string Dialogue6_FirstTokenPickup = "Keep it up! You're doing great!";
    public string Dialogue7_HalfTokenPickup = "You're halfway there!";
    public string Dialogue8_PenultimateTokenPickup = "Just one more token to go!";
    public string Dialogue9_AllTokensPickup = "All tokens have been collected! We're sending the extraction team now, give us a minute...";
    public string Dialogue10_ExtractionCountdown = "Ten... Nine... Eight... Seven... Six... Five... Four... Three... Two... One...!";
    public string Dialogue11_WinningMsg = "Congratulations! You've made it out of the simulation!";
    public string Dialogue12_Thanks = "Thanks for playing!";
    public string EndDialogue = "The simulation has ended. Want to play again?";

    public string[] PickupTokenDialogue = new string[] {
        "You've picked up a token! The simulation will change...",
        "Another token! The simulation will change...",
        "Wow, you're on a roll!",
        "Oooh, this is getting interesting...",
        "Oh yeah! Keep it up!",
        "You're doing great!",
        "Unstoppable!",
        "Boom! Another token!",
        "That doesn't look good...",
    };

    public string[] WaterRisingDamageDialogue = new string[] {
        "The water is rising!",
        "You're getting wet!",
        "You're taking damage!",
        "You're taking critical damage!",
        "Get to higher ground!",
        "Get out of the water!",
        "Get to higher ground ASAP!",
        "You're drowning!",
        "You don't want to be underwater for too long!",
        "You don't have gills, you know!",
        "You're not a fish!",
    };

    /*
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    */
    
}
