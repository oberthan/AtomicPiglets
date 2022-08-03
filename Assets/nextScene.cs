using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class nextScene : MonoBehaviour
{
    private bool gameScene = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void nextSceneFunction()
    {
        SceneManager.LoadScene("Game");
        gameScene = true;
        return;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
        gameScene = false;
        return;
    }

    public GameObject escapeMenu;
    
    void Update()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("Game") && (Input.GetKeyDown(KeyCode.Escape)))
        {
            print("escape trykket");
            escapeMenu.SetActive(true);
        }
    }

}
//((gameScene == true) && 