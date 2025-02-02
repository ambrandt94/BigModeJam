using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;

public class SpellCaster : MonoBehaviour
{
    public Vector3 Direction => aimTransform? aimTransform.forward : transform.forward;

    public Transform spellOrigin;
    public Transform aimTransform;
    public BaseSpell[] spells;
    public BaseSpell[] hotbarSpells; // The active spells on hotbar
    public float mana = 100f;
    

    private BaseSpell castedSpell;

    private int currentSpellIndex;
    private float[] cooldownTimers;

    public event Action<int> OnSpellSelected; // Event for spell selection
    public event Action<int, float> OnCooldownUpdated; // Event for cooldown updates

    public Action OnHotbarUpdated; // UI event for updating the hotbar

    private void Start()
    {
        cooldownTimers = new float[spells.Length];

        // Initialize with the first 4 spells
        for (int i = 0; i < hotbarSpells?.Length; i++)
        {
            if (i < spells.Length) hotbarSpells[i] = spells[i];
            OnHotbarUpdated.Invoke();
        }

      

    }

    private void Update()
    {
        if (castedSpell != null)
            castedSpell.CastingUpdate(this);

        if (this.gameObject.tag != "Player")
            return;

        HandleCooldowns();
      
            for (int i = 0; i < hotbarSpells.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) && hotbarSpells[i] != null)
                {
                    Cast(i, spellOrigin.position, transform.forward);
                }
            }
        

        //if (Input.mouseScrollDelta.y != 0) {
        //    CycleSpells((int)Input.mouseScrollDelta.y);
        //}


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

    //public void Cast(int spellIndex, Vector3 origin, Vector3 direction)
    public void Cast(int hotbarIndex, Vector3 origin, Vector3 direction)
    {
        // Old casting logic for LMB click
        //if (spells == null || spells.Length == 0) return;
        //if (spellIndex < 0 || spellIndex >= spells.Length) return;

        //spells[spellIndex].Cast(this, origin, direction);
        // End Old casting logic for LMB click

        if (hotbarIndex < 0 || hotbarIndex >= hotbarSpells.Length || hotbarSpells[hotbarIndex] == null) return;
        hotbarSpells[hotbarIndex].Cast(this, origin, direction);
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

    public void AssignSpellToHotbar(int hotbarIndex, BaseSpell spell)
    {
        if (hotbarIndex < 0 || hotbarIndex >= hotbarSpells.Length) return;
        hotbarSpells[hotbarIndex] = spell;
        OnHotbarUpdated?.Invoke(); // Notify UI to update
    }

    public void SwapSpells(int indexA, int indexB)
    {
        if (indexA == indexB) return;

        BaseSpell temp = hotbarSpells[indexA];
        hotbarSpells[indexA] = hotbarSpells[indexB];
        hotbarSpells[indexB] = temp;

        OnHotbarUpdated?.Invoke();       
    }

    public void SetSpellInHotbar(int index, BaseSpell newSpell)
    {
        Debug.Log($"Setting spell {newSpell?.name} in hotbar slot {index}");
        hotbarSpells[index] = newSpell;
        OnHotbarUpdated?.Invoke();
    }

}
