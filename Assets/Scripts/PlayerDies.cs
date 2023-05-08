using UnityEngine;
using UnityEngine.Events;

public class PlayerDies : MonoBehaviour
{
    public UnityEvent onLoose;
    private void OnTriggerExit(Collider collision) // if touches "Lava" = dies
    {
        if (collision.CompareTag("Lava"))
        {
            Destroy(gameObject);
            onLoose.Invoke();
        }

    }
}
