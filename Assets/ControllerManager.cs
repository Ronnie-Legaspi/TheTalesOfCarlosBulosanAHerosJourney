using System.Collections;
using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    public GameObject DescriptionPanel;
    public GameObject PlayerControlllerPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(CheckActiveControllers());
    }

    // Update is called once per frame
    IEnumerator CheckActiveControllers()
    {
        if (DescriptionPanel.activeSelf)
        {
            PlayerControlllerPanel.SetActive(false);
        }
        else
        {
            PlayerControlllerPanel.SetActive(true);
        }
        yield return null;
    }
}
