using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpellTooltipUI : MonoBehaviour
{
    public TextMeshProUGUI spellNameText;
    public TextMeshProUGUI spellDescriptionText;
    public Image spellIcon;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        HideTooltip();
    }

    void Update()
    {
        //Vector2 mousePos = Input.mousePosition;
        //rectTransform.position = mousePos + new Vector2(10f, -10f); // Slight offset from cursor
    }

    public void ShowTooltip(BaseSpell spell)
    {
        spellNameText.text = spell.spellName;
        spellDescriptionText.text = spell.description;
        spellIcon.sprite = spell.icon;
        canvasGroup.alpha = 1; // Show tooltip
    }

    public void HideTooltip()
    {
        canvasGroup.alpha = 0; // Hide tooltip
    }
}
