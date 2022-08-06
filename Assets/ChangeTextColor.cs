using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangeTextColor : MonoBehaviour
{

    public TMP_Text TextToChange;



    public void Start()
    {
        TextToChange.color = Color.red;
    }

    public void pressed(bool isPressed)
    {
        if (isPressed == true)
        {
            TextToChange.color = Color.green;
            TextToChange.text = "Ready";
        }
        else
        {
            TextToChange.color = Color.red;
            TextToChange.text = "Unready";
        }
    }

}    
