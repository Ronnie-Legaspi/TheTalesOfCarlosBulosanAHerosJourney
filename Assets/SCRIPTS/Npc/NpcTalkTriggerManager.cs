using UnityEngine;
using UnityEngine.UI;

public class NpcTalkTriggerManager : MonoBehaviour
{
    public int npcId;
    public Button talkButton;
    public GameObject npcObject; // ← Assign the NPC GameObject that has Npc1Conversation
    private Npc1Conversation npcScript;

    private bool playerInside = false;
    private Transform playerTransform;

    private ThirdPersonCamera mainCamera;

    void Start()
    {
        if (talkButton != null)
            talkButton.gameObject.SetActive(false);

        if (talkButton != null)
            talkButton.onClick.AddListener(TriggerTalk);

        if (npcObject != null)
            npcScript = npcObject.GetComponent<Npc1Conversation>();

        // Find main camera with ThirdPersonCamera script
        mainCamera = FindObjectOfType<ThirdPersonCamera>();
    }

    void Update()
    {
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            TriggerTalk();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            playerTransform = other.transform;

            if (npcScript != null)
                npcScript.playerTransform = playerTransform;

            if (talkButton != null)
                talkButton.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            playerTransform = null;

            if (talkButton != null)
                talkButton.gameObject.SetActive(false);

            if (npcScript != null)
                npcScript.EndConversation();

            if (mainCamera != null)
                mainCamera.ResetCamera(); // ← Return to normal view
        }
    }

    void TriggerTalk()
    {
        if (npcScript != null)
            npcScript.StartConversation();

        if (mainCamera != null)
            mainCamera.ZoomOnTalk(); // ← Zoom camera

        NpcDialogPanelManager.Instance.OpenPanel(npcId);
    }
}
