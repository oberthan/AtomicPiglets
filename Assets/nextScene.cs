using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class nextScene : MonoBehaviour
{
   
 
    public GameObject escapeMenu;
    public GameObject Game;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            print("escape trykket");
            escapeMenu.SetActive(true);
        }
    }

}
