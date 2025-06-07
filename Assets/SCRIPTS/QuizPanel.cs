using UnityEngine;

public class QuizPanel : MonoBehaviour
{
    [Tooltip("Drag the corresponding QuizManager that controls this panel")]
    public QuizManager quizManager;

    void Start()
    {
        if (quizManager == null)
        {
            Debug.LogError($"QuizManager not assigned to {gameObject.name}. Make sure to assign the correct manager in the Inspector.");
        }
    }

    // Optional: expose manager logic to this panel
    public void ShowQuiz()
    {
        gameObject.SetActive(true);
        quizManager.gameObject.SetActive(true); // Optional, if you hide the manager object
    }

    public void HideQuiz()
    {
        gameObject.SetActive(false);
        quizManager.gameObject.SetActive(false); // Optional
    }
}
