using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SpellHotbarSlot : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    public Image icon;
    private SpellCaster spellCaster;

    private void Start()
    {
        spellCaster = FindObjectOfType<SpellCaster>();
    }

    public void SetSpell(BaseSpell newSpell)
    {
        spellCaster.SetSpellInHotbar(slotIndex, newSpell);
    }

    public void OnDrop(PointerEventData eventData)
    {
        SpellDragItem droppedItem = eventData.pointerDrag?.GetComponent<SpellDragItem>();
        if (droppedItem == null) return;

        spellCaster.SwapSpells(slotIndex, droppedItem.slotIndex);
    }
}
