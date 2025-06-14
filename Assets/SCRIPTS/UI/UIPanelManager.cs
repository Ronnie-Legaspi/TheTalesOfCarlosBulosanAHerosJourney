using UnityEngine;

public class UIPanelManager : MonoBehaviour
{
    public GameObject playerControllerPanel;     // The panel to toggle
    public GameObject[] otherPanels;             // List of all other panels

    void Update()
    {
        bool anyOtherPanelOpen = false;

        foreach (GameObject panel in otherPanels)
        {
            if (panel.activeSelf)
            {
                anyOtherPanelOpen = true;
                break;
            }
        }

        // Toggle the PlayerControllerPanel based on other panels
        if (playerControllerPanel != null)
            playerControllerPanel.SetActive(!anyOtherPanelOpen);
    }
}
