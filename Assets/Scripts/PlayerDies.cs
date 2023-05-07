using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDies : MonoBehaviour
{
    public GameObject YouLostText;
    public Multiplayer multiplayer;
    private void OnTriggerExit(Collider collision) // if touches "Lava" = dies
    {
        if (collision.CompareTag("Lava"))
        {
            Destroy(gameObject);
            YouLostText.SetActive(true);
            multiplayer.die();
        }

    }
}
