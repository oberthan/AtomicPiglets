using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using GameLogic;

public class StartGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

//    [SerializeField]
    public TMP_Text AvailableActionsText;

    public TMP_Text CurrentPlayerName;

    public TMP_Text CurrentPlayerCards;

    public void Hej()
    {
        var game = GameFactory.CreateExplodingKittensLikeGame(2);

        CurrentPlayerName.text = game.CurrentPlayer.Name;
        CurrentPlayerCards.text = string.Join("\n", game.CurrentPlayer.Hand.All);

        var gameLogic = new AtomicPigletRules(game);
        var actions = gameLogic.GetLegalActionsForPlayer(game.CurrentPlayer).ToList();

        AvailableActionsText.text = string.Join("\n", actions.Select(x => x.GetType().Name));
    }
}
