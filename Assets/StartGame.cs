using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Assets.Dto;
using GameLogic;
using FishNet.Object;
using FishNet.Connection;
using Assets.Network;

public class StartGame : NetworkBehaviour
{




    //    [SerializeField]
    public TMP_Text AvailableActionsText;
    

    public TMP_Text CurrentPlayerName;

    public TMP_Text CurrentPlayerCards;


    //public GameObject ButtonPrefab;
   // public GameObject ButtonCanvas;

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
//        Button btn = yourButton.GetComponent<Button>();
//        btn.onClick.AddListener(Pressed);
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


        startGame();
        print("trykket på hej");



    }




    // starts the game and sends data to the observer
    [ServerRpc]
    public void startGame()
    {
        Debug.Log("sendt til server");


        var game = GameFactory.CreateExplodingKittensLikeGame(2);

        var gameLogic = new AtomicPigletRules(game);
        var actions = gameLogic.GetLegalActionsForPlayer(game.CurrentPlayer).ToList();

        var Playername = game.CurrentPlayer.Name;
        var PlayerCards = string.Join("\n", game.CurrentPlayer.Hand.All);
        var AvaibleActions = string.Join("\n", actions.Select(x => x.GetType().Name));

        var playerInfo = new PlayerInfo { PlayerName = "Soren" };

        //var lobbyServer = 

        var netObject = new MitNetObject { PlayerName = "Søren", TurnsLeft = 5 };
        //sendNetObjectTo(netObject, gameLogicObject);
        sendNetObjectTo(netObject);

        //        sendTo(Playername, PlayerCards, AvaibleActions, gameLogicObject);
    }


    public class MitNetObject
    {
        public string PlayerName;
        public int TurnsLeft;
    }


    //[ObserversRpc]
    //public void sendNetObjectTo(MitNetObject mitNetObject, GameObject gamelogicObject)
    //{
    //    var startGame = gamelogicObject.GetComponent<StartGame>();
    //    startGame.CurrentPlayerName.text = mitNetObject.PlayerName;
    //    startGame.CurrentPlayerCards.text = mitNetObject.TurnsLeft.ToString();

    //}
    [ObserversRpc]
    public void sendNetObjectTo(MitNetObject mitNetObject)
    {
        var startGame = gameObject.GetComponent<StartGame>();
        startGame.CurrentPlayerName.text = mitNetObject.PlayerName;
        startGame.CurrentPlayerCards.text = mitNetObject.TurnsLeft.ToString();

    }


    // finds the textfield and prints the text recieved from the server
    [ObserversRpc]
    public void sendTo(string PlayerName, string playercards, string AvaibleActions, GameObject gamelogicObject)
    {
        Debug.Log("sendt tilbage");

        //print("prøver at skrive "+ PlayerName+", "+playercards+", "+AvaibleActions);
        //CurrentPlayerName.text = PlayerName;
        //   CurrentPlayerCards.text = playercards;

        gamelogicObject.GetComponent<StartGame>().CurrentPlayerName.text = PlayerName;
        gamelogicObject.GetComponent<StartGame>().CurrentPlayerCards.text = playercards;
        gamelogicObject.GetComponent<StartGame>().AvailableActionsText.text = AvaibleActions;

        //foreach (char currentAction in AvaibleActions)
        //{
        //    new 
       // }
    }
}
