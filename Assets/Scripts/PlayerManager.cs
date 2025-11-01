using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private float centerViewAngle = 0f;
    [SerializeField] private float leftViewAngle = -90f;
    [SerializeField] private float rightViewAngle = 90f;
    [SerializeField] private float downViewAngle = 45f;
    [SerializeField] private float leftViewTiltX = 15f; // Tilt when looking left
    [SerializeField] private float leftViewTiltY = -5f; // Downward angle when looking left
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float screenEdgeThreshold = 0.1f;

    [Header("Flashlight Settings")]
    [SerializeField] private Light spotLight;
    [SerializeField] private float maxIntensity = 1.5f;
    [SerializeField] private float fadeSpeed = 8f;
    [SerializeField] private float spotAngle = 45f;
    [SerializeField] private float spotRange = 10f;
    
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

    // Camera Variables
    private float targetYRotation;
    private float currentYRotation;
    private float currentXRotation = 0f;
    private float targetXRotation = 0f;
    private bool isLookingDown = false;

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

    public enum ViewState
    {
        Center,
        Left,
        Right,
        Down
    }

    private ViewState currentViewState = ViewState.Center;

    private void Start()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Initialize Camera
        targetYRotation = centerViewAngle;
        currentYRotation = centerViewAngle;
        transform.rotation = Quaternion.Euler(0f, centerViewAngle, 0f);

        // Initialize Flashlight
        if (spotLight == null)
        {
            spotLight = GetComponentInChildren<Light>();
        }

        if (spotLight != null)
        {
            spotLight.type = LightType.Spot;
            spotLight.spotAngle = spotAngle;
            spotLight.range = spotRange;
            spotLight.intensity = 0;
        }
        else
        {
            Debug.LogError("No Light component found! Please add a Spotlight to the Player object.");
        }

        // Initialize Battery
        currentBattery = maxBattery;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Update()
    {
        // Only handle camera movement if flashlight is off
        if (!isFlashlightOn)
        {
            HandleCameraMovement();
        }

        HandleFlashlightInput();
        UpdateFlashlight();
    }

    private void HandleCameraMovement()
    {
        float mouseX = Input.mousePosition.x / Screen.width;
        float mouseY = Input.mousePosition.y / Screen.height;

        // Handle looking down
        if (mouseY < screenEdgeThreshold && !isLookingDown)
        {
            isLookingDown = true;
            currentViewState = ViewState.Down;
            targetXRotation = downViewAngle;
            targetYRotation = centerViewAngle;
            return;
        }
        else if (mouseY >= screenEdgeThreshold && isLookingDown)
        {
            isLookingDown = false;
            targetXRotation = 0f;
            currentViewState = ViewState.Center;
        }

        // Handle horizontal movement when not looking down
        if (!isLookingDown)
        {
            if (mouseX < screenEdgeThreshold)
            {
                targetYRotation = leftViewAngle;
                targetXRotation = leftViewTiltX;
                currentViewState = ViewState.Left;
            }
            else if (mouseX > 1 - screenEdgeThreshold)
            {
                targetYRotation = rightViewAngle;
                targetXRotation = 0f;
                currentViewState = ViewState.Right;
            }
            else
            {
                targetYRotation = centerViewAngle;
                targetXRotation = 0f;
                currentViewState = ViewState.Center;
            }
        }

        // Smooth rotation interpolation
        currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, rotationSpeed * Time.deltaTime);
        currentXRotation = Mathf.Lerp(currentXRotation, targetXRotation, rotationSpeed * Time.deltaTime);

        // Apply rotations with special handling for left view
        Quaternion targetRotation;
        if (currentViewState == ViewState.Left)
        {
            targetRotation = Quaternion.Euler(currentXRotation, currentYRotation + leftViewTiltY, 0f);
        }
        else
        {
            targetRotation = Quaternion.Euler(currentXRotation, currentYRotation, 0f);
        }

        transform.rotation = targetRotation;
    }

    private void HandleFlashlightInput()
    {
        if (!isFlashlightEnabled) return;

        // Toggle flashlight on mouse click
        if (Input.GetMouseButtonDown(0))
        {
            isFlashlightOn = !isFlashlightOn;
            PlayFlashlightClickSound();

            // Lock camera position when turning on flashlight
            if (isFlashlightOn)
            {
                targetYRotation = currentYRotation;
                targetXRotation = currentXRotation;
            }
        }

        // Update battery when flashlight is on
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

        targetIntensity = isFlashlightOn && isFlashlightEnabled ? maxIntensity : 0f;
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, fadeSpeed * Time.deltaTime);

        float finalIntensity = currentIntensity;

        if (useFlickerEffect && currentIntensity > 0)
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

    // Public methods for UI
    public float GetBatteryPercentage() => (currentBattery / maxBattery) * 100f;
    public bool IsLowBattery() => currentBattery <= lowBatteryThreshold;
    public bool IsFlashlightActive() => isFlashlightOn;
    public ViewState GetCurrentViewState() => currentViewState;
}