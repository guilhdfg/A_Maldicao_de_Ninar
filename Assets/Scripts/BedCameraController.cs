using UnityEngine;

public class BedCameraController : MonoBehaviour
{
    [Header("View Angles")]
    [SerializeField] private float centerViewAngle = 0f;
    [SerializeField] private float leftViewAngle = -90f;
    [SerializeField] private float rightViewAngle = 90f;
    [SerializeField] private float downViewAngle = 45f;
    [SerializeField] private float leftViewTiltX = 15f;
    [SerializeField] private float leftViewTiltY = -5f;

    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float screenEdgeThreshold = 0.1f;

    [Header("Down View Settings")]
    [SerializeField] private float moveForwardAmount = 0.5f;
    [SerializeField] private float downMovementSpeed = 3f;
    [SerializeField] private float initialDownTiltAngle = 15f;
    [SerializeField] private float finalDownTiltAngle = 180f; // Changed to 180 for upside down view
    [SerializeField] private float downZRotation = 180f; // New: Controls the Z-axis rotation when looking down
    [SerializeField] private float verticalOffset = -0.5f; // How far down to move when looking down

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private float targetYRotation;
    private float currentYRotation;
    private float currentXRotation = 0f;
    private float targetXRotation = 0f;
    private float currentZRotation = 0f;
    private float targetZRotation = 0f;
    private bool isLookingDown = false;
    private bool isMovingForward = false;
    private float downMovementProgress = 0f;

    private PlayerManager playerManager;

    private void Start()
    {
        originalPosition = transform.localPosition;
        targetPosition = originalPosition;
        targetYRotation = centerViewAngle;
        currentYRotation = centerViewAngle;
        transform.rotation = Quaternion.Euler(0f, centerViewAngle, 0f);

        playerManager = GetComponentInParent<PlayerManager>();
    }

    private void Update()
    {
        if (playerManager != null && playerManager.IsFlashlightActive())
            return;

        HandleCameraMovement();
    }

    private void HandleCameraMovement()
    {
        float mouseX = Input.mousePosition.x / Screen.width;
        float mouseY = Input.mousePosition.y / Screen.height;

        if (mouseY < screenEdgeThreshold)
        {
            if (!isLookingDown)
            {
                StartLookingDown();
            }
            UpdateDownwardMovement();
        }
        else if (mouseY >= screenEdgeThreshold)
        {
            if (isLookingDown)
            {
                ReturnFromLookingDown();
            }
            HandleHorizontalMovement(mouseX);
        }

        ApplyRotationAndPosition();
    }

    private void StartLookingDown()
    {
        isLookingDown = true;
        isMovingForward = true;
        downMovementProgress = 0f;
        
        // Calculate forward and downward movement
        Vector3 forwardMove = transform.forward * moveForwardAmount;
        Vector3 downwardMove = Vector3.up * verticalOffset;
        targetPosition = originalPosition + forwardMove + downwardMove;
        
        // Set target rotations for looking down
        targetXRotation = initialDownTiltAngle;
        targetZRotation = downZRotation;
    }

    private void UpdateDownwardMovement()
    {
        if (isMovingForward)
        {
            downMovementProgress += Time.deltaTime * downMovementSpeed;
            downMovementProgress = Mathf.Clamp01(downMovementProgress);

            // Move and rotate simultaneously
            transform.localPosition = Vector3.Lerp(originalPosition, targetPosition, downMovementProgress);
            
            // Calculate rotations based on progress
            float currentDownAngle = Mathf.Lerp(initialDownTiltAngle, finalDownTiltAngle, downMovementProgress);
            float currentZAngle = Mathf.Lerp(0f, downZRotation, downMovementProgress);
            
            targetXRotation = currentDownAngle;
            targetZRotation = currentZAngle;

            if (downMovementProgress >= 1f)
            {
                isMovingForward = false;
            }
        }
    }

    private void ReturnFromLookingDown()
    {
        isLookingDown = false;
        isMovingForward = false;
        targetPosition = originalPosition;
        targetXRotation = 0f;
        targetZRotation = 0f;
    }

    private void HandleHorizontalMovement(float mouseX)
    {
        if (mouseX < screenEdgeThreshold)
        {
            targetYRotation = leftViewAngle;
            targetXRotation = leftViewTiltX;
            targetZRotation = 0f;
        }
        else if (mouseX > 1 - screenEdgeThreshold)
        {
            targetYRotation = rightViewAngle;
            targetXRotation = 0f;
            targetZRotation = 0f;
        }
        else
        {
            targetYRotation = centerViewAngle;
            targetXRotation = 0f;
            targetZRotation = 0f;
        }
    }

    private void ApplyRotationAndPosition()
    {
        // Smooth position interpolation
        if (!isLookingDown)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, downMovementSpeed * Time.deltaTime);
        }

        // Smooth rotation interpolation
        currentYRotation = Mathf.Lerp(currentYRotation, targetYRotation, rotationSpeed * Time.deltaTime);
        currentXRotation = Mathf.Lerp(currentXRotation, targetXRotation, rotationSpeed * Time.deltaTime);
        currentZRotation = Mathf.Lerp(currentZRotation, targetZRotation, rotationSpeed * Time.deltaTime);

        // Apply final rotation
        Quaternion targetRotation;
        if (!isLookingDown && Mathf.Approximately(targetYRotation, leftViewAngle))
        {
            // Special case for left view with tilt
            targetRotation = Quaternion.Euler(currentXRotation, currentYRotation + leftViewTiltY, currentZRotation);
        }
        else
        {
            targetRotation = Quaternion.Euler(currentXRotation, currentYRotation, currentZRotation);
        }

        transform.rotation = targetRotation;
    }

    public bool IsLookingDown() => isLookingDown;
}