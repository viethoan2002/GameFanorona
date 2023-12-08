using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayBtn :  BaseButton
{
    public override void OnClick()
    {
        GameMode.Instance.PlayGame();
    }
}
