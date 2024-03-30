using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Switches between the submenus in the main menu
/// When adding a new submenu, add it to the list of GameObjects and add a new function to switch to it
/// </summary>

public class SwitchSubmenus : MonoBehaviour
{
    // note: Don't drag this script to any old GameObject, it is placed on the root canvas  
    public GameObject MainMenuMain;
    public GameObject GraphicsSubMenu;
    public GameObject LockerSubMenu;
    public GameObject SelectGameModeMenu;
    public GameObject AccountMenu;
    public GameObject AdvancedGraphicsMenu;
    private AudioSource[] allAudioSources;

    public void StopAllAudio() {
        allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach( AudioSource audioS in allAudioSources) {
            audioS.Stop();
            Debug.Log($"Stopped audio on {audioS.gameObject.name}");
        }
    }

    public int CurrentMenu = 0; // 0 = MainMenuMain, 1 = GraphicsSubMenu, 2 = LockerSubMenu, 3 = SelectGameMode, 4 = AccountMenu, 5 = AdvancedGraphicsMenu

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Initialised SwitchSubmenus");
        CurrentMenu = 0;
        // Do not show the submenus on start, other than MainMenuMain
        MainMenuMain.SetActive(true);
        GraphicsSubMenu.SetActive(false);
        LockerSubMenu.SetActive(false);
        SelectGameModeMenu.SetActive(false);
        AccountMenu.SetActive(false);
        AdvancedGraphicsMenu.SetActive(false);
    }

    public void SwitchToGraphicsSubMenu()
    {
        CurrentMenu = 1;
        MainMenuMain.SetActive(false);
        GraphicsSubMenu.SetActive(true);
        LockerSubMenu.SetActive(false);
        SelectGameModeMenu.SetActive(false);
        AccountMenu.SetActive(false);
        AdvancedGraphicsMenu.SetActive(false);
    }

    public void SwitchToLockerSubMenu()
    {
        CurrentMenu = 2;
        MainMenuMain.SetActive(false);
        LockerSubMenu.SetActive(true);
        GraphicsSubMenu.SetActive(false);
        SelectGameModeMenu.SetActive(false);
        AccountMenu.SetActive(false);
        //StopAllAudio();
    }

    public void SwitchToGameModeSelect()
    {
        CurrentMenu = 3;
        MainMenuMain.SetActive(false);
        LockerSubMenu.SetActive(false);
        GraphicsSubMenu.SetActive(false);
        SelectGameModeMenu.SetActive(true);
        AccountMenu.SetActive(false);
    }

    public void SwitchToAccountMenu()
    {
        CurrentMenu = 4;
        MainMenuMain.SetActive(false);
        LockerSubMenu.SetActive(false);
        GraphicsSubMenu.SetActive(false);
        SelectGameModeMenu.SetActive(false);
        AccountMenu.SetActive(true);
    }

    public void SwitchToMainMenuMain()
    {
        CurrentMenu = 0;
        GraphicsSubMenu.SetActive(false);
        LockerSubMenu.SetActive(false);
        MainMenuMain.SetActive(true);
        SelectGameModeMenu.SetActive(false);
        AccountMenu.SetActive(false);
    }

    public void SwitchToAdvancedGraphicsMenu()
    {
        Debug.Log("Switching to advanced graphics menu");
        // Switch to the advanced graphics menu
        CurrentMenu = 5;
        GraphicsSubMenu.SetActive(false);
        LockerSubMenu.SetActive(false);
        MainMenuMain.SetActive(false);
        SelectGameModeMenu.SetActive(false);
        AccountMenu.SetActive(false);
        AdvancedGraphicsMenu.SetActive(true);
    }
}
