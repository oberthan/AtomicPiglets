using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerNameInfo : MonoBehaviour
{

    
    public string PlayerName;      

    

    public TMP_Text PlayerNameText;
    public TMP_Text PlayerNameTextPreview;

    public Button StartButton;

    //public Button saveName;
    //public GameObject showButton;

    void Start()
    {
        
        if (PlayerPrefs.HasKey("name"))
        {        
            loadName();
            
        }
        else
        {
            
            //showButton.SetActive(true);
        }
        //saveName.GetComponent<Button>().onClick.AddListener(SaveName);
    }

    // Update is called once per frame

    
    

    public void SaveName()
    {
        PlayerName = PlayerNameText.text;
        

        PlayerPrefs.SetString("name", PlayerName);
        print(PlayerName);
        
        
        //showButton.SetActive(false);

        
    }

    private void loadName()
    {
        PlayerName = PlayerPrefs.GetString("name");
        print(PlayerName);



        PlayerNameTextPreview.text = PlayerName;
    }

    
    
    private void Update()
    {
        if (PlayerName == "")
        {
            PlayerName = null;
        }
       // PlayerNameTextField.GetComponent<InputField>().text = PlayerName;

      
        if (PlayerName == null)
        {
            StartButton.interactable = false;
        }
        else
        {
            StartButton.interactable = true;
        }
    }
}
