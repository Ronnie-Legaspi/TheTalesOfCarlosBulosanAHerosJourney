using UnityEngine;

public class PcPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float rotationSpeed = 10f;

    // Animator
    private Animator animator;
    private readonly int isWalkingHash = Animator.StringToHash("isWalking");
    private readonly int isRunningHash = Animator.StringToHash("isRunning");
    private readonly int isTalkingHash = Animator.StringToHash("isTalking");

    // State flags
    private bool isRunning = false;
    private bool isInConversation = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (isInConversation && HasMovementInput())
        {
            StopConversation();
        }

        if (!isInConversation)
        {
            HandleMovement();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (isInConversation)
                StopConversation();
            else
                StartConversation();
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D
        float vertical = Input.GetAxisRaw("Vertical");     // W/S
        Vector3 direction = new Vector3(horizontal, 0f, vertical);
        bool hasInput = direction.sqrMagnitude > 0.01f;

        isRunning = hasInput && Input.GetKey(KeyCode.LeftShift);
        float moveSpeed = isRunning ? runSpeed : walkSpeed;

        if (hasInput)
        {
            direction.Normalize();
            Vector3 moveDirection = Camera.main != null 
                ? Camera.main.transform.TransformDirection(direction) 
                : direction;

            moveDirection.y = 0f;
            moveDirection.Normalize();

            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

            // Rotate towards movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        animator.SetBool(isWalkingHash, hasInput);
        animator.SetBool(isRunningHash, isRunning);
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

    bool HasMovementInput()
    {
        return Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;
    }
}
