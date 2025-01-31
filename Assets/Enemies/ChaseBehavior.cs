using TMPro;
using UnityEngine;

public class ChaseBehavior : MonoBehaviour
{
    public float chaseSpeed = 5f;
    private Transform player;
    public float rotationSpeed = 5f; // Adjust rotation speed for smooth turning
      
    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    public void Chase()
    {
        if (player == null) return;
        transform.position = Vector3.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);


        // Rotate smoothly towards the target position
        Vector3 direction = (player.position - transform.position).normalized;
        if (direction != Vector3.zero) // Prevent errors when reaching target
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }

       
    }
}
