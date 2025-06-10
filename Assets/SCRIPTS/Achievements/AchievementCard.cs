using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementCardUI : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Slider progressBar;
    public TMP_Text progressText;
    public Image background;
    public GameObject Plus3Thropy; // ✅ Add this in the Inspector and assign your done icon GameObject

    public void Setup(string title, string description, int userProgress, int maxProgress, bool unlocked)
    {
        titleText.text = title;
        descriptionText.text = description;
        progressBar.maxValue = maxProgress;
        progressBar.value = userProgress;
        progressText.text = $"{userProgress} / {maxProgress}";

        // Background transparency
        Color bgColor = background.color;
        background.color = unlocked
            ? new Color(bgColor.r, bgColor.g, bgColor.b, 0f)    // transparent if unlocked
            : new Color(bgColor.r, bgColor.g, bgColor.b, 0.5f); // semi-transparent if locked

        // Done icon visibility
        if (Plus3Thropy != null)
            Plus3Thropy.SetActive(unlocked);

        // Title color
        titleText.color = unlocked ? new Color(0.0f, 0.5f, 0.0f) : Color.gray;

    }
}
