using UnityEngine;

public class PlatformSwitcher : MonoBehaviour
{
    public MonoBehaviour pcController;
    public MonoBehaviour mobileController;

    void Start()
    {
#if UNITY_STANDALONE || UNITY_WEBGL || UNITY_EDITOR
        EnableController(pcController, mobileController);
#elif UNITY_IOS || UNITY_ANDROID
        EnableController(mobileController, pcController);
#else
        Debug.LogWarning("Unknown platform - defaulting to PC controller.");
        EnableController(pcController, mobileController);
#endif
    }

    void EnableController(MonoBehaviour toEnable, MonoBehaviour toDisable)
    {
        if (toEnable != null) toEnable.enabled = true;
        if (toDisable != null) toDisable.enabled = false;
    }
}
