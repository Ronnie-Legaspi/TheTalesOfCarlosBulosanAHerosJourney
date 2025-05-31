using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class LoginManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    public TMP_Text messageText;
    public GameObject messagePanel;
    private CanvasGroup messageCanvasGroup;

    public Button loginButton;
    public Button toRegisterLink;

    private string loginURL = "http://127.0.0.1:8000/unity/login.php";

    void Start()
    {
        loginButton.onClick.AddListener(OnLogin);
        toRegisterLink.onClick.AddListener(SwitchToRegisterPanel);

        messageCanvasGroup = messagePanel.GetComponent<CanvasGroup>();
        if (messageCanvasGroup != null)
        {
            messageCanvasGroup.alpha = 0;
            messagePanel.SetActive(false);
        }
    }

    void SwitchToRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }

    public void OnLogin()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            StartCoroutine(ShowMessage("❌ All fields are required.", false));
            return;
        }

        StartCoroutine(LoginUser(email, password));
    }

    IEnumerator LoginUser(string email, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        UnityWebRequest www = UnityWebRequest.Post(loginURL, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string responseText = www.downloadHandler.text;

            int userId = ExtractUserIdFromResponse(responseText);
            if (userId != -1)
            {
                PlayerPrefs.SetInt("user_id", userId);
                PlayerPrefs.Save();

                yield return StartCoroutine(ShowMessage(" Login successful!", true));
                SceneManager.LoadScene("MainMenuScene");
            }
            else
            {
                yield return StartCoroutine(ShowMessage(" Login failed. Please check your credentials.", false));
            }
        }
        else
        {
            // yield return StartCoroutine(ShowMessage(" Error: " + www.error, false));
            yield return StartCoroutine(ShowMessage("User not found", false));
        }
    }

    int ExtractUserIdFromResponse(string json)
    {
        if (json.Contains("\"id\":"))
        {
            string[] split = json.Split(new[] { "\"id\":" }, System.StringSplitOptions.None);
            string number = new string(split[1].TakeWhile(char.IsDigit).ToArray());
            if (int.TryParse(number, out int id))
                return id;
        }
        return -1;
    }

    IEnumerator ShowMessage(string message, bool success = false)
    {
        messageText.text = message;
        messageText.color = success ? Color.green : Color.red;

        messagePanel.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(messageCanvasGroup, 0f, 1f, 0.3f));
        yield return new WaitForSeconds(1.5f);
        yield return StartCoroutine(FadeCanvasGroup(messageCanvasGroup, 1f, 0f, 0.3f));
        messagePanel.SetActive(false);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = end;
    }
}
