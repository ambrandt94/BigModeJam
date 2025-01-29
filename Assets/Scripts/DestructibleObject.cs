using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    [SerializeField] private int objectHealth = 1;

    [SerializeField] private GameObject[] objectsToDeactivateOnDestroy;
    [SerializeField] private GameObject[] objectsToActivateOnDestroy;


    public void DestroyObject()
    {
        foreach (var obj in objectsToDeactivateOnDestroy)
        {
            obj.SetActive(false);
        }

        foreach (var obj in objectsToActivateOnDestroy)
        {
            obj.SetActive(true);

        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
