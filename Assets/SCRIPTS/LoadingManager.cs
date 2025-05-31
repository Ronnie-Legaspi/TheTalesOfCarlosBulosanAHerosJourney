using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;          // Make sure to include this for TMP_Text
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public Slider progressBar;     // Assign your UI Slider in inspector
    public TMP_Text progressText;  // Assign your TMP Text in inspector

    void Start()
    {
        if (string.IsNullOrEmpty(SceneLoader.TargetScene))
        {
            Debug.LogError("TargetScene is null or empty! Cannot load scene.");
            return;
        }

        StartCoroutine(LoadTargetSceneAsync());
    }

    IEnumerator LoadTargetSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneLoader.TargetScene);

        // Optionally prevent auto switching while you want to show loading
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            // operation.progress is 0 to 0.9 when loading, then 0.9 to 1 when activated
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = (progress * 100f).ToString("F0") + "%";

            // When loading is complete (progress >= 0.9f), allow scene activation to finish load
            if (operation.progress >= 0.9f)
            {
                // Show 100% progress
                if (progressBar != null)
                    progressBar.value = 1f;
                if (progressText != null)
                    progressText.text = "100%";

                // Activate the scene
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
