using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeCountDown : MonoBehaviour
{
    public bool endTime = false;
    public float timeLeft = 5f;
    public float currentTime;
    public Image timeBar;
    public TMP_Text timeText;

    public bool isPause;

    private void Start()
    {
        currentTime = timeLeft;
    }

    private void Update()
    {
        if(!isPause)
            currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            endTime = true;
        }
        else
        {
            FillBar();
        }

    }

    private void FillBar()
    {
        timeBar.fillAmount = currentTime / timeLeft;
        timeText.text = $"{Mathf.FloorToInt(currentTime)/ 01:00}";
    }

    public void ResetTime()
    {
        currentTime = timeLeft;
    }
}
