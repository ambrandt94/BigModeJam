
using UnityEngine;
using UnityEngine.Events;

public class FireEventsOnStart : MonoBehaviour
{
    public UnityEvent[] eventsToFire;

    private void Start()
    {
        foreach (var unityEvent in eventsToFire)
        {
            unityEvent?.Invoke();
        }
    }
}
