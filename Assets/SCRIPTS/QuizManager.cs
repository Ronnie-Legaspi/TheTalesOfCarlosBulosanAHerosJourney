using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

[System.Serializable]
public class Choice { public int id; public string choice_text; }

[System.Serializable]
public class Question { public int id; public string question_text; public List<Choice> choices; }

[System.Serializable]
public class QuizWrapper { public List<Question> questions; }

[System.Serializable]
public class AnswerResponse
{
    public string status;
    public int correct;
    public int questionCount;
    public int score;
}

[System.Serializable]
public class PerformanceEntry
{
    public string question_text;
    public string user_choice;
    public string correct_answer;
    public int is_correct;
}
[System.Serializable]
public class QuizTitleResponse
{
    public string title;
}
public class QuizManager : MonoBehaviour
{
    [Header("Quiz UI")]
    public TMP_Text questionText;
    public GameObject choiceButtonPrefab;
    public Transform choiceContainer;
    public Button nextButton;

    public TMP_Text quizTitleTMP;


    [Header("Result UI")]
    public GameObject resultPanel;
    public TMP_Text correctText, questionCountText, percentageText;
    public TMP_Text ResultLabelTMP;
    public GameObject Star0, Star1, Star2, Star3;
    public Button closeButton;

    [Header("Performance UI")]
    public Button viewMyPerformanceButton;
    public GameObject performancePanel;
    public Transform performanceContainer;
    public GameObject performanceEntryPrefab;

    [Header("Status Icons")]
    public Sprite checkIcon;
    public Sprite crossIcon;

    private List<Question> questions = new();
    private int currentIndex = 0;
    private int correctCount = 0;
    private int questionCount = 0;
    private int userId;
    private int quizId;
    private int selectedChoiceId = -1;
    private Button selectedButton = null;

    void Start()
    {
        userId = PlayerPrefs.GetInt("user_id", -1);
        if (userId == -1)
        {
            Debug.LogError("User ID not found in PlayerPrefs.");
            return;
        }

        nextButton.onClick.AddListener(OnNextClicked);
        closeButton.onClick.AddListener(CloseResultPanel);
        viewMyPerformanceButton.onClick.AddListener(ViewMyPerformance);

        StartCoroutine(FetchQuizNum());
        // StartCoroutine(FetchQuizTitle());
    }

    IEnumerator FetchQuizTitle()
    {
        string url = $"http://127.0.0.1:8000/unity/quiz/get_quiz_title.php?quiz_id={quizId}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            try
            {
                QuizTitleResponse response = JsonUtility.FromJson<QuizTitleResponse>(www.downloadHandler.text);
                quizTitleTMP.text = response.title;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error parsing quiz title: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Failed to fetch quiz title: " + www.error);
        }
    }

   IEnumerator FetchQuizNum()
    {
        UnityWebRequest www = UnityWebRequest.Get($"http://127.0.0.1:8000/unity/quiz/get_quiz_num.php?user_id={userId}");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            if (int.TryParse(www.downloadHandler.text.Trim(), out int quizNum))
            {
                quizId = quizNum + 1;

                // ✅ Now that quizId is available, fetch title
                StartCoroutine(FetchQuizTitle());

                // ✅ Continue with questions
                StartCoroutine(FetchQuestions());
            }
            else
            {
                Debug.LogError("Failed to parse quiz number.");
            }
        }
        else
        {
            Debug.LogError("Quiz num fetch error: " + www.error);
        }
    }


    IEnumerator FetchQuestions()
    {
        UnityWebRequest www = UnityWebRequest.Get($"http://127.0.0.1:8000/unity/quiz/quiz.php?quiz_id={quizId}");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string rawJson = www.downloadHandler.text.Trim();
            string wrappedJson = "{\"questions\":" + rawJson + "}";

            try
            {
                QuizWrapper wrapper = JsonUtility.FromJson<QuizWrapper>(wrappedJson);
                questions = wrapper.questions;
                ShowQuestion();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JSON parse failed: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("FetchQuestions error: " + www.error);
        }
    }

    void ShowQuestion()
    {
        selectedChoiceId = -1;
        selectedButton = null;
        nextButton.interactable = false;
        ClearChoices();

        if (currentIndex < questions.Count)
        {
            var q = questions[currentIndex];
            questionText.text = q.question_text;

            foreach (var choice in q.choices)
            {
                GameObject obj = Instantiate(choiceButtonPrefab, choiceContainer);
                TMP_Text textComponent = obj.GetComponentInChildren<TMP_Text>();
                textComponent.text = choice.choice_text;

                Button btn = obj.GetComponent<Button>();
                int localChoiceId = choice.id;

                btn.onClick.AddListener(() =>
                {
                    selectedChoiceId = localChoiceId;
                    nextButton.interactable = true;

                    if (selectedButton != null)
                        SetButtonColor(selectedButton, Color.white);

                    selectedButton = btn;
                    SetButtonColor(selectedButton, Color.cyan);
                });
            }

            nextButton.GetComponentInChildren<TMP_Text>().text =
                (currentIndex == questions.Count - 1) ? "Submit" : "Next";
        }
    }

    void SetButtonColor(Button btn, Color color)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor = color;
        btn.colors = cb;
    }

    void OnNextClicked()
    {
        if (selectedChoiceId != -1)
            StartCoroutine(SendAnswer());
    }

    IEnumerator SendAnswer()
    {
        var q = questions[currentIndex];
        WWWForm form = new();
        form.AddField("user_id", userId);
        form.AddField("quiz_id", quizId);
        form.AddField("question_id", q.id);
        form.AddField("choice_id", selectedChoiceId);

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/unity/quiz/submit_answer.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            try
            {
                AnswerResponse response = JsonUtility.FromJson<AnswerResponse>(www.downloadHandler.text.Trim());

                correctCount += response.correct;
                questionCount = response.questionCount;

                currentIndex++;
                if (currentIndex >= questions.Count)
                    ShowResults();
                else
                    ShowQuestion();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Answer response parse failed: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Submit answer error: " + www.error);
        }
    }

   void ShowResults()
    {
        questionText.transform.parent.gameObject.SetActive(false);
        resultPanel.SetActive(true);

        int percent = Mathf.RoundToInt((float)correctCount * 100f / questionCount);

        correctText.text = correctCount.ToString();
        questionCountText.text = questionCount.ToString();
        percentageText.text = $"Score: {percent}%";

        if (percent == 100)
        {
            ResultLabelTMP.text = "Perfect!";
            ActivateStar(3);
        }
        else if (percent > 60)
        {
            ResultLabelTMP.text = "Great!";
            ActivateStar(3);
        }
        else if (percent > 30)
        {
            ResultLabelTMP.text = "Nice!";
            ActivateStar(2);
        }
        else if (percent > 0)
        {
            ResultLabelTMP.text = "Keep Learning!";
            ActivateStar(1);
        }
        else
        {
            ResultLabelTMP.text = "Better Luck Next Time!";
            ActivateStar(0);
        }

        StartCoroutine(IncrementQuizNum());
    }

    void ActivateStar(int count)
    {
        Star0.SetActive(count == 0);
        Star1.SetActive(count == 1);
        Star2.SetActive(count == 2);
        Star3.SetActive(count == 3);
    }

    void ClearChoices()
    {
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);
    }

    void CloseResultPanel()
    {
        resultPanel.SetActive(false);
        questionText.transform.parent.gameObject.SetActive(true);
        currentIndex = 0;
        correctCount = 0;
        questionCount = 0;
        ClearChoices();
        ShowQuestion();
    }

    void ViewMyPerformance()
    {
        performancePanel.SetActive(true);
        StartCoroutine(FetchPerformance());
    }

    IEnumerator FetchPerformance()
    {
        foreach (Transform child in performanceContainer)
            Destroy(child.gameObject);

        string url = $"http://127.0.0.1:8000/unity/quiz/get_performance.php?user_id={userId}&quiz_id={quizId}";
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text.Trim();
            if (string.IsNullOrEmpty(json)) yield break;

            try
            {
                PerformanceEntry[] entries = JsonHelper.FromJson<PerformanceEntry>(json);
                foreach (var entry in entries)
                {
                    var row = Instantiate(performanceEntryPrefab, performanceContainer);
                    row.transform.Find("QuestionText").GetComponent<TMP_Text>().text = entry.question_text;
                    row.transform.Find("YourAnswer").GetComponent<TMP_Text>().text = entry.user_choice;
                    row.transform.Find("CorrectAnswer").GetComponent<TMP_Text>().text = entry.correct_answer;

                    Image icon = row.transform.Find("StatusIcon").GetComponent<Image>();
                    icon.sprite = (entry.is_correct == 1) ? checkIcon : crossIcon;
                    icon.color = (entry.is_correct == 1) ? Color.green : Color.red;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Performance parse error: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Fetch performance error: " + www.error);
        }
    }

    IEnumerator IncrementQuizNum()
    {
        UnityWebRequest www = UnityWebRequest.Get($"http://127.0.0.1:8000/unity/quiz/increment_quiz_num.php?user_id={userId}");
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Quiz number incremented: " + www.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Increment quiz number failed: " + www.error);
        }
    }
}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{\"Items\":" + json + "}";
        return JsonUtility.FromJson<Wrapper<T>>(newJson).Items;
    }

    [System.Serializable]
    private class Wrapper<T> { public T[] Items; }
}
