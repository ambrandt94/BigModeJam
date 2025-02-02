// SpellSlotUI Component (Attach to your Spell Slot Prefab)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellMenuSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public BaseSpell spell;
    public Image spellIcon;
    public Image highlightImage; // For visual feedback
    public SpellMenuUI spellMenu; // Direct reference (Set in the Inspector on the prefab)

    void Start()
    {
        if (highlightImage != null)
        {
            highlightImage.enabled = false; // Disable highlight initially
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked on " + spell.name + " in spell select menu");

        if (spellMenu != null)
        {
            spellMenu.SelectSpell(spell, this); // Pass the SpellMenuSlotUI instance
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (spellMenu != null)
        {
            spellMenu.ShowTooltip(spell);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (spellMenu != null)
        {
            spellMenu.HideTooltip();
        }
    }
}
