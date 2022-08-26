using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CardDeckScript : MonoBehaviour
{
    public GameObject CardPrefab;
    //   public GameObject Deck;

    private double _dx = 0.1;
    private double _dy = 0.021;
    private double _dz = 0.1;
    private double _dr = 3.0;
    private readonly System.Random _rnd = new();

    // Start is called before the first frame update
    void Start()
    {

        //InitializeNewDeck(56);
    }

    public IEnumerator InitializeNewDeck(int maxCards)
    {
        var deck = this.GameObject();

        foreach (Transform child in deck.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < maxCards; i++)
        {
            var card = Instantiate(CardPrefab);
            var x = (float)RandomSigned(_rnd, _dx);
            var y = (float)(i * _dy);
            var z = (float)RandomSigned(_rnd, _dz);
            float angle = (float)RandomSigned(_rnd, _dr);
            card.transform.position = new Vector3(x, y, z);
            card.transform.Rotate(new Vector3(0, 1, 0), angle);
            card.transform.SetParent(deck.transform, false);
            yield return new WaitForSeconds(0.02f);
        }
    }

    private double RandomSigned(System.Random rnd, double scale)
    {
        return scale * (rnd.NextDouble() * 2 - 1);
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

    private Transform GetTopActiveTransform()
    {
        var deck = this.GameObject();
        for (int i = deck.transform.childCount - 1; i >= 0; i--)
        {
            var child = deck.transform.GetChild(i);
            if (child.gameObject.activeInHierarchy)
                return child;
        }

        return null;
    }

    public void DrawCardToBottom(int cardIndex)
    {
        var animationScript = GetCardAnimator(cardIndex, out var animator);

        animator.enabled = true;
        animationScript.DrawCard();

    }
    public void DrawCardToTop(int cardIndex)
    {
        var animationScript = GetCardAnimator(cardIndex, out var animator);

        animator.enabled = true;
        animationScript.EnemyDrawCard();
    }

    private AnimationScript GetCardAnimator(int cardIndex, out Animator animator)
    {
//        SetCardCount(cardIndex + 1);
        var deck = this.GameObject();
        var card = deck.transform.GetChild(cardIndex);
        var cardGameObject = card.gameObject;
        var animationScript = cardGameObject.GetComponentInChildren<AnimationScript>();
        animator = cardGameObject.GetComponentInChildren<Animator>();
        return animationScript;
    }
}
