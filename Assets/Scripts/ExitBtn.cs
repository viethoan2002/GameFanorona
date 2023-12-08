using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitBtn : BaseButton
{
    public override void OnClick()
    {
#if UNITY_EDITOR
        // Trong trường hợp chạy trên Unity Editor
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Trong trường hợp chạy trên build standalone hoặc di động
        Application.Quit();
#endif
    }
}
