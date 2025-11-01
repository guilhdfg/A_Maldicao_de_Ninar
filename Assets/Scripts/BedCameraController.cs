// Created by Tacioli21
// Last updated: 2025-11-01 19:30:51 UTC

using UnityEngine;

public class BedCameraController : MonoBehaviour
{
    [Header("View Settings")]
    [SerializeField] private float centerViewAngle = -30f;
    [SerializeField] private float leftViewAngle = -90f;
    [SerializeField] private float rightViewAngle = 90f;
    [SerializeField] private float downViewAngle = 180f;
    
    [Header("Tilt Settings")]
    [SerializeField] private float leftTiltX = 15f;
    [SerializeField] private float leftTiltY = -5f;
    
    [Header("Movement Settings")]
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float mouseDetectionWidth = 0.3f;
    [SerializeField] private float mouseDetectionHeight = 0.2f;

    [Header("Down View Settings")]
    [SerializeField] private float forwardMoveDistance = 1f;
    [SerializeField] private float downMoveDistance = 0.5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float sequenceDelay = 0.3f;

    [Header("Input Keys")]
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;
    [SerializeField] private KeyCode downKey = KeyCode.S;
    [SerializeField] private KeyCode upKey = KeyCode.W;

    public enum ViewState
    {
        Center,
        Left,
        Right,
        Down
    }

    private enum DownSequenceState
    {
        NotDown,
        MovingForward,
        MovingDown,
        Rotating,
        Complete,
        Unrotating,
        LiftingUp,
        ReturningBack
    }

    private ViewState currentView = ViewState.Center;
    private DownSequenceState downSequence = DownSequenceState.NotDown;
    private float sequenceTimer = 0f;

    private float currentRotationX = 0f;
    private float targetRotationX = 0f;
    private float currentRotationY = 0f;
    private float targetRotationY = 0f;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private PlayerManager playerManager;

    private void Start()
    {
        originalPosition = transform.localPosition;
        targetPosition = originalPosition;
        playerManager = GetComponentInParent<PlayerManager>();

        // Initialize at center angle
        currentRotationY = centerViewAngle;
        targetRotationY = centerViewAngle;
        transform.rotation = Quaternion.Euler(0f, centerViewAngle, 0f);
    }

    private void Update()
    {
        if (playerManager != null && playerManager.IsFlashlightActive())
            return;

        HandleInput();
        UpdateDownSequence();
        UpdateMovement();
    }

    private void HandleInput()
    {
        if (downSequence != DownSequenceState.NotDown && downSequence != DownSequenceState.Complete)
            return;

        float mouseX = Input.mousePosition.x / Screen.width;
        float mouseY = Input.mousePosition.y / Screen.height;

        // Handle return from down view
        if (currentView == ViewState.Down)
        {
            if (Input.GetKeyDown(upKey) || mouseY > (1 - mouseDetectionHeight))
            {
                SetView(ViewState.Center);
                return;
            }
        }
        // Handle entering down view
        else if ((Input.GetKeyDown(downKey) || mouseY < mouseDetectionHeight) && currentView == ViewState.Center)
        {
            SetView(ViewState.Down);
            return;
        }

        // Handle horizontal views
        if (!IsRotating() && currentView != ViewState.Down)
        {
            if (Input.GetKeyDown(leftKey) || mouseX < mouseDetectionWidth)
            {
                SetView(ViewState.Left);
            }
            else if (Input.GetKeyDown(rightKey) || mouseX > (1 - mouseDetectionWidth))
            {
                SetView(ViewState.Right);
            }
            else if (mouseX >= mouseDetectionWidth && mouseX <= (1 - mouseDetectionWidth))
            {
                SetView(ViewState.Center);
            }
        }
    }

    private void SetView(ViewState newView)
    {
        if (currentView == newView)
            return;

        bool isValidTransition = false;
        switch (currentView)
        {
            case ViewState.Center:
                isValidTransition = true;
                break;
            case ViewState.Down:
                isValidTransition = (newView == ViewState.Center);
                if (isValidTransition)
                {
                    StartReverseDownSequence();
                    return;
                }
                break;
            default:
                isValidTransition = (newView == ViewState.Center);
                break;
        }

        if (!isValidTransition)
            return;

        currentView = newView;

        if (newView == ViewState.Down)
        {
            StartDownSequence();
        }
        else
        {
            ResetDownSequence();
            SetRotationForView(newView);
        }
    }

    private void StartDownSequence()
    {
        downSequence = DownSequenceState.MovingForward;
        sequenceTimer = 0f;
        // Instant reset to 0
        currentRotationY = 0f;
        targetRotationY = 0f;
        transform.rotation = Quaternion.Euler(currentRotationX, 0f, 0f);
        targetPosition = originalPosition + transform.forward * forwardMoveDistance;
    }

    private void StartReverseDownSequence()
    {
        downSequence = DownSequenceState.Unrotating;
        sequenceTimer = 0f;
    }

    private void UpdateDownSequence()
    {
        if (currentView != ViewState.Down && downSequence == DownSequenceState.NotDown)
            return;

        sequenceTimer += Time.deltaTime;

        switch (downSequence)
        {
            // Going down sequence
            case DownSequenceState.MovingForward:
                if (sequenceTimer >= sequenceDelay)
                {
                    downSequence = DownSequenceState.MovingDown;
                    sequenceTimer = 0f;
                    targetPosition += Vector3.down * downMoveDistance;
                }
                break;

            case DownSequenceState.MovingDown:
                if (sequenceTimer >= sequenceDelay)
                {
                    downSequence = DownSequenceState.Rotating;
                    sequenceTimer = 0f;
                    targetRotationX = downViewAngle;
                }
                break;

            case DownSequenceState.Rotating:
                if (sequenceTimer >= sequenceDelay)
                {
                    downSequence = DownSequenceState.Complete;
                }
                break;

            // Return sequence
            case DownSequenceState.Unrotating:
                if (sequenceTimer >= sequenceDelay)
                {
                    downSequence = DownSequenceState.LiftingUp;
                    sequenceTimer = 0f;
                    targetRotationX = 0f;
                }
                break;

            case DownSequenceState.LiftingUp:
                if (sequenceTimer >= sequenceDelay && Mathf.Abs(currentRotationX) < 5f)
                {
                    downSequence = DownSequenceState.ReturningBack;
                    sequenceTimer = 0f;
                    targetPosition = originalPosition + transform.forward * forwardMoveDistance;
                }
                break;

            case DownSequenceState.ReturningBack:
                if (sequenceTimer >= sequenceDelay)
                {
                    currentView = ViewState.Center;
                    downSequence = DownSequenceState.NotDown;
                    targetPosition = originalPosition;
                    SetRotationForView(ViewState.Center);
                }
                break;
        }
    }

    private void SetRotationForView(ViewState view)
    {
        switch (view)
        {
            case ViewState.Center:
                targetRotationY = centerViewAngle;
                targetRotationX = 0f;
                break;
            case ViewState.Left:
                targetRotationY = leftViewAngle + leftTiltY;
                targetRotationX = leftTiltX;
                break;
            case ViewState.Right:
                targetRotationY = rightViewAngle;
                targetRotationX = 0f;
                break;
        }
    }

    private void ResetDownSequence()
    {
        if (downSequence != DownSequenceState.Unrotating &&
            downSequence != DownSequenceState.LiftingUp &&
            downSequence != DownSequenceState.ReturningBack)
        {
            downSequence = DownSequenceState.NotDown;
            sequenceTimer = 0f;
            targetPosition = originalPosition;
        }
    }

    private void UpdateMovement()
    {
        // Update position
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);

        // Update rotation
        currentRotationX = Mathf.LerpAngle(currentRotationX, targetRotationX, rotationSpeed * Time.deltaTime);
        currentRotationY = Mathf.LerpAngle(currentRotationY, targetRotationY, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0f);
    }

    private bool IsRotating()
    {
        return Mathf.Abs(currentRotationX - targetRotationX) > 0.1f ||
               Mathf.Abs(currentRotationY - targetRotationY) > 0.1f;
    }

    public ViewState GetCurrentView() => currentView;
    public bool IsLookingDown() => currentView == ViewState.Down;
}