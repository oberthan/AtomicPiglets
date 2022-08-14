using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardDeckScript : MonoBehaviour
{
    public GameObject CardPrefab;
    // Start is called before the first frame update
    void Start()
    {
        var rnd = new System.Random();
        var cardDeck = GameObject.Find("Kort");
        var dx = 0.1;
        var dy = 0.021;
        var dz = 0.1;
        var dr = 5.0;
        for (int i = 0; i < 56; i++)
        {
            var card = Instantiate(CardPrefab);
            var x = (float)((rnd.NextDouble()-0.5)*dx);
            var y = (float)(i * dy);
            var z = (float)((rnd.NextDouble() - 0.5) * dz);
            float angle = (float)((rnd.NextDouble() - 0.5) * dr);
            card.transform.position = new Vector3(x, y, z);
            card.transform.Rotate(new Vector3(0, 1, 0), angle);
            card.transform.SetParent(cardDeck.transform, false);
        } 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
