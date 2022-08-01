using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HostScript : MonoBehaviour{
    private const int Z = 0;
    public Slider Players;
    public TextMeshProUGUI Value;

    public GameObject toggle;
    public GameObject password;
    private float moving = 0;
    private float destanation;
    public float t;

    private Vector3 startPosition;
    private Vector3 endPosition;

    private float startTime;
    private float endTime;
    public float animationTime = 1.0f;

    void Start()
    {
        
        
    }
    

    public void makePrivate(bool isPrivate)
    {
        if (isPrivate == true)
        {
            moving = -410;
            destanation = -400;
            
        }
        else
        {
            moving = 10;
            destanation = 0;
            
        }
        password.SetActive(isPrivate);

        Debug.Log(moving);
        Debug.Log(toggle.transform.localPosition);

        startPosition = toggle.transform.localPosition;
        endPosition = new Vector3(destanation, startPosition.y, startPosition.z);

        startTime = Time.time;
        endTime = startTime + animationTime;
    }

    public void Update()
    {
        var t = Time.time;

        if (startTime == 0 || t > endTime || animationTime <= 0) return;

        var dt = Time.time - startTime;
        var dAnim = dt / (animationTime);

        var x = Mathf.SmoothStep(startPosition.x, endPosition.x, dAnim);
        toggle.transform.localPosition = new Vector3(x, startPosition.y, startPosition.z);
    }

    public void PlayerCount()
    {
        Value.text = Players.value.ToString("0" + " Players");
    }
    
    public void playeradd()
    {
        Players.value = Players.value + 1;
        PlayerCount();
    }

    public void playerminus()
    {
        Players.value = Players.value - 1;
        PlayerCount();
    }
}
