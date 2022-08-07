using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastSubmenu : MonoBehaviour
{

    public GameObject MainMenu;
    public GameObject JoinMatch;
    public GameObject HostMatch;
    public GameObject Lobby;
    private string LastActiveMenu = "Main";

    // Update is called once per frame
    public void SetLastActive()
    {
        if (MainMenu.activeInHierarchy == true)
        {
            MainMenu.SetActive(false);
            LastActiveMenu = "Main";
        }

        if (JoinMatch.activeInHierarchy == true)
        {
            JoinMatch.SetActive(false);
            LastActiveMenu = "Join";
        }

        if (HostMatch.activeInHierarchy == true)
        {
            HostMatch.SetActive(false);
            LastActiveMenu = "Host";
        }

        if (Lobby.activeInHierarchy == true)
        {
            Lobby.SetActive(false);
            LastActiveMenu = "Lobby";
        }
    }

    public void BackToMenu()
    {
        if (LastActiveMenu == "Main")
        {
            MainMenu.SetActive(true);
        }

        if (LastActiveMenu == "Host")
        {
            HostMatch.SetActive(true);
        }

        if (LastActiveMenu == "Join")
        {
            JoinMatch.SetActive(true);
        }

        if (LastActiveMenu == "Lobby")
        {
            Lobby.SetActive(true);
        }
    }
}
