using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using GameLogic;
using FishNet.Object;
using FishNet.Connection;

public class StartGame : NetworkBehaviour
{


 

    //    [SerializeField]
    public TMP_Text AvailableActionsText;

    public TMP_Text CurrentPlayerName;

    public TMP_Text CurrentPlayerCards;



    // Disables the scribts you're not the owner of
    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            print("IsOwner");
        }
        else
        {
            GetComponent<StartGame>().enabled = false;
            print("IsNotOwner");
        }
    }

    
    // Detects if the button is pressed
    public Button yourButton;
    void Start()
    {
        Button btn = yourButton.GetComponent<Button>();
        btn.onClick.AddListener(Pressed);
    }

    
    
    // Asks the server to start the game
    public void Pressed()
    {
        //   var game = GameFactory.CreateExplodingKittensLikeGame(2);

        //   CurrentPlayerName.text = game.CurrentPlayer.Name;
        //   CurrentPlayerCards.text = string.Join("\n", game.CurrentPlayer.Hand.All);

        //   var gameLogic = new AtomicPigletRules(game);
        //   var actions = gameLogic.GetLegalActionsForPlayer(game.CurrentPlayer).ToList();

        //   AvailableActionsText.text = string.Join("\n", actions.Select(x => x.GetType().Name));
        
        
            startGame(gameObject);
        print("trykket på hej");
        
        

    }

    
    
    
    // starts the game and sends data to the observer
    [ServerRpc]
    public void startGame(GameObject gameLogicObject)
    {
        Debug.Log("sendt til server");


        var game = GameFactory.CreateExplodingKittensLikeGame(2);

        var gameLogic = new AtomicPigletRules(game);
        var actions = gameLogic.GetLegalActionsForPlayer(game.CurrentPlayer).ToList();

        sendTo(game.CurrentPlayer.Name, string.Join("\n", 
            game.CurrentPlayer.Hand.All), 
            string.Join("\n", actions.Select(x => x.GetType().Name)), gameLogicObject);
    }

    
    
    // finds the textfield and prints the text recieved from the server
    [ObserversRpc]
    public void sendTo(string PlayerName, string playercards, string AvaibleActions, GameObject gamelogicObject)
    {
        Debug.Log("sendt tilbage");

        print("prøver at skrive "+ PlayerName+", "+playercards+", "+AvaibleActions);
        CurrentPlayerName.text = PlayerName;
        //   CurrentPlayerCards.text = playercards;

        gamelogicObject.GetComponent<StartGame>().CurrentPlayerName.text = PlayerName;
        gamelogicObject.GetComponent<StartGame>().CurrentPlayerCards.text = playercards;
        gamelogicObject.GetComponent<StartGame>().AvailableActionsText.text = AvaibleActions;

     //   AvailableActionsText.text = AvaibleActions;
    }
}
