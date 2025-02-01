using UnityEngine;
using System;

public class SpellCaster : MonoBehaviour
{
    public Vector3 Direction => aimTransform? aimTransform.forward : transform.forward;

    public Transform spellOrigin;
    public Transform aimTransform;
    public BaseSpell[] spells;
    public float mana = 100f;

    private BaseSpell castedSpell;

    private int currentSpellIndex;
    private float[] cooldownTimers;

    public event Action<int> OnSpellSelected; // Event for spell selection
    public event Action<int, float> OnCooldownUpdated; // Event for cooldown updates

    private void Start()
    {
        cooldownTimers = new float[spells.Length];

    }

    private void Update()
    {
        if (castedSpell != null)
            castedSpell.CastingUpdate(this);

        if (this.gameObject.tag != "Player")
            return;

        HandleCooldowns();
        if (Input.GetMouseButtonDown(0)) {
            CastCurrentSpell();
        }

        if (Input.mouseScrollDelta.y != 0) {
            CycleSpells((int)Input.mouseScrollDelta.y);
        }

      
    }

    private void HandleCooldowns()
    {
        for (int i = 0; i < spells.Length; i++) {
            if (cooldownTimers[i] > 0) {
                cooldownTimers[i] -= Time.deltaTime;
                OnCooldownUpdated?.Invoke(i, Mathf.Max(0, cooldownTimers[i]));
            }
        }
    }

    public void Cast(int spellIndex, Vector3 origin, Vector3 direction)
    {
        if (spells == null || spells.Length == 0) return;
        if (spellIndex < 0 || spellIndex >= spells.Length) return;

        spells[spellIndex].Cast(this, origin, direction);
    }

    public bool HasSpells()
    {
        return spells != null && spells.Length > 0;
    }

    private void CastCurrentSpell()
    {
        if (currentSpellIndex < 0 || currentSpellIndex >= spells.Length) return;
        var spell = spells[currentSpellIndex];
        if (mana < spell.manaCost || cooldownTimers[currentSpellIndex] > 0) return;

        mana -= spell.manaCost;
        castedSpell = spell;
        spell.Cast(this, spellOrigin.position, Direction);

        cooldownTimers[currentSpellIndex] = spell.cooldown; // Set cooldown
        OnCooldownUpdated?.Invoke(currentSpellIndex, spell.cooldown);
    }

    private void CycleSpells(int direction)
    {
        currentSpellIndex = (currentSpellIndex + direction + spells.Length) % spells.Length;
        OnSpellSelected?.Invoke(currentSpellIndex);
    }
}
