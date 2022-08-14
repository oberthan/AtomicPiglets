using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardDeckScript : MonoBehaviour
{
    public GameObject CardPrefab;
 //   public GameObject Deck;

    public int MaxCards = 56;
    private double _dx = 0.1;
    private double _dy = 0.021;
    private double _dz = 0.1;
    private double _dr = 3.0;

    // Start is called before the first frame update
    void Start()
    {
        var deck = this.GameObject();
        var rnd = new System.Random();

        for (int i = 0; i < MaxCards; i++)
        {
            var card = Instantiate(CardPrefab);
            var x = (float) RandomSigned(rnd, _dx) ;
            var y = (float)(i * _dy);
            var z = (float) RandomSigned(rnd, _dz);
            float angle = (float) RandomSigned(rnd, _dr);
            card.transform.position = new Vector3(x, y, z);
            card.transform.Rotate(new Vector3(0, 1, 0), angle);
            card.transform.SetParent(deck.transform, false);
        } 
    }

    private double RandomSigned(System.Random rnd, double scale)
    {
        return scale*(rnd.NextDouble() * 2 - 1);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetCardCount(int cardCount)
    {
        var deck = this.GameObject();

        for (int i = 0; i < deck.transform.childCount; i++)
        {
            var child = deck.transform.GetChild(i);
            child.gameObject.SetActive(i < cardCount);
        }
    }

}