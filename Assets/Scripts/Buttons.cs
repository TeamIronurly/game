using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    public void SinglePlayer()
    {
        SceneManager.LoadScene("SinglePlayer");
    }
    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
