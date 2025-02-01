using UnityEngine;
using UnityEngine.UI; // Or TextMeshPro if you use that
using TMPro;

public class WaveUI : MonoBehaviour
{
    public WaveManager waveManager;
    public TextMeshProUGUI waveNameText; // UI element to show the wave name
    public Button waveStartButton;

    private void Start()
    {
        waveStartButton.onClick.AddListener(waveManager.StartWaves);
        waveManager.OnWaveStart += OnWaveStarted;
        waveManager.OnWaveEnd += OnWaveEnded;
        waveManager.OnAllWavesComplete += OnAllWavesCompleted;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            waveManager.StartWaves();
        }
    }

    private void OnWaveStarted(Wave wave)
    {
        waveNameText.text = wave.waveName;
        waveStartButton.gameObject.SetActive(false);
    }

    private void OnWaveEnded(Wave wave)
    {
        // Update UI (e.g., show wave complete message)
    }

    private void OnAllWavesCompleted()
    {
        waveNameText.text = "All Waves Complete!";
        waveStartButton.gameObject.SetActive(true);
    }
}