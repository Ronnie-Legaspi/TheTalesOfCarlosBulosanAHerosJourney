using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject achievementsPanel;
    // You can add other panels here too, e.g.:
    // public GameObject settingsPanel;
    // public GameObject inventoryPanel;

    // Call this method from your Achievements button OnClick event in the Inspector
    public void ShowAchievementsPanel()
    {
        // Set achievements panel active (visible)
        achievementsPanel.SetActive(true);

        // Optionally, disable other panels if you want only one shown at a time
        // settingsPanel.SetActive(false);
        // inventoryPanel.SetActive(false);
    }

    // Optional: A method to hide the achievements panel, if you want to close it by button
    public void HideAchievementsPanel()
    {
        achievementsPanel.SetActive(false);
    }
}
