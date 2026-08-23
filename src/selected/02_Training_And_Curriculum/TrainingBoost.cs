using UnityEngine;

public class TrainingBoost : MonoBehaviour
{
    [Tooltip("Turn this on only when training from mlagents-learn.")]
    public bool enable = true;

    [Range(1f, 40f)]
    [Min(1f)] public float timeScale = 15f;

    [Tooltip("Lower physics load slightly (0.02 default). 0.0167 = 60 Hz, 0.01 = 100 Hz.")]
    public float fixedDeltaTime = 0.0167f;

    void Awake()
    {
        if (!enable) return;

        // Uncap framerate and unbind vsync
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;

        // Reduce rendering work
        AudioListener.pause = true;       // mute + skip audio mixing
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;

        // Speed up sim
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = fixedDeltaTime; // keeps physics steady at higher timeScale

        // Optional: prevent big catch-up spikes
        Time.maximumDeltaTime = 0.05f;
    }
}
