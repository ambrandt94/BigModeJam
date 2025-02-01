using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Required for UI elements
using DestroyIt;
using TMPro;

public class DestructibleTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI percentageText; // UI Text element to display the percentage
    [SerializeField] private float updateInterval = 1f; // How often to update the percentage

    private int totalDestructibles;
    private int remainingDestructibles;
    private float timer;

    private void Start()
    {
        // Initialize the total count of destructible objects.
        totalDestructibles = Destructible.AllTrackedDestructibles.Count;
        UpdatePercentage(); // Initial update
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            timer -= updateInterval;
            UpdatePercentage();
        }
    }


    private void UpdatePercentage()
    {
        remainingDestructibles = Destructible.AllTrackedDestructibles.Count;
        if (remainingDestructibles == 0)
        {
            percentageText.text = "0%";
            return;
        }

        float percentage = (float)remainingDestructibles / totalDestructibles * 100f;
        percentageText.text = $"{percentage:F0}%"; // Format to whole number percentage
    }
}