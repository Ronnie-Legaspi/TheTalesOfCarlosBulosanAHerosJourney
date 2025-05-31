using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;

public class PartDescriptionController : MonoBehaviour
{
    public GameObject descriptionPanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public ScrollRect scrollRect;
    public Button nextButton;
    public Button closeButton;

    public float typingSpeed = 0.02f;
    public int linesPerPage = 5;

    private int partNumber;
    private int userId;
    private string[] allLines;
    private int currentLineIndex = 0;
    private bool isTyping = false;

    void Start()
    {
        userId = PlayerPrefs.GetInt("user_id", -1);
        partNumber = PlayerPrefs.GetInt("current_part_number", -1);

        if (userId == -1 || partNumber == -1)
        {
            Debug.LogError("Missing user_id or part_number in PlayerPrefs.");
            return;
        }

        nextButton.onClick.AddListener(OnNextClicked);
        closeButton.onClick.AddListener(() =>
        {
            descriptionPanel.SetActive(false);
        });

        nextButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);

        StartCoroutine(CheckAndShowDescription());
    }

    IEnumerator CheckAndShowDescription()
    {
        string url = $"http://127.0.0.1:8000/unity/get_part_description.php?part_number={partNumber}&user_id={userId}";
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch part description: " + request.error);
            yield break;
        }

        Debug.Log("Received description JSON: " + request.downloadHandler.text);
        PartDescriptionData data = JsonUtility.FromJson<PartDescriptionData>(request.downloadHandler.text);

        if (!data.is_introduced)
        {
            titleText.text = data.title;
            allLines = data.description.Split('\n');
            currentLineIndex = 0;

            descriptionText.text = "";
            descriptionPanel.SetActive(true);
            scrollRect.verticalNormalizedPosition = 1f;

            StartCoroutine(TypeDescription());
            StartCoroutine(StoreTrigger());
        }
        else
        {
            descriptionPanel.SetActive(false);
        }
    }

    IEnumerator TypeDescription()
    {
        isTyping = true;
        nextButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
        descriptionText.text = "";

        int linesToDisplay = Mathf.Min(linesPerPage, allLines.Length - currentLineIndex);
        string linesSegment = "";

        for (int i = 0; i < linesToDisplay; i++)
        {
            linesSegment += allLines[currentLineIndex + i] + "\n";
        }

        foreach (char c in linesSegment)
        {
            descriptionText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        currentLineIndex += linesToDisplay;
        isTyping = false;

        if (currentLineIndex < allLines.Length)
        {
            nextButton.gameObject.SetActive(true);
        }
        else
        {
            closeButton.gameObject.SetActive(true);
        }
    }

    void OnNextClicked()
    {
        if (!isTyping)
        {
            scrollRect.verticalNormalizedPosition = 1f;
            nextButton.gameObject.SetActive(false);
            StartCoroutine(TypeDescription());
        }
    }

    IEnumerator StoreTrigger()
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("part_number", partNumber);

        UnityWebRequest request = UnityWebRequest.Post("http://127.0.0.1:8000/unity/store_part_trigger.php", form);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError("Failed to store trigger: " + request.error);
        else
            Debug.Log("Stored part trigger: " + request.downloadHandler.text);
    }

    [System.Serializable]
    public class PartDescriptionData
    {
        public string title;
        public string description;
        public bool is_introduced;
    }
}
