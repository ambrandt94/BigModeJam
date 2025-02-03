using UnityEngine;
using UnityEngine.UI;

public class SpellHotbarUI : MonoBehaviour
{
    public SpellCaster spellCaster;
    public Image[] hotbarIcons;
    public Sprite defaultIcon; // Default empty slot icon

    private void OnEnable()
    {
        if(spellCaster == null)
        {
            spellCaster = FindObjectOfType<SpellHUD>()?.spellCaster;
        }

        if (spellCaster != null)
        {
            spellCaster.OnHotbarUpdated += UpdateHotbar;
        }
    }

    private void OnDisable()
    {
        if (spellCaster != null)
        {
            spellCaster.OnHotbarUpdated -= UpdateHotbar;
        }
    }

    private void Start()
    {
        UpdateHotbar(); // Refresh hotbar on start
    }

    public void UpdateHotbar()
    {
        Debug.Log("Updating Hotbar UI...");

        for (int i = 0; i < hotbarIcons.Length; i++)
        {
            if (spellCaster.hotbarSpells[i] != null)
                hotbarIcons[i].sprite = spellCaster.hotbarSpells[i].icon; // ✅ Correctly set swapped icons
            else
                hotbarIcons[i].sprite = defaultIcon; // ✅ Ensure empty slots display correctly

            hotbarIcons[i].transform.localPosition = Vector3.zero;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>()); // ✅ Force Unity UI refresh
    }

}
