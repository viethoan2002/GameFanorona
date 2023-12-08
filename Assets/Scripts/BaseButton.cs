using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseButton : MonoBehaviour
{
    public Button button;

    private void Start()
    {
        this.button = GetComponent<Button>();
        this.AddOnClickEvent();
    }

    private void AddOnClickEvent()
    {
        this.button.onClick.AddListener(this.OnClick);
    }

    public abstract void OnClick();
}
