using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Camera settings
    public ThirdPersonCamera cameraScript;

    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float rotationSpeed = 5f;

    Animator animator;
    int isWalkingHash;
    int isRunningHash;
    int isTalkingHash;

    bool isInConversation = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isTalkingHash = Animator.StringToHash("isTalking");

        // Optional: auto-assign cameraScript if not manually set
        if (cameraScript == null)
        {
            cameraScript = FindObjectOfType<ThirdPersonCamera>();
        }
    }

    void Update()
    {
        bool forwardPressed = Input.GetKey(KeyCode.W);
        bool leftPressed = Input.GetKey(KeyCode.A);
        bool rightPressed = Input.GetKey(KeyCode.D);
        bool runPressed = Input.GetKey(KeyCode.LeftShift);

        bool anyMovementPressed = forwardPressed || leftPressed || rightPressed;

        // Cancel talking if any movement or Escape
        if (isInConversation && (anyMovementPressed || Input.GetKeyDown(KeyCode.Escape)))
        {
            StopConversation();
        }

        // Start conversation with E
        if (Input.GetKeyDown(KeyCode.E) && !isInConversation)
        {
            StartConversation();
        }

        // Movement only when not in conversation
        if (!isInConversation)
        {
            HandleMovement(forwardPressed, leftPressed, rightPressed, runPressed);
        }

        // Update camera state if script exists
        if (cameraScript != null)
        {
            cameraScript.SetMoving(forwardPressed);
            cameraScript.SetTalking(isInConversation);
        }
    }

    void HandleMovement(bool forwardPressed, bool leftPressed, bool rightPressed, bool runPressed)
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);
        float moveSpeed = runPressed ? runSpeed : walkSpeed;

        // Forward movement
        if (forwardPressed)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

            // Smooth turning left/right
            if (leftPressed)
            {
                Quaternion targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y - 90f, 0f);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            else if (rightPressed)
            {
                Quaternion targetRotation = Quaternion.Euler(0f, transform.eulerAngles.y + 90f, 0f);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }

        // Walking animation
        animator.SetBool(isWalkingHash, forwardPressed);

        // Running animation
        animator.SetBool(isRunningHash, forwardPressed && runPressed);
    }

    void StartConversation()
    {
        isInConversation = true;
        animator.SetTrigger("TalkTrigger");     // Play waving once
        animator.SetBool(isTalkingHash, true);  // Start looping talk
        if (cameraScript != null)
            cameraScript.SetTalking(true);
    }

    void StopConversation()
    {
        isInConversation = false;
        animator.SetBool(isTalkingHash, false); // Exit talk loop
        if (cameraScript != null)
            cameraScript.SetTalking(false);
    }
}
