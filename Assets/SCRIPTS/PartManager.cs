using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PartManager : MonoBehaviour
{
    public string partsUrl = "http://127.0.0.1:8000/unity/get_parts.php";
    public GameObject partCardPrefab;
    public Transform contentContainer;

    private int userId;

    void Start()
    {
        userId = PlayerPrefs.GetInt("user_id", -1);
        if (userId == -1)
        {
            Debug.LogError("User ID not found in PlayerPrefs. Make sure it's set after login.");
            return;
        }

        Debug.Log("Starting to load parts for user ID: " + userId);
        StartCoroutine(LoadParts());
    }

    IEnumerator LoadParts()
    {
        Debug.Log("Requesting parts from: " + partsUrl);
        UnityWebRequest request = UnityWebRequest.Get(partsUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch parts: " + request.error);
            Debug.LogError("Response Code: " + request.responseCode);
            Debug.LogError("Requested URL: " + partsUrl);
            yield break;
        }

        Debug.Log("Raw response: " + request.downloadHandler.text);

        string jsonWrapped = "{\"parts\":" + request.downloadHandler.text + "}";
        Debug.Log("Wrapped JSON: " + jsonWrapped);

        PartListWrapper partList = JsonUtility.FromJson<PartListWrapper>(jsonWrapped);

        foreach (Part part in partList.parts)
        {
            GameObject card = Instantiate(partCardPrefab, contentContainer);
            Debug.Log("Instantiated card for Part #" + part.number + " - " + part.title);

            TMP_Text numberText = card.transform.Find("PartNumber")?.GetComponent<TMP_Text>();
            if (numberText != null) numberText.text = "Part #" + part.number;

            TMP_Text titleText = card.transform.Find("PartTitle")?.GetComponent<TMP_Text>();
            if (titleText != null) titleText.text = part.title;

            Sprite partSprite = Resources.Load<Sprite>("Parts/" + part.image);
            Image img = card.transform.Find("PartImage")?.GetComponent<Image>();
            if (img != null && partSprite != null)
                img.sprite = partSprite;
            else
                Debug.LogWarning("Missing image for: " + part.image);

            Button btn = card.GetComponent<Button>();

            if (part.enabled)
            {
                if (btn != null)
                {
                    int sceneNumber = part.number;
                    btn.onClick.AddListener(() =>
                    {
                        string sceneName = "Part" + sceneNumber + "Scene";

                        PlayerPrefs.SetInt("current_part_number", sceneNumber);
                        PlayerPrefs.SetInt("user_id", userId);

                        SceneLoader.TargetScene = sceneName;
                        SceneManager.LoadScene("LoadingScene");
                    });
                }
            }
            else
            {
                if (btn != null) btn.interactable = false;

                Image cardImage = card.GetComponent<Image>();
                if (cardImage != null) cardImage.color = Color.gray;

                Transform overlay = card.transform.Find("Overlay");
                if (overlay != null) overlay.gameObject.SetActive(true);
            }
        }

        Debug.Log("Finished loading parts.");
    }

    [System.Serializable]
    public class Part
    {
        public int number;
        public string title;
        public string image;
        public bool enabled;
    }

    [System.Serializable]
    public class PartListWrapper
    {
        public Part[] parts;
    }
}
