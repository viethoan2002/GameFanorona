using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplayBtn : BaseButton
{
    public GameObject pauseUI;

    public override void OnClick()
    {
        GameManager.Instance.CreateMap();
        pauseUI.SetActive(false);
    }
}
