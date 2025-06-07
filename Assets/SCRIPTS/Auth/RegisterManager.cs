using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class RegisterManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject registerPanel;
    public GameObject loginPanel;

    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Dropdown departmentDropdown;

    public TMP_Text messageText;
    public GameObject messagePanel;
    private CanvasGroup messageCanvasGroup;

    public Button registerButton;
    public Button toLoginLink;

    string baseURL = "http://127.0.0.1:8000/unity/register.php";

    void Start()
    {
        SetupDepartmentDropdown();
        registerButton.onClick.AddListener(OnRegister);
        toLoginLink.onClick.AddListener(SwitchToLoginPanel);

        messageCanvasGroup = messagePanel.GetComponent<CanvasGroup>();
        if (messageCanvasGroup != null)
        {
            messageCanvasGroup.alpha = 0;
            messagePanel.SetActive(false);
        }
    }

    void SetupDepartmentDropdown()
    {
        departmentDropdown.ClearOptions();
        List<string> departments = new List<string> {
            "BSIT", "BSCRIM", "BSED", "BSHM", "BSMIDWIFERY", "BSA", "BSCE", "BSBA"
        };
        departmentDropdown.AddOptions(departments);
    }

    void SwitchToLoginPanel()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void OnRegister()
    {
        string username = usernameInput.text.Trim();
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string department = departmentDropdown.options[departmentDropdown.value].text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            StartCoroutine(ShowMessage("All fields are required.", false));
            return;
        }

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            StartCoroutine(ShowMessage("Enter a valid email address.", false));
            return;
        }

        StartCoroutine(RegisterUser(username, email, password, department));
    }

    IEnumerator RegisterUser(string username, string email, string password, string department)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", username);
        form.AddField("email", email);
        form.AddField("password", password);
        form.AddField("department", department);

        UnityWebRequest www = UnityWebRequest.Post(baseURL, form);
        yield return www.SendWebRequest();

        string responseText = www.downloadHandler.text;

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            string errorMsg = !string.IsNullOrEmpty(responseText) ? responseText : www.error;
            StartCoroutine(ShowMessage("Error: " + errorMsg, false));
        }
        else
        {
            if (www.responseCode == 200 && responseText.Contains("\"id\":"))
            {
                int userId = ExtractUserIdFromResponse(responseText);
                PlayerPrefs.SetInt("user_id", userId);
                PlayerPrefs.Save();

                yield return RegisterAchievement(userId, 1);
                yield return StartCoroutine(ShowMessage("Registered successfully!", true));
                yield return new WaitForSeconds(1f);
                // ✅ Set the target scene before loading the loading screen
                SceneLoader.TargetScene = "MainMenuScene";
                SceneManager.LoadScene("LoadingScene");
            }
            else
            {
                StartCoroutine(ShowMessage(responseText, false));
            }
        }
    }

    int ExtractUserIdFromResponse(string json)
    {
        if (json.Contains("\"id\":"))
        {
            string[] split = json.Split(new[] { "\"id\":" }, System.StringSplitOptions.None);
            string number = new string(split[1].TakeWhile(char.IsDigit).ToArray());
            int.TryParse(number, out int id);
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

    IEnumerator RegisterAchievement(int userId, int achievementId)
    {
        WWWForm form = new WWWForm();
        form.AddField("user_id", userId);
        form.AddField("achievement_id", achievementId);

        string url = "http://127.0.0.1:8000/unity/achievements/register_success.php";

        UnityWebRequest request = UnityWebRequest.Post(url, form);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to register achievementL " + request.error);

        }
        else
        {
            Debug.Log("Achievement registered successfully for user ID: " + userId);
        }


    }
}
