using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MobilePlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float rotationSpeed = 10f;

    [Header("UI")]
    public Joystick joystick;
    public Button talkButton;
    public Button runButton;

    // Animator
    private Animator animator;
    private readonly int isWalkingHash = Animator.StringToHash("isWalking");
    private readonly int isRunningHash = Animator.StringToHash("isRunning");
    private readonly int isTalkingHash = Animator.StringToHash("isTalking");

    // State flags
    private bool runButtonHeld = false;
    private bool isInConversation = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (talkButton != null)
            talkButton.onClick.AddListener(OnTalkButtonPressed);

        if (runButton != null)
            AddHoldEventsToRunButton();
    }

    void Update()
    {
        if (isInConversation && (joystick.Horizontal != 0 || joystick.Vertical != 0))
            StopConversation();

        if (!isInConversation)
            HandleMovement();
    }

    void HandleMovement()
    {
        Vector3 direction = new Vector3(joystick.Horizontal, 0f, joystick.Vertical);
        bool hasInput = direction.sqrMagnitude > 0.01f;

        bool shouldRun = hasInput && runButtonHeld;
        float moveSpeed = shouldRun ? runSpeed : walkSpeed;

        if (hasInput)
        {
            direction.Normalize();
            Vector3 moveDirection = Camera.main.transform.TransformDirection(direction);
            moveDirection.y = 0;

            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

            // Rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        animator.SetBool(isWalkingHash, hasInput);
        animator.SetBool(isRunningHash, shouldRun);
    }

    void OnTalkButtonPressed()
    {
        if (isInConversation)
            StopConversation();
        else
            StartConversation();
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

    void AddHoldEventsToRunButton()
    {
        EventTrigger trigger = runButton.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = runButton.gameObject.AddComponent<EventTrigger>();

        var downEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        downEntry.callback.AddListener(_ => runButtonHeld = true);
        trigger.triggers.Add(downEntry);

        var upEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        upEntry.callback.AddListener(_ => runButtonHeld = false);
        trigger.triggers.Add(upEntry);

        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener(_ => runButtonHeld = false);
        trigger.triggers.Add(exitEntry);
    }
}
