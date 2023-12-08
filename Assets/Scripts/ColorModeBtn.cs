using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorModeBtn : BaseButton
{
    public Button button2;
    public bool isWhite;

    public override void OnClick()
    {
        button2.image.color = new Color(0.53f, 0.53f, 0.53f);
        button.image.color = new Color(1, 1, 1);
        if (isWhite)
            GameMode.Instance.SetColorMode(0);
        else
            GameMode.Instance.SetColorMode(1);
    }
}
