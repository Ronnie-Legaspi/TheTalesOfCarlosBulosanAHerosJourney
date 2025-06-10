using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;
    public Transform cameraTarget;
    public float distance = 4.0f;
    public float height = 2.0f;
    public float zoomSpeed = 2.0f;
    public float minZoom = 2.0f;
    public float maxZoom = 6.0f;
    public float rotationSmoothTime = 0.2f;
    public float returnDelay = 1.0f;

    public bool isTalking = false;
    public bool isMoving = false;

    private Vector3 currentVelocity;
    private float lastActiveTime = 0f;
    private float yawVelocity;
    private float pitch = 15f;
    private float targetYaw;

    private bool isDragging = false;

    void LateUpdate()
    {
        HandleZoom();
        HandleRotationInput();

        if (isTalking)
        {
            CinematicView();
            return;
        }

        // Smooth camera yaw to stay behind player within 90 degrees
        if (!isDragging)
        {
            float playerYaw = player.eulerAngles.y;
            float currentYaw = transform.eulerAngles.y;
            float angleDifference = Mathf.DeltaAngle(currentYaw, playerYaw);

            // Clamp angle within -45 to 45 degrees
            angleDifference = Mathf.Clamp(angleDifference, -45f, 45f);
            targetYaw = playerYaw - angleDifference;
        }

        float smoothYaw = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetYaw, ref yawVelocity, rotationSmoothTime);
        Quaternion rotation = Quaternion.Euler(pitch, smoothYaw, 0f);

        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        Vector3 targetPosition = player.position + Vector3.up * height + offset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 0.1f);
        transform.LookAt(cameraTarget);
    }

    void HandleZoom()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
#endif

#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            float prevDist = (t0.position - t0.deltaPosition - (t1.position - t1.deltaPosition)).magnitude;
            float currDist = (t0.position - t1.position).magnitude;
            float delta = prevDist - currDist;
            distance += delta * 0.01f;
        }
#endif
        distance = Mathf.Clamp(distance, minZoom, maxZoom);
    }

    void HandleRotationInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0)) isDragging = true;
        if (Input.GetMouseButtonUp(0)) isDragging = false;

        if (isDragging)
        {
            float mouseX = Input.GetAxis("Mouse X");
            targetYaw += mouseX * 100f * Time.deltaTime;
        }
#else
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) isDragging = true;
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) isDragging = false;

            if (isDragging && touch.phase == TouchPhase.Moved)
            {
                targetYaw += touch.deltaPosition.x * 0.1f;
            }
        }
#endif
        lastActiveTime = Time.time;
    }

    void CinematicView()
    {
        Quaternion cinematicRot = Quaternion.Euler(20f, player.eulerAngles.y + 15f, 0f);
        Vector3 offset = cinematicRot * new Vector3(0, 1.5f, -distance + 1f);
        transform.position = Vector3.Lerp(transform.position, player.position + offset, Time.deltaTime * 2f);
        transform.LookAt(cameraTarget);
    }

    public void SetTalking(bool talking) => isTalking = talking;
    public void SetMoving(bool moving) => isMoving = moving;
}
