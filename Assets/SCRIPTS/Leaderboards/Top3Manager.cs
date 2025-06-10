using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

[System.Serializable]
public class TopUser {
    public string username;
    public string email;
    public string course;
    public int total_trophy;
}

[System.Serializable]
public class Top3Wrapper {
    public TopUser top1;
    public TopUser top2;
    public TopUser top3;
}


public class Top3Manager : MonoBehaviour
{
    public string apiUrl = "http://127.0.0.1:8000/unity/leaderboard/get_top3.php";

    public TMP_Text top1Username, top1Email, top1Course, top1Trophy;
    public TMP_Text top2Username, top2Email, top2Course, top2Trophy;
    public TMP_Text top3Username, top3Email, top3Course, top3Trophy;

    void Start()
    {
        StartCoroutine(FetchTop3());
    }

    IEnumerator FetchTop3()
    {
        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error fetching top 3: " + request.error);
            yield break;
        }

        Debug.Log("JSON: " + request.downloadHandler.text);

        Top3Wrapper top3Data = JsonUtility.FromJson<Top3Wrapper>(request.downloadHandler.text);

        // Top 1
        top1Username.text = top3Data.top1.username;
        top1Email.text = top3Data.top1.email;
        top1Course.text = top3Data.top1.course;
        top1Trophy.text = top3Data.top1.total_trophy.ToString();

        // Top 2
        top2Username.text = top3Data.top2.username;
        top2Email.text = top3Data.top2.email;
        top2Course.text = top3Data.top2.course;
        top2Trophy.text = top3Data.top2.total_trophy.ToString();

        // Top 3
        top3Username.text = top3Data.top3.username;
        top3Email.text = top3Data.top3.email;
        top3Course.text = top3Data.top3.course;
        top3Trophy.text = top3Data.top3.total_trophy.ToString();
    }
}
