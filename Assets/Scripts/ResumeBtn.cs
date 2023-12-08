using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResumeBtn : BaseButton
{
    public TimeCountDown timeCountDown;

    public GameObject ui;

    public override void OnClick()
    {
        timeCountDown.isPause = false;
        ui.SetActive(false);
    }
}
