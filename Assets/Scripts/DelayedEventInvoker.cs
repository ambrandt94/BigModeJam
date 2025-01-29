using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class DelayedEventInvoker : MonoBehaviour
{
    [System.Serializable]
    public class DelayedEvent
    {
        public string eventName = "Event"; // Optional name for clarity in the inspector
        public UnityEvent unityEvent;     // The UnityEvent to invoke
        public float delay = 0f;          // Delay before invoking the event
    }

    [Header("Events Configuration")]
    [Tooltip("List of events with delays to invoke sequentially")]
    public DelayedEvent[] events;

    /// <summary>
    /// Starts invoking events when the object is enabled.
    /// </summary>
    private void OnEnable()
    {
        StartCoroutine(InvokeEvents());
    }

    /// <summary>
    /// Coroutine to invoke events with specified delays.
    /// </summary>
    private IEnumerator InvokeEvents()
    {
        foreach (var delayedEvent in events)
        {
            if (delayedEvent != null)
            {
                yield return new WaitForSeconds(delayedEvent.delay);
                delayedEvent.unityEvent?.Invoke();
            }
        }
    }
}
