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
    public int wrong;
    public int score;
}

public class QuizManager : MonoBehaviour
{
    public TMP_Text questionText;
    public GameObject choiceButtonPrefab;
    public Transform choiceContainer;
    public Button nextButton;
    public GameObject resultPanel;
    public TMP_Text correctText, wrongText, percentageText;

    private List<Question> questions = new();
    private int currentIndex = 0;
    private int correctCount = 0;
    private int wrongCount = 0;
    private int userId;
    private int selectedChoiceId = -1;
    private Button selectedButton = null;

    void Start()
    {
        userId = PlayerPrefs.GetInt("user_id", 1);
        nextButton.onClick.AddListener(OnNextClicked);
        StartCoroutine(FetchQuestions());
    }

    IEnumerator FetchQuestions()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://127.0.0.1:8000/unity/quiz/quiz.php?quiz_id=1");
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
                Debug.LogError("JSON parsing failed: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Fetch error: " + www.error);
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
                var obj = Instantiate(choiceButtonPrefab, choiceContainer);
                TMP_Text textComponent = obj.GetComponentInChildren<TMP_Text>();
                textComponent.text = choice.choice_text;

                Button btn = obj.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    selectedChoiceId = choice.id;
                    nextButton.interactable = true;

                    if (selectedButton != null)
                        SetButtonColor(selectedButton, Color.white);

                    selectedButton = btn;
                    SetButtonColor(selectedButton, Color.cyan);
                });
            }

            nextButton.GetComponentInChildren<TMP_Text>().text = (currentIndex == questions.Count - 1) ? "Submit" : "Next";
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
        form.AddField("quiz_id", 1);
        form.AddField("question_id", q.id);
        form.AddField("choice_id", selectedChoiceId);

        UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:8000/unity/quiz/submit_answer.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            try
            {
                AnswerResponse response = JsonUtility.FromJson<AnswerResponse>(www.downloadHandler.text.Trim());

                if (response.status == "success")
                {
                    correctCount += response.correct;
                    wrongCount += response.wrong;

                    currentIndex++;
                    if (currentIndex >= questions.Count)
                        ShowResults();
                    else
                        ShowQuestion();
                }
                else
                {
                    Debug.LogWarning("Unexpected response: " + www.downloadHandler.text);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JSON parsing failed: " + ex.Message);
            }
        }
        else
        {
            Debug.LogError("Answer submit error: " + www.error);
        }
    }

    void ShowResults()
    {
        questionText.transform.parent.gameObject.SetActive(false);
        resultPanel.SetActive(true);
        correctText.text = $"Correct: {correctCount}";
        wrongText.text = $"Wrong: {wrongCount}";
        percentageText.text = $"Score: {(correctCount * 100 / questions.Count)}%";
    }

    void ClearChoices()
    {
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);
    }
}
