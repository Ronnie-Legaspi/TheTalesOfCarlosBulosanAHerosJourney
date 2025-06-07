using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AchievementManager : MonoBehaviour
{
    [System.Serializable]
    public class Achievement
    {
        public int id;
        public string title;
        public string description;
        public int max_progress;
        public int user_progress;
    }

    public Transform contentParent; // ScrollView > Viewport > Content
    public GameObject achievementCardPrefab; // assign your prefab in inspector

    void Start()
    {
        LoadAchievements();
    }

    public void LoadAchievements()
    {
        StartCoroutine(GetAchievements());
    }

    IEnumerator GetAchievements()
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", PlayerPrefs.GetInt("user_id"));

        using (UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/unity/achievements/get_achievements.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                yield break;
            }

            List<Achievement> achievementList = JsonUtilityWrapper.FromJsonList<Achievement>(www.downloadHandler.text);

            foreach (Achievement ach in achievementList)
            {
                GameObject card = Instantiate(achievementCardPrefab, contentParent);
                AchievementCardUI cardUI = card.GetComponent<AchievementCardUI>();

                bool unlocked = ach.user_progress >= ach.max_progress;
                cardUI.Setup(ach.title, ach.description, ach.user_progress, ach.max_progress, unlocked);
            }
        }
    }
}
