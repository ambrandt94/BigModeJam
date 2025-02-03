using UnityEngine;
using System;

public class SpellCaster : MonoBehaviour
{
    public Vector3 Direction => aimTransform? aimTransform.forward : transform.forward;

    public Transform spellOrigin;
    public Transform aimTransform;
    public BaseSpell[] spells;
    public BaseSpell[] hotbarSpells; // The active spells on hotbar
    public float mana = 100f;
    

    private BaseSpell castedSpell;

    private int currentSpellIndex = 0; // Initialize to -1 to indicate no spell selected initially
    private int currentHotbarIndex = 0; // Index of the selected hotbar slot
    private float[] cooldownTimers;

    public event Action<int> OnSpellSelected; // Event for spell selection (now passes hotbar index)
    public event Action<int, float> OnCooldownUpdated; // Event for cooldown updates
    public Action OnHotbarUpdated; // UI event for updating the hotbar

    private SpellMenuUI spellMenuUI; //Bad practice but its here for now

    private void Start()
    {
        spellMenuUI = GameObject.FindFirstObjectByType<SpellMenuUI>();
        cooldownTimers = new float[spells.Length];

        // Initialize with the first 4 spells
        for (int i = 0; i < hotbarSpells?.Length; i++)
        {
            if (i < spells.Length) hotbarSpells[i] = spells[i];
        }
        OnHotbarUpdated?.Invoke();     

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
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSpell(i); // Select the spell at this hotbar index
            }
        }

        if (spellMenuUI != null && spellMenuUI.IsMenuOpen())
            return; 

        if (Input.GetMouseButtonDown(0) && currentHotbarIndex != -1 && hotbarSpells[currentHotbarIndex] != null) // Cast on LMB click
        {
            Cast(currentHotbarIndex, spellOrigin.position, Direction);
        }


    }

    private void SelectSpell(int hotbarIndex)
    {
        currentHotbarIndex = hotbarIndex;
        OnSpellSelected?.Invoke(hotbarIndex); // Invoke the event with the hotbar index
        Debug.Log("Selected spell at hotbar index: " + hotbarIndex);
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

        BaseSpell spell = hotbarSpells[hotbarIndex];

        if (mana < spell.manaCost || cooldownTimers[Array.IndexOf(spells, spell)] > 0) return; // Mana and cooldown check

        mana -= spell.manaCost;
        spell.Cast(this, origin, direction);

        cooldownTimers[Array.IndexOf(spells, spell)] = spell.cooldown; // Set cooldown
        castedSpell = spell;
        OnCooldownUpdated?.Invoke(Array.IndexOf(spells, spell), spell.cooldown);
    }

        public void Cast(BaseSpell spell, Vector3 origin, Vector3 direction)
    {
        // Old casting logic for LMB click
        //if (spells == null || spells.Length == 0) return;
        //if (spellIndex < 0 || spellIndex >= spells.Length) return;

        //spells[spellIndex].Cast(this, origin, direction);
        // End Old casting logic for LMB click
        castedSpell = spell;
        spell.Cast(this, origin, direction);
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
