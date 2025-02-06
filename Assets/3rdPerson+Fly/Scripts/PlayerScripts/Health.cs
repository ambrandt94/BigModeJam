using Sirenix.OdinInspector;
using System;
using UnityEngine;


public class Health : MonoBehaviour, IHaveSliderData
{
    public float maxHealth = 100f;
    [SerializeField,ReadOnly]
    private float currentHealth;

    public Action<float> OnSliderDataUpdate { get; set; }

    public event Action OnDeath; // Event for death

    public float CurrentHealth {
        get => currentHealth;
        private set {
            currentHealth = Mathf.Clamp(value, 0, maxHealth); // Keep health within bounds
            OnSliderDataUpdate?.Invoke(currentHealth); // Trigger the health update event

            if (currentHealth <= 0) {
                Die();
            }
        }
    }

    public float CurrentAmount { get => currentHealth; set => currentHealth = value; }
    public float MaxAmount { get => maxHealth; set => maxHealth = value; }

    void Start()
    {
        CurrentHealth = maxHealth; // Initialize health at start
    }

    public void TakeDamage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage. Health: " + CurrentHealth);
    }

    public void Heal(float healAmount)
    {
        CurrentHealth += healAmount;
        Debug.Log(gameObject.name + " healed for " + healAmount + ". Health: " + CurrentHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke(); // Trigger the death event
        Debug.Log(gameObject.name + " has died!");
        // Handle death (e.g., destroy the object, respawn, etc.)
        //Destroy(gameObject); // Example: Destroys the gameobject on death.
    }

    [ButtonGroup("Test"), Button("---")]
    private void LoseHealth()
    {
        CurrentHealth -= maxHealth * .1f;
    }

    [ButtonGroup("Test"), Button("+++")]
    private void GainHealth()
    { 
        CurrentHealth += maxHealth * .1f;
    }
}