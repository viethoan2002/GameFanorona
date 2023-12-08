using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioBtn : BaseButton
{
    public bool isOn = true;
    public AudioSource audioSource;
    public override void OnClick()
    {
        if (isOn)
        {
            isOn = false;
            audioSource.volume = 0;
            button.image.color = Color.gray;
        }
        else
        {
            isOn = true;
            audioSource.volume = 1;
            button.image.color = Color.white;
        }
    }
}
