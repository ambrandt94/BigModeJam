using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpellHUD : MonoBehaviour
{
    public SpellCaster spellCaster;
    public TextMeshProUGUI selectedSpellText;
    public GameObject sliderPrefab; // Prefab for a single slider
    public Transform sliderParent; // Parent object for the sliders

    private Slider[] cooldownSliders;

    private void Start()
    {
        InitializeSliders();
        spellCaster.OnSpellSelected += UpdateSelectedSpell;
        spellCaster.OnCooldownUpdated += UpdateCooldownSlider;

        UpdateSelectedSpell(0);
    }

    private void OnDestroy()
    {
        spellCaster.OnSpellSelected -= UpdateSelectedSpell;
        spellCaster.OnCooldownUpdated -= UpdateCooldownSlider;
    }

    private void InitializeSliders()
    {
        // Destroy existing sliders if reinitializing
        foreach (Transform child in sliderParent)
        {
            Destroy(child.gameObject);
        }

        // Create a slider for each spell
        cooldownSliders = new Slider[spellCaster.spells.Length];
        for (int i = 0; i < spellCaster.spells.Length; i++)
        {
            GameObject sliderObject = Instantiate(sliderPrefab, sliderParent);
            Slider slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = 1; // Start at full (no cooldown)

            Text spellNameText = sliderObject.GetComponentInChildren<Text>();
            if (spellNameText != null)
            {
                spellNameText.text = spellCaster.spells[i].spellName;
            }

            cooldownSliders[i] = slider;
        }
    }

    private void UpdateSelectedSpell(int spellIndex)
    {
        if (spellIndex < 0 || spellIndex >= spellCaster.spells.Length) return;
        selectedSpellText.text = spellCaster.spells[spellIndex].spellName;
    }

    private void UpdateCooldownSlider(int spellIndex, float cooldownTime)
    {
        if (spellIndex < 0 || spellIndex >= cooldownSliders.Length) return;
        cooldownSliders[spellIndex].value = cooldownTime / spellCaster.spells[spellIndex].cooldown;
        if(cooldownSliders[spellIndex].value == 0)
        {
            Destroy(cooldownSliders[spellIndex].gameObject);
        }
    }
}
