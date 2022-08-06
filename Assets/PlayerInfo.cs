using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInfo : MonoBehaviour
{
    private string PlayerName;

    public TMP_Text WriteName;

    public GameObject mainMenu;

    public GameObject PlayerNameTextField;
    public TMP_Text PlayerNameText;
    public TMP_Text PlayerNameTextPreview;

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

        WriteName.text = PlayerName;
    }

    private void loadName()
    {
        PlayerName = PlayerPrefs.GetString("name");
        print(PlayerName);

        WriteName.text = PlayerName;
        PlayerNameTextPreview.text = PlayerName;
    }

    private string PlayernameReset;
    
    private void Update()
    {
        if (PlayerName == "")
        {
            PlayerName = PlayernameReset;
        }
       // PlayerNameTextField.GetComponent<InputField>().text = PlayerName;
    }
}
