// Created by Tacioli21
// Last updated: 2025-11-02 01:48:55 UTC

using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private Light spotLight;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float fadeSpeed = 8f;
    [SerializeField] private float spotRange = 10f;

    [Header("Spotlight Angle Settings")]
    [SerializeField] private float normalSpotAngle = 45f;
    [SerializeField] private float downViewSpotAngle = 90f; // Wider angle when looking down

    [Header("Flashlight Effects")]
    [SerializeField] private bool useFlickerEffect = true;
    [SerializeField] private float flickerIntensity = 0.1f;
    [SerializeField] private float flickerSpeed = 15f;

    [Header("Battery Settings")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float batteryDrainRate = 10f;
    [SerializeField] private float lowBatteryThreshold = 20f;
    [SerializeField] private AudioClip lowBatterySound;
    [SerializeField] private AudioClip batteryDeadSound;
    [SerializeField] private AudioClip flashlightClickSound;

    // Flashlight Variables
    private float currentIntensity = 0f;
    private float targetIntensity = 0f;
    private bool isFlashlightOn = false;

    // Battery Variables
    private float currentBattery;
    private bool isFlashlightEnabled = true;
    private bool hasPlayedLowBatteryWarning = false;
    private bool hasPlayedDeadBatterySound = false;
    private AudioSource audioSource;
    private BedCameraController cameraController;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (spotLight == null)
        {
            spotLight = GetComponentInChildren<Light>();
        }

        if (spotLight != null)
        {
            spotLight.type = LightType.Spot;
            spotLight.spotAngle = normalSpotAngle;
            spotLight.range = spotRange;
            spotLight.intensity = 0;
        }
        else
        {
            Debug.LogError("No Light component found! Please add a Spotlight to the Player object.");
        }

        currentBattery = maxBattery;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        cameraController = GetComponentInChildren<BedCameraController>();
    }

    private void Update()
    {
        HandleFlashlightInput();
        UpdateFlashlight();
    }

    private void HandleFlashlightInput()
    {
        if (!isFlashlightEnabled) return;

        if (Input.GetMouseButtonDown(0))
        {
            isFlashlightOn = !isFlashlightOn;
            PlayFlashlightClickSound();
        }

        if (isFlashlightOn)
        {
            currentBattery -= batteryDrainRate * Time.deltaTime;

            if (currentBattery <= 0)
            {
                BatteryDepleted();
            }
            else if (currentBattery <= lowBatteryThreshold && !hasPlayedLowBatteryWarning)
            {
                PlayLowBatteryWarning();
            }
        }

        currentBattery = Mathf.Max(0f, currentBattery);
    }

    private void UpdateFlashlight()
    {
        if (spotLight == null) return;

        // Update spot angle based on camera view
        if (cameraController != null)
        {
            spotLight.spotAngle = cameraController.IsLookingDown() ? downViewSpotAngle : normalSpotAngle;
        }

        targetIntensity = isFlashlightOn && isFlashlightEnabled ? maxIntensity : 0f;
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, fadeSpeed * Time.deltaTime);

        float finalIntensity = currentIntensity;

        if (useFlickerEffect && isFlashlightOn && currentIntensity > 0)
        {
            float flickerMod = currentBattery < lowBatteryThreshold ? 2f : 1f;
            finalIntensity += Mathf.Sin(Time.time * flickerSpeed) * flickerIntensity * flickerMod;
        }

        finalIntensity *= (currentBattery / maxBattery);
        finalIntensity = Mathf.Clamp(finalIntensity, 0, maxIntensity);
        spotLight.intensity = finalIntensity;
    }

    private void BatteryDepleted()
    {
        isFlashlightEnabled = false;
        isFlashlightOn = false;
        currentBattery = 0f;

        if (!hasPlayedDeadBatterySound)
        {
            PlayDeadBatterySound();
        }
    }

    private void PlayFlashlightClickSound()
    {
        if (flashlightClickSound != null)
        {
            audioSource.PlayOneShot(flashlightClickSound);
        }
    }

    private void PlayLowBatteryWarning()
    {
        if (lowBatterySound != null)
        {
            audioSource.PlayOneShot(lowBatterySound);
            hasPlayedLowBatteryWarning = true;
        }
    }

    private void PlayDeadBatterySound()
    {
        if (batteryDeadSound != null)
        {
            audioSource.PlayOneShot(batteryDeadSound);
            hasPlayedDeadBatterySound = true;
        }
    }

    public float GetBatteryPercentage() => (currentBattery / maxBattery) * 100f;
    public bool IsLowBattery() => currentBattery <= lowBatteryThreshold;
    public bool IsFlashlightActive() => isFlashlightOn;
}