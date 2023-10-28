using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingManager : MonoBehaviour
{
    public static PostProcessingManager Instance { get; private set; }

    [SerializeField] private PostProcessVolume _lowHealthEffect;
    [SerializeField] private PostProcessVolume _lightingAndBloomEffect;

    Vignette vignette;

    private void OnEnable()
    {
        _lowHealthEffect.profile.TryGetSettings(out vignette);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void setVignetteIntensity(float healthPercentage)
    {
        float missingHealthPercentage = 1 - healthPercentage;
        Debug.Log(missingHealthPercentage);
        FloatParameter newIntensity = new FloatParameter { value = missingHealthPercentage };
        vignette.intensity.value = missingHealthPercentage;
        Debug.Log("Vignette intensity: " + vignette.intensity.value);
    }
}
