using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellHotbarSlot : MonoBehaviour, IPointerClickHandler
{
    public int slotIndex;
    public Image icon;
    public SpellCaster spellCaster;
    private SpellHotbarUI spellHotbarUI; // Direct reference
    SpellMenuUI spellMenu;


    private void Start()
    {
        if (spellCaster == null)
        {
            spellCaster = FindObjectOfType<SpellHUD>()?.spellCaster;
        }
        spellHotbarUI = FindObjectOfType<SpellHotbarUI>(); // Get reference to SpellHotbarUI
        spellMenu = FindObjectOfType<SpellMenuUI>(); // Or better, store a reference
    }

    public void SetSpell(BaseSpell newSpell)
    {
        spellCaster.SetSpellInHotbar(slotIndex, newSpell);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked on hotbar slot index " +  slotIndex);       
        if (spellMenu != null && spellMenu.selectedSpell != null)
        {
            spellCaster.SetSpellInHotbar(slotIndex, spellMenu.selectedSpell);

            if (spellHotbarUI != null) // Use the direct reference
            {
                spellHotbarUI.UpdateHotbar(); // Call the UpdateHotbar function in SpellHotbarUI
            }
            else
            {
                Debug.LogError("SpellHotbarUI not found in parent!"); // Debug if not found
            }

            spellMenu.DeselectSpell(); // Deselect after equipping
        }
    }
}
