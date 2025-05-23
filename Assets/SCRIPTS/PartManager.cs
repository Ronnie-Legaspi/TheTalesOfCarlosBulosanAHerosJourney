using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement; // Added for scene loading
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Part {
    public int number;
    public string image;
    public string title;
    public bool enabled;
}

[System.Serializable]
public class PartListWrapper {
    public List<Part> parts;
}

public class PartManager : MonoBehaviour
{
    public string partsUrl = "http://localhost/unity/get_parts.php";
    public GameObject partCardPrefab;
    public Transform contentContainer;

    void Start()
    {
        StartCoroutine(LoadParts());
    }

    IEnumerator LoadParts()
    {
        UnityWebRequest request = UnityWebRequest.Get(partsUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch parts: " + request.error);
            yield break;
        }

        Debug.Log("Received JSON: " + request.downloadHandler.text);

        // Wrap JSON array into an object for JsonUtility
        string jsonWrapped = "{\"parts\":" + request.downloadHandler.text + "}";
        PartListWrapper partList = JsonUtility.FromJson<PartListWrapper>(jsonWrapped);

        foreach (Part part in partList.parts)
        {
            GameObject card = Instantiate(partCardPrefab, contentContainer);

            // Set text fields with TMP_Text
            TMP_Text numberText = card.transform.Find("PartNumber")?.GetComponent<TMP_Text>();
            if (numberText != null)
                numberText.text = "Part #" + part.number;

            TMP_Text titleText = card.transform.Find("PartTitle")?.GetComponent<TMP_Text>();
            if (titleText != null)
                titleText.text = part.title;

            // Load image from Resources folder
            Sprite partSprite = Resources.Load<Sprite>("Parts/" + part.image);
            Image img = card.transform.Find("PartImage")?.GetComponent<Image>();
            if (partSprite != null)
                img.sprite = partSprite;
            else
                Debug.LogWarning("Missing image for: " + part.image);

            // Access the Button component
            Button btn = card.GetComponent<Button>();

            if (part.enabled)
            {
                // Enable interaction and scene navigation
                if (btn != null)
                {
                    int sceneNumber = part.number; // Avoid closure issue
                    btn.onClick.AddListener(() =>
                    {
                        string sceneName = "Part" + sceneNumber + "Scene";
                        Debug.Log("Loading scene: " + sceneName);
                        SceneManager.LoadScene(sceneName);
                    });
                }
            }
            else
            {
                // Disable interaction and gray out
                if (btn != null) btn.interactable = false;

                Image cardImage = card.GetComponent<Image>();
                if (cardImage != null)
                    cardImage.color = Color.gray;

                Transform overlay = card.transform.Find("Overlay");
                if (overlay != null)
                    overlay.gameObject.SetActive(true);
            }
        }
    }
}
