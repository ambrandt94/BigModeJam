using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting; // Required for List

public class SpellMenuUI : MonoBehaviour
{
    public GameObject spellMenuPanel; // The panel containing your spell slots
    public GameObject spellSlotPrefab; // Prefab for individual spell slots in the menu
    public Transform spellSlotContainer; // Parent transform to hold spell slots
    public SpellCaster spellCaster;
    public SpellTooltipUI spellTooltip; // Reference to your tooltip UI

    public List<BaseSpell> availableSpells; // List of all available spells
    public Image[] hotbarIcons;
    public Sprite defaultIcon;

    private bool menuOpen = false;
    public bool IsMenuOpen()
    {
        return menuOpen;
    }

    public BaseSpell selectedSpell;
    private SpellMenuSlotUI currentlyHighlightedSlot;
    public Color selectionColor = Color.yellow;



    void Start()
    {
        spellMenuPanel.SetActive(false); // Initially hide the menu
        PopulateSpellMenu();
        UpdateHotbar();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) // Example: Toggle with K key
        {
            menuOpen = !menuOpen;
            spellMenuPanel.SetActive(menuOpen);

            if (menuOpen)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void PopulateSpellMenu()
    {
        foreach (BaseSpell spell in availableSpells)
        {
            GameObject spellSlot = Instantiate(spellSlotPrefab, spellSlotContainer);
            SpellMenuSlotUI slotUI = spellSlot.GetComponent<SpellMenuSlotUI>();
            slotUI.spellMenu = this; // Assign the reference here!
            slotUI.spell = spell;
            slotUI.spellIcon.sprite = spell.icon;
        }
    }


    public void UpdateHotbar()
    {
        for (int i = 0; i < hotbarIcons.Length; i++)
        {
            if (spellCaster.hotbarSpells[i] != null)
            {
                hotbarIcons[i].sprite = spellCaster.hotbarSpells[i].icon;
                hotbarIcons[i].color = Color.white; // Make sure the color is not set to something transparent
            }
            else
            {
                hotbarIcons[i].sprite = defaultIcon;
                hotbarIcons[i].color = Color.white; // Ensure default icon also has a proper color
            }
        }
    }

    // Drag and Drop Event Handlers
    private void OnBeginDrag(SpellMenuSlotUI slotUI)
    {
        //slotUI.transform.SetParent(spellCaster.transform); // Or another canvas for dragging
        //slotUI.GetComponent<CanvasGroup>().blocksRaycasts = false; // Allow raycasting through the dragged item
    }

    private void OnDrag(SpellMenuSlotUI slotUI)
    {
        //slotUI.transform.position = Input.mousePosition;
    }

    private void OnEndDrag(SpellMenuSlotUI slotUI)
    {
        //slotUI.GetComponent<CanvasGroup>().blocksRaycasts = true;
        //slotUI.transform.SetParent(spellSlotContainer); // Return to original parent

        //// Check for drop target (Hotbar Slot)
        //PointerEventData eventData = new PointerEventData(EventSystem.current);
        //eventData.position = Input.mousePosition;
        //List<RaycastResult> results = new List<RaycastResult>();
        //EventSystem.current.RaycastAll(eventData, results);

        //foreach (RaycastResult result in results)
        //{
        //    SpellHotbarSlot hotbarSlot = result.gameObject.GetComponent<SpellHotbarSlot>();
        //    if (hotbarSlot != null)
        //    {
        //        // *** KEY CHANGE: Call SetSpellInHotbar with the spell from the dragged slot ***
        //        spellCaster.SetSpellInHotbar(hotbarSlot.slotIndex, slotUI.spell);  // Use slotUI.spell
        //        UpdateHotbar(); // Update the hotbar UI
        //        break;
        //    }
        //}

        //// Return to original parent *after* checking for drop target
        //slotUI.transform.SetParent(spellSlotContainer);
        //slotUI.transform.localPosition = Vector3.zero; // Reset local position
    }

    // Tooltip Methods
    public void ShowTooltip(BaseSpell spell)
    {
        spellTooltip.ShowTooltip(spell);
    }

    public void HideTooltip()
    {
        spellTooltip.HideTooltip();
    }

   

    public void SelectSpell(BaseSpell spell, SpellMenuSlotUI slotUI)
    {
        selectedSpell = spell;

        if (currentlyHighlightedSlot != null)
        {
            currentlyHighlightedSlot.highlightImage.enabled = false;
            currentlyHighlightedSlot.spellIcon.color = Color.white; // Reset color of previously selected
        }

        slotUI.highlightImage.enabled = true;
        slotUI.spellIcon.color = selectionColor;
        currentlyHighlightedSlot = slotUI;
    }

    public void DeselectSpell()
    {
        selectedSpell = null;

        if (currentlyHighlightedSlot != null)
        {
            currentlyHighlightedSlot.highlightImage.enabled = false;
            currentlyHighlightedSlot.spellIcon.color = Color.white; // Reset color
            currentlyHighlightedSlot = null;
        }
    }
}

