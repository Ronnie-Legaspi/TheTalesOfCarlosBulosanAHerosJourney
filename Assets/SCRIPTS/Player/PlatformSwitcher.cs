using UnityEngine;

public class PlatformSwitcher : MonoBehaviour
{
    public PlayerController pcController;                  // For PC/WebGL
    public MobilePlayerController mobileController;        // For Android/iOS

    void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (pcController != null) pcController.enabled = false;
        if (mobileController != null) mobileController.enabled = true;
        Debug.Log("Mobile controller enabled");
#else
        if (pcController != null) pcController.enabled = true;
        if (mobileController != null) mobileController.enabled = false;
        Debug.Log("PC controller enabled");
#endif
    }
}
