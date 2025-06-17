using UnityEngine;

public class Npc1Conversation : MonoBehaviour
{
    public Animator npcAnimator;
    public Transform playerTransform;
    public float rotationSpeed = 3f;

    private Quaternion originalRotation;
    private bool isTalking = false;

    void Start()
    {
        // Use Animator on same GameObject if not manually assigned
        if (npcAnimator == null)
            npcAnimator = GetComponent<Animator>();

        // Auto-detect player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (isTalking && playerTransform != null)
        {
            Vector3 direction = playerTransform.position - transform.position;
            direction.y = 0; // ignore vertical difference
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
        else
        {
            // Smoothly return to original facing direction
            transform.rotation = Quaternion.Slerp(transform.rotation, originalRotation, Time.deltaTime * rotationSpeed);
        }
    }

    public void StartConversation()
    {
        isTalking = true;
        if (npcAnimator != null)
            npcAnimator.SetBool("isTalking", true);
    }

    public void EndConversation()
    {
        isTalking = false;
        if (npcAnimator != null)
            npcAnimator.SetBool("isTalking", false);
    }
}
