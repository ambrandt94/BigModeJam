using UnityEngine;
using System;

public class StunComponent : MonoBehaviour
{
    public bool IsStunned { get; private set; }
    public float StunDuration { get; private set; }
    public float StunTimer { get; private set; }

    public event Action OnStunStart;
    public event Action OnStunEnd;

    private MoveBehaviour moveBehaviour;
    private FlyBehaviour flyBehaviour;
    private SpellCaster spellCaster;
    private Rigidbody rb;

    private FlyingEnemy flyingEnemy;

    private void Start()
    {
        moveBehaviour = GetComponent<MoveBehaviour>();
        flyBehaviour = GetComponent<FlyBehaviour>();
        spellCaster = GetComponent<SpellCaster>();
        rb = GetComponent<Rigidbody>();

        flyingEnemy = GetComponent<FlyingEnemy>();
    }

    public void Stun(float duration)
    {
        if (IsStunned)
        {
            // Refresh the stun duration if already stunned.
            StunTimer = 0f;
        }

        IsStunned = true;
        StunDuration = duration;
        OnStunStart?.Invoke();

        // Disable movement and casting
        if (moveBehaviour != null) moveBehaviour.enabled = false;
        if (flyBehaviour != null) flyBehaviour.enabled = false;
        if (spellCaster != null) spellCaster.enabled = false;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero; //Stop current movement
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll; //Lock rotation and position
        }

        if(flyingEnemy != null) flyingEnemy.enabled = false;
    }

    private void Update()
    {
        if (IsStunned)
        {
            StunTimer += Time.deltaTime;
            if (StunTimer >= StunDuration)
            {
                IsStunned = false;
                OnStunEnd?.Invoke();

                // Re-enable movement and casting
                if (moveBehaviour != null) moveBehaviour.enabled = true;
                if (flyBehaviour != null) flyBehaviour.enabled = true;
                if (spellCaster != null) spellCaster.enabled = true;
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints.None; //Unlock rotation and position
                }

                if (flyingEnemy != null) flyingEnemy.enabled = true;
            }
        }
    }
}