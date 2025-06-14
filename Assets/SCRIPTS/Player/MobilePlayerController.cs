using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;   // ★ for pointer‑down / pointer‑up

public class MobilePlayerController : MonoBehaviour
{
    public ThirdPersonCamera cameraScript;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float runSpeed  = 4f;
    public float rotationSpeed = 5f;

    [Header("UI")]
    public Joystick joystick;     // Fixed Joystick
    public Button   talkButton;   // “Talk” button
    public Button   runButton;    // “Run / Sprint” button (held)

    // Animator hashes
    private Animator animator;
    private readonly int isWalkingHash = Animator.StringToHash("isWalking");
    private readonly int isRunningHash = Animator.StringToHash("isRunning");
    private readonly int isTalkingHash = Animator.StringToHash("isTalking");

    // State flags
    private bool runButtonHeld   = false;   // true while finger is on runButton
    private bool isInConversation = false;

    /* ─────────────────────────────────────────────── */

    void Start()
    {
        animator = GetComponent<Animator>();

        if (cameraScript == null)
            cameraScript = FindObjectOfType<ThirdPersonCamera>();

        /* ---------- UI LISTENERS ---------- */

        // “Talk” is a normal click (toggle)
        if (talkButton != null)
            talkButton.onClick.AddListener(OnTalkButtonPressed);

        // “Run” needs hold behaviour → use pointer‑down/up events
        if (runButton != null)
            AddHoldEventsToRunButton();
    }

    /* ─────────────────────────────────────────────── */

    void Update()
    {
        // Break out of conversation if player moves
        if (isInConversation && (joystick.Horizontal != 0 || joystick.Vertical != 0))
            StopConversation();

        if (!isInConversation)
            HandleMovement();

        // Camera helper (optional – keep your own logic)
        if (cameraScript != null)
        {
            cameraScript.SetMoving(joystick.Vertical != 0 || joystick.Horizontal != 0);
            cameraScript.SetTalking(isInConversation);
        }
    }

    /* ─────────────────────────────────────────────── */
    /* ---------------  MOVEMENT  -------------------- */

    void HandleMovement()
    {
        Vector3 direction = new Vector3(joystick.Horizontal, 0f, joystick.Vertical);
        bool hasInput = direction.sqrMagnitude > 0.01f;   // tiny dead‑zone

        // Run only if BOTH joystick is moved AND run button is held
        bool shouldRun = hasInput && runButtonHeld;
        float moveSpeed = shouldRun ? runSpeed : walkSpeed;

        if (hasInput)
        {
            // Normalise for consistent speed in diagonals
            direction.Normalize();

            // Move
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);

            // Rotate towards movement direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0, targetAngle, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Animator
        animator.SetBool(isWalkingHash, hasInput);
        animator.SetBool(isRunningHash, shouldRun);
    }

    /* ─────────────────────────────────────────────── */
    /* ---------------  TALKING  --------------------- */

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
        cameraScript?.SetTalking(true);
    }

    void StopConversation()
    {
        isInConversation = false;
        animator.SetBool(isTalkingHash, false);
        cameraScript?.SetTalking(false);
    }

    /* ─────────────────────────────────────────────── */
    /* ---------  RUN BUTTON HOLD HELPERS  ----------- */

    void AddHoldEventsToRunButton()
    {
        EventTrigger trigger = runButton.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = runButton.gameObject.AddComponent<EventTrigger>();

        // PointerDown
        var downEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        downEntry.callback.AddListener(_ => runButtonHeld = true);
        trigger.triggers.Add(downEntry);

        // PointerUp / PointerExit
        var upEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        upEntry.callback.AddListener(_ => runButtonHeld = false);
        trigger.triggers.Add(upEntry);

        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener(_ => runButtonHeld = false);
        trigger.triggers.Add(exitEntry);
    }
}
