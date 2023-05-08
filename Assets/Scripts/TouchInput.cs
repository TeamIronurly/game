using UnityEngine;
using UnityEngine.EventSystems;

public class TouchInput : MonoBehaviour
{
    public bool pressed = false;
    void Start()
    {
        if (!Application.isMobilePlatform){
            gameObject.SetActive(false);
            return;
        }

        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();

        var pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((e) => { pressed = true; });
        trigger.triggers.Add(pointerDown);

        var pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((e) => { pressed = false; });
        trigger.triggers.Add(pointerUp);
    }
}
