using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class UserProfile : MonoBehaviour
{
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI emailText;

    private int userId;

    void Start() // <-- CAPITAL S
    {
        userId = PlayerPrefs.GetInt("user_id", -1);
        if (userId == -1)
        {
            Debug.LogError("User ID not found in PlayerPrefs. Make sure it's set after Login.");
            return;
        }

        StartCoroutine(FetchUserProfile(userId));
    }

    IEnumerator FetchUserProfile(int userId)
    {
        string url = $"http://127.0.0.1:8000/unity/profile/profile_button.php?user_id={userId}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to fetch user profile: " + request.error);
                yield break;
            }

            string jsonResponse = request.downloadHandler.text;
            Debug.Log("Response: " + jsonResponse); // helpful for debugging

            User user = JsonUtility.FromJson<User>(jsonResponse);
            usernameText.text = user.username;
            emailText.text = user.email;
        }
    }

    [System.Serializable]
    public class User
    {
        public string username;
        public string email;
    }
}
