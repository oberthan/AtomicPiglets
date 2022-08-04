using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

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

    [SerializeField]
    private TMP_Text _title;

    public void Hej()
    {
        var game = GameLogic.GameFactory.CreateExplodingKittensLikeGame(2);
        _title.text = game.CurrentPlayer.Hand.All.First().ToString();
    }
}
