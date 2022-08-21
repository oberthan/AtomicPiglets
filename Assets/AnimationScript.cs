using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    private Animator _animator;

    // Start is called before the first frame update
    void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    private void DrawCard()
    {
        _animator.SetTrigger("DrawCard");
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            _animator.SetTrigger("DrawCard");
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            _animator.SetTrigger("EnemyDrawCard");
        }
    }
}
