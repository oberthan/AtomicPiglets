using System;
using System.Linq;
using GameLogic;
using UnityEngine;

public class GameAudioScript : MonoBehaviour
{
    public AudioClip Shuffle1;
    public AudioClip Shuffle2;
    public AudioClip Shuffle3;
    public AudioClip Shuffle4;
    
    public AudioClip Deal1;
    public AudioClip Deal2;
    
    public AudioClip Draw1;
    public AudioClip Draw2;
    public AudioClip Draw3;
    public AudioClip DrawFromPlayer1;
    public AudioClip DrawFromPlayer2;

    public AudioClip PlayCard1;
    public AudioClip PlayCard2;
    public AudioClip PlayMulti1;
    public AudioClip PlayMulti2;
    
    public AudioClip Nope;

    public AudioClip Win;
    public AudioClip GameOver;
    public AudioClip Defuse;
    public AudioClip DrawAtomicPiglet;

    private AudioSource _audioSource;

    private System.Random _rnd = new();
    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GameObject.Find("Game").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shuffle()
    {
        PlayRandom(new[] { Shuffle1, Shuffle2, Shuffle3, Shuffle4 });
    } 
    
    public void PlayDeal()
    {
        PlayRandom(new[] { Deal1, Deal2 });
    }
    public void PlayDraw()
    {
        PlayRandom(new[] { Draw1, Draw2, Draw3 });
    }
    public void PlayDrawFromPlayer()
    {
        PlayRandom(new[] { DrawFromPlayer1, DrawFromPlayer2 });
    }
    public void PlayCard()
    {
        PlayRandom(new[] { PlayCard1, PlayCard2});
    }
    public void PlayMulti()
    {
        PlayRandom(new[] { PlayMulti1, PlayMulti2});
    }

    public void PlayWin()
    {
        PlayRandom(new[] {Win});
    }
    public void PlayGameOver()
    {
        PlayRandom(new[] {GameOver});
    }
    public void PlayDefuse()
    {
        PlayRandom(new[] {Defuse});
    }
    public void PlayDrawAtomicPiglet()
    {
        PlayRandom(new[] {DrawAtomicPiglet});
    }

    public void PlayNope()
    {
        PlayRandom(new[] { Nope });
    }

    private void PlayRandom(AudioClip[] clips)
    {
        _audioSource.clip = clips[_rnd.Next(clips.Length)];
        _audioSource.pitch = (float)(1.0 + 0.2 * (_rnd.NextDouble() - 0.5));
        _audioSource.Play();
    }

    public void PlayGameEvent(GameEvent gameEvent)
    {
        switch (gameEvent.Type)
        {
            case GameEventType.NewGameStarted:
                PlayDeal();
                break;
            case GameEventType.GameOver:
                PlayGameOver();
                break;
            case GameEventType.GameWon:
                PlayWin();
                break;
            case GameEventType.CardsPlayed:
                if (gameEvent.PlayCards.Length > 1)
                    PlayMulti();
                else
                {
                    if (gameEvent.PlayCards.First().Type == CardType.DefuseCard)
                        PlayDefuse();
                    else
                        PlayCard();
                }

                break;
            case GameEventType.ActionExecuted:
                switch (gameEvent.ActionType)
                {
                    case nameof(DrawFromPlayerAction):
                    case nameof(DemandCardFromPlayerAction):
                    case nameof(FavorAction):
                        PlayDrawFromPlayer();
                        break;
                    case nameof(DrawFromDiscardPileAction):
                    case nameof(DrawFromDeckAction):
                        PlayDraw();
                        break;
                    case nameof(NopeAction):
                        PlayNope();
                        break;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

    }
}
