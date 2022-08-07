using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullscreenScript : MonoBehaviour
{
    

    // Update is called once per frame
    public void ToggleFullscreen(bool isFullscreen)
    {
        
        
        
         Screen.fullScreen = isFullscreen;
        
    }
}
