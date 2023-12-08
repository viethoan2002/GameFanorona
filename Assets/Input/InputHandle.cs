using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class InputHandle : MonoBehaviour
{
    public static InputHandle Instance;
    public PlayerControls playerControls;

    public bool isSellect;

    private void Awake()
    {
        if(InputHandle.Instance == null)
        {
            InputHandle.Instance = this;
        }
        playerControls = new PlayerControls();
    }

    private void SelectInput()
    {
        playerControls.PlayerController.Sellect.started += inputAction => isSellect = true;
        playerControls.PlayerController.Sellect.canceled += inputAction => isSellect = false;
    }
}
