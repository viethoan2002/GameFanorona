using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMode : MonoBehaviour
{
    public static GameMode Instance;

    private void Awake()
    {
        GameMode.Instance = this;
    }

    private void Start()
    {
        PlayerPrefs.SetInt("GameMode", 0);
        PlayerPrefs.SetInt("ColorMode", 0);
    }

    public void SetGameMode(int value)
    {
        PlayerPrefs.SetInt("GameMode", value);
    }  
    
    public void SetColorMode(int value)
    {
        PlayerPrefs.SetInt("ColorMode", value);
    }  
    
    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }    
}
