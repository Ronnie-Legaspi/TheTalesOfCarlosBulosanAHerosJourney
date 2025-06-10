using UnityEngine;
using UnityEngine.UI;

public class MobilePlayerController : MonoBehaviour
{
    public ThirdPersonCamera cameraScript;

    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float rotationSpeed = 5f;

    public Joystick joystick;          // Fixed joystick reference
    public Button talkButton;          // UI Talk Button
    public Button runButton;           // UI Run Button

    private Animator animator;
    private int isWalkingHash;
    private int isRunningHash;
    private int isTalkingHash;

    private bool isInConversation = false;
    private bool isRunning = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isTalkingHash = Animator.StringToHash("isTalking");

        if (cameraScript == null)
            cameraScript = FindObjectOfType<ThirdPersonCamera>();

        // Button listeners
        if (talkButton != null)
            talkButton.onClick.AddListener(OnTalkButtonPressed);

        if (runButton != null)
            runButton.onClick.AddListener(() => isRunning = !isRunning);
    }

    void Update()
    {
        if (isInConversation && (joystick.Horizontal != 0 || joystick.Vertical != 0))
        {
            StopConversation();
        }

        if (!isInConversation)
        {
            HandleMovement();
        }

        // Update camera states
        if (cameraScript != null)
        {
            cameraScript.SetMoving(joystick.Vertical != 0);
            cameraScript.SetTalking(isInConversation);
        }
    }

    void HandleMovement()
    {
        Vector3 direction = new Vector3(joystick.Horizontal, 0f, joystick.Vertical).normalized;

        bool isWalking = direction.magnitude >= 0.1f;
        float moveSpeed = isRunning ? runSpeed : walkSpeed;

        if (isWalking)
        {
            // Move
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            // Rotate smoothly in joystick direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        animator.SetBool(isWalkingHash, isWalking);
        animator.SetBool(isRunningHash, isWalking && isRunning);
    }

    void OnTalkButtonPressed()
    {
        if (isInConversation)
        {
            StopConversation();
        }
        else
        {
            StartConversation();
        }
    }

    void StartConversation()
    {
        isInConversation = true;
        animator.SetTrigger("TalkTrigger");
        animator.SetBool(isTalkingHash, true);
        if (cameraScript != null)
            cameraScript.SetTalking(true);
    }

    void StopConversation()
    {
        isInConversation = false;
        animator.SetBool(isTalkingHash, false);
        if (cameraScript != null)
            cameraScript.SetTalking(false);
    }
}
