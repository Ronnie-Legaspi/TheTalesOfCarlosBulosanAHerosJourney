using UnityEngine;
using UnityEngine.UI;

public class NpcTalkTriggerManager : MonoBehaviour
{
    public int npcId;
    public Button talkButton;

    private bool playerInside = false;

    void Start()
    {
        if (talkButton != null)
            talkButton.gameObject.SetActive(false);

        if (talkButton != null)
            talkButton.onClick.AddListener(TriggerTalk);
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
            if (talkButton != null)
                talkButton.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            if (talkButton != null)
                talkButton.gameObject.SetActive(false);
        }
    }

    void TriggerTalk()
    {
        NpcDialogPanelManager.Instance.OpenPanel(npcId);
    }
}
