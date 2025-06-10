using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject achievementsPanel;
    public GameObject leaderboardsPanel;

    public void ShowAchievementsPanel()
    {
        // Set achievements panel active (visible)
        achievementsPanel.SetActive(true);
    }
    public void HideAchievementsPanel()
    {
        achievementsPanel.SetActive(false);
    }
    public void ShowleaderboardsPanel()
    {
        // Set leaderboards panel active (visible)
        leaderboardsPanel.SetActive(true);
    }

    // Optional: A method to hide the achievements panel, if you want to close it by button
    public void HideLeaderboardsPanel()
    {
        leaderboardsPanel.SetActive(false);
    }
}
