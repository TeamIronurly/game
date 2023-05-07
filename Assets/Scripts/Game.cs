using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public GameObject YouLostText;
    public GameObject YouWonText;

    public bool gameFinished = false;

    public void onLoose()
    {
        if (gameFinished) return;
        gameFinished = true;
        YouLostText.SetActive(true);
    }
    public void onWin()
    {
        if (gameFinished) return;
        gameFinished = true;
        YouWonText.SetActive(true);
    }
}
