using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class nextScene : MonoBehaviour
{
   
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void nextSceneFunction()
    {
        SceneManager.LoadScene("Game");
       
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");

    }

    public GameObject escapeMenu;
    public GameObject Game;
    
    void Update()
    {
        if ((Game.activeInHierarchy == true) && (Input.GetKeyDown(KeyCode.Escape)))
        {
            print("escape trykket");
            escapeMenu.SetActive(true);
        }
    }

}
