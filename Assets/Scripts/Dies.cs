using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dies : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision) // if touches "Lava" = dies
    {
        if (collision.CompareTag("Lava"))
        {
            Destroy(gameObject);
        }

    }
}
