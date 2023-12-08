using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseBtn : BaseButton
{
    public TimeCountDown timeCountDown;

    public GameObject ui;

    public override void OnClick()
    {
        timeCountDown.isPause = true;
        ui.SetActive(true);
    }
}
