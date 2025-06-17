using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;
    public Transform cameraTarget;

    [Header("Normal Settings")]
    public float distance = 4.0f;
    public float height = 2.0f;

    [Header("Zoom Settings")]
    public float zoomDistance = 2.0f;
    public float zoomHeight = 1.2f;
    public float zoomSideOffset = 1.5f; // ← New: how much to slide to the side
    public float zoomYawOffset = 45f;   // ← New: angle to rotate during zoom
    public float zoomSpeed = 4f;

    [Header("Common Settings")]
    public float smoothTime = 0.2f;

    private Vector3 currentVelocity;
    private float yawOffset = 0f;
    private bool isDragging = false;
    private bool shouldReturn = false;
    private bool isZoomed = false;

    private float currentDistance;
    private float currentHeight;
    private float currentSideOffset;
    private float currentZoomYaw;

    private float defaultYawOffset;

    void Start()
    {
        currentDistance = distance;
        currentHeight = height;
        currentSideOffset = 0f;
        defaultYawOffset = yawOffset;
    }

    void LateUpdate()
    {
        HandleDragInput();

        if (!isDragging && shouldReturn && !isZoomed)
        {
            yawOffset = Mathf.Lerp(yawOffset, defaultYawOffset, Time.deltaTime * 5f);
            if (Mathf.Abs(yawOffset - defaultYawOffset) < 0.1f)
            {
                yawOffset = defaultYawOffset;
                shouldReturn = false;
            }
        }

        float targetYaw = player.eulerAngles.y + yawOffset;
        Quaternion rotation = Quaternion.Euler(10f, targetYaw, 0f);

        // Compute position offset
        Vector3 offset = rotation * new Vector3(currentSideOffset, 0, -currentDistance);
        Vector3 targetPos = player.position + Vector3.up * currentHeight + offset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
        transform.LookAt(cameraTarget);

        UpdateCameraZoomSmoothly();
    }

    void HandleDragInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            shouldReturn = false;
        }
        if (Input.GetMouseButton(0) && isDragging)
        {
            float mouseX = Input.GetAxis("Mouse X");
            yawOffset += mouseX * 2f;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            shouldReturn = true;
        }
#else
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                isDragging = true;
                shouldReturn = false;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                yawOffset += touch.deltaPosition.x * 0.1f;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
                shouldReturn = true;
            }
        }
#endif
    }

    void UpdateCameraZoomSmoothly()
    {
        float targetDist = isZoomed ? zoomDistance : distance;
        float targetHeight = isZoomed ? zoomHeight : height;
        float targetSide = isZoomed ? zoomSideOffset : 0f;
        float targetYaw = isZoomed ? zoomYawOffset : defaultYawOffset;

        currentDistance = Mathf.Lerp(currentDistance, targetDist, Time.deltaTime * zoomSpeed);
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * zoomSpeed);
        currentSideOffset = Mathf.Lerp(currentSideOffset, targetSide, Time.deltaTime * zoomSpeed);
        yawOffset = Mathf.Lerp(yawOffset, targetYaw, Time.deltaTime * zoomSpeed);
    }

    // 🟢 Call this from the NPC or trigger script
    public void ZoomOnTalk()
    {
        isZoomed = true;
        isDragging = false;
    }

    // 🟢 Call this when the player leaves the conversation
    public void ResetCamera()
    {
        isZoomed = false;
        shouldReturn = true;
    }
}
