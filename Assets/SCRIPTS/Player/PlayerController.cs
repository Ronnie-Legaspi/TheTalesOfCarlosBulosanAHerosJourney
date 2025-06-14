using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float rotationSpeed = 5f;

    [Header("Talk Settings")]
    public KeyCode talkKey = KeyCode.T;

    private Animator animator;
    private readonly int isWalkingHash = Animator.StringToHash("isWalking");
    private readonly int isRunningHash = Animator.StringToHash("isRunning");
    private readonly int isTalkingHash = Animator.StringToHash("isTalking");

    private bool isInConversation = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();

        if (!isInConversation)
        {
            HandleMovement();
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(talkKey))
        {
            if (isInConversation)
                StopConversation();
            else
                StartConversation();
        }

        if (isInConversation && HasMovementInput())
        {
            StopConversation();
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float vertical = Input.GetAxisRaw("Vertical");     // W/S or Up/Down

        Vector3 direction = new Vector3(horizontal, 0f, vertical);
        bool hasInput = direction.sqrMagnitude > 0.01f;

        // Determine if player is holding Shift to run
        bool shouldRun = hasInput && Input.GetKey(KeyCode.LeftShift);
        float moveSpeed = shouldRun ? runSpeed : walkSpeed;

        if (hasInput)
        {
            direction.Normalize();
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            // Rotate towards movement direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0, targetAngle, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Update animator
        animator.SetBool(isWalkingHash, hasInput);
        animator.SetBool(isRunningHash, shouldRun);
    }

    bool HasMovementInput()
    {
        return Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;
    }

    void StartConversation()
    {
        isInConversation = true;
        animator.SetTrigger("TalkTrigger");
        animator.SetBool(isTalkingHash, true);
    }

    void StopConversation()
    {
        isInConversation = false;
        animator.SetBool(isTalkingHash, false);
    }
}
