using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class TrophyLeaderboardManager : MonoBehaviour
{
    public string apiUrl = "http://127.0.0.1:8000/unity/leaderboard/get_user_trophies.php";
    public GameObject cardPrefab;
    public Transform cardParent;

    [System.Serializable]
    public class User
    {
        public string username;
        public string email;
        public string course;
        public int trophy;
    }

    [System.Serializable]
    public class UserResponse
    {
        public bool success;
        public List<User> users;
    }

    void Start()
    {
        StartCoroutine(FetchTrophyData());
    }

    IEnumerator FetchTrophyData()
{
    using (UnityWebRequest www = UnityWebRequest.Get(apiUrl))
    {
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            // Fix JSON if necessary
            string rawJson = www.downloadHandler.text;
            string cleanJson = rawJson.Contains("\"users\"") ? rawJson : "{\"users\":" + rawJson + "}";
            UserResponse response = JsonUtility.FromJson<UserResponse>(cleanJson);

            for (int i = 0; i < response.users.Count; i++)
            {
                var user = response.users[i];
                GameObject card = Instantiate(cardPrefab, cardParent);
                var ui = card.GetComponent<UserTrophyCardUI>();
                ui.Setup(i + 1, user.username, user.email, user.course, user.trophy); // i + 1 = rank
            }
        }
        else
        {
            Debug.LogError("Failed to load data: " + www.error);
        }
    }
}

}
