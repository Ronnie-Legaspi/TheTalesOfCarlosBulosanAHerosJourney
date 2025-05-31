using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class SessionChecker : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CheckSession());
    }

    IEnumerator CheckSession()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://127.0.0.1:8000/unity/check_session.php");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;
            Debug.Log("Session check response: " + json);

            SessionResponse response = JsonUtility.FromJson<SessionResponse>(json);

            if (response.logged_in)
            {
                PlayerPrefs.SetInt("user_id", response.user_id);
                SceneManager.LoadScene("MainMenuScene");
            }
        }
        else
        {
            Debug.LogError("Session check failed: " + www.error);
        }
    }

    [System.Serializable]
    public class SessionResponse
    {
        public bool logged_in;
        public int user_id;
    }
}
