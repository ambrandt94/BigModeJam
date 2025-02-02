using UnityEngine;
using UnityEngine.EventSystems;

public class SpellDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int slotIndex;
    private Transform originalParent;
    private SpellCaster spellCaster;
    private GameObject placeholder;

    private void Start()
    {
        //spellCaster = FindObjectOfType<SpellCaster>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (spellCaster?.hotbarSpells[slotIndex] == null) return;

        originalParent = transform.parent;
        placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(originalParent);
        placeholder.transform.SetSiblingIndex(transform.GetSiblingIndex());

        transform.SetParent(spellCaster.transform);
        transform.SetAsLastSibling();    
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;

        if (eventData.pointerEnter != null)
        {
            SpellHotbarSlot targetSlot = eventData.pointerEnter.GetComponent<SpellHotbarSlot>();

            if (targetSlot != null)
            {
                spellCaster?.SwapSpells(slotIndex, targetSlot.slotIndex);
            }
        }

        transform.SetParent(originalParent);
        transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());

        Destroy(placeholder);
    }

}
