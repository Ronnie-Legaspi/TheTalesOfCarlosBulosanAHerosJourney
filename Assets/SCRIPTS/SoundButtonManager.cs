using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SoundButtonManager : MonoBehaviour
{
    [Header("Buttons that should play the click sound")]
    public List<GameObject> clickableObjects;

    [Header("Click Sound")]
    public AudioClip clickSound;
    public AudioSource audioSource;

    private void Start()
    {
        foreach (GameObject obj in clickableObjects)
        {
            if (obj != null)
            {
                Button button = obj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => PlayClickSound());
                }
                else
                {
                    Debug.LogWarning($"{obj.name} does not have a Button component!");
                }
            }
        }
    }

    void PlayClickSound()
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
