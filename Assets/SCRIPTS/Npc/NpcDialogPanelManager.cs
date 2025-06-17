using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using System.IO;

public class NpcDialogPanelManager : MonoBehaviour
{
    public static NpcDialogPanelManager Instance;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text dialogueText;
    public Image npcImage;
    public Button nextButton;
    public TMP_Text nextButtonText;
    public TMP_Text npcNameText; // <-- Added for displaying NPC name

    [Header("Typing")]
    [Tooltip("Speed of text appearance. Lower is faster.")]
    [Min(0f)]
    public float typewriterSpeed = 0.005f;

    private NpcDialogueData[] dialogues;
    private int currentIndex = 0;
    private bool showingPlayerResponse = false;
    private string playerName;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        Instance = this;
        panel.SetActive(false);
        nextButton.onClick.AddListener(HandleNextClick);
        playerName = PlayerPrefs.GetString("username", "You");
    }

    public void OpenPanel(int npcId)
    {
        panel.SetActive(true);
        StartCoroutine(FetchDialogues(npcId));
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        currentIndex = 0;
        dialogues = null;
        npcNameText.text = ""; // Clear NPC name when closed
    }

    IEnumerator FetchDialogues(int npcId)
    {
        string url = "http://127.0.0.1:8000/unity/npcs/dialogues.php?npc_id=" + npcId;
        UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Fetch failed: " + request.error);
        }
        else
        {
            string wrappedJson = "{\"dialogues\":" + request.downloadHandler.text + "}";
            DialogueWrapper wrapper = JsonUtility.FromJson<DialogueWrapper>(wrappedJson);
            dialogues = wrapper.dialogues;
            currentIndex = 0;
            showingPlayerResponse = false;
            ShowDialogueOrResponse(false);
        }
    }

    void HandleNextClick()
    {
        if (typingCoroutine != null) return;

        if (dialogues == null || currentIndex >= dialogues.Length)
        {
            ClosePanel();
            return;
        }

        if (!showingPlayerResponse)
        {
            ShowDialogueOrResponse(true);
        }
        else
        {
            int nextId = dialogues[currentIndex].next_dialogue_id;
            if (nextId == 0)
            {
                nextButtonText.text = "Close";
                nextButton.onClick.RemoveAllListeners();
                nextButton.onClick.AddListener(ClosePanel);
            }
            else
            {
                currentIndex = FindNextIndex(nextId);
                showingPlayerResponse = false;
                ShowDialogueOrResponse(false);
            }
        }
    }

    void ShowDialogueOrResponse(bool showPlayer)
    {
        if (currentIndex >= dialogues.Length) return;

        NpcDialogueData d = dialogues[currentIndex];

        string line = showPlayer
            ? $"{playerName}: {d.player_response}"
            : $"{d.npc_name}: {d.dialogue}";

        dialogueText.color = showPlayer ? new Color(0f, 0.5f, 0f) : Color.black;

        if (!showPlayer)
        {
            StartCoroutine(LoadAndDisplayImage(d.npc_image_url));
            npcNameText.text = d.npc_name; // <-- Display NPC name when NPC is speaking
        }

        nextButton.interactable = false;
        typingCoroutine = StartCoroutine(TypeWriterEffect(line));

        showingPlayerResponse = showPlayer;
        nextButtonText.text = d.next_dialogue_id == 0 && showPlayer ? "Close" : "Next";
    }

    IEnumerator TypeWriterEffect(string fullText)
    {
        dialogueText.text = "";

        if (typewriterSpeed <= 0f)
        {
            dialogueText.text = fullText;
        }
        else
        {
            foreach (char c in fullText)
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(typewriterSpeed);
            }
        }

        nextButton.interactable = true;
        typingCoroutine = null;
    }

    IEnumerator LoadAndDisplayImage(string url)
    {
        string fileName = Path.GetFileName(new System.Uri(url).LocalPath);
        string localPath = Path.Combine(Application.persistentDataPath, "npcs");

        if (!Directory.Exists(localPath))
            Directory.CreateDirectory(localPath);

        string filePath = Path.Combine(localPath, fileName);

        if (!File.Exists(filePath))
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Failed to download image: " + www.error);
                yield break;
            }

            Texture2D tex = DownloadHandlerTexture.GetContent(www);
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);
        }

        byte[] imgBytes = File.ReadAllBytes(filePath);
        Texture2D loadedTex = new Texture2D(2, 2);
        loadedTex.LoadImage(imgBytes);
        npcImage.sprite = Sprite.Create(loadedTex, new Rect(0, 0, loadedTex.width, loadedTex.height), new Vector2(0.5f, 0.5f));
    }

    int FindNextIndex(int nextId)
    {
        for (int i = 0; i < dialogues.Length; i++)
        {
            if (dialogues[i].id == nextId)
                return i;
        }
        return dialogues.Length;
    }

    [System.Serializable]
    public class NpcDialogueData
    {
        public int id;
        public int npc_id;
        public string npc_name;
        public string npc_image_url;
        public string dialogue;
        public string player_response;
        public bool is_question;
        public bool is_answer;
        public int next_dialogue_id;
    }

    [System.Serializable]
    public class DialogueWrapper
    {
        public NpcDialogueData[] dialogues;
    }
}
