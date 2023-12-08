using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeBtn : BaseButton
{
    public Button button2;
    public bool onePlayer;

    public override void OnClick()
    {
        button2.image.color = new Color(0.53f, 0.53f, 0.53f);
        button.image.color = new Color(1, 1, 1);
        if(onePlayer)
            GameMode.Instance.SetGameMode(0);
        else
            GameMode.Instance.SetGameMode(1);
    }
}
