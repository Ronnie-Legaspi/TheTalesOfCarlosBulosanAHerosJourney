using TMPro;
using UnityEngine;

public class UserTrophyCardUI : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text usernameText;
    public TMP_Text emailText;
    public TMP_Text courseText;
    public TMP_Text trophyText;

    public void Setup(int rank, string username, string email, string course, int trophy)
    {
        rankText.text = rank.ToString(); // e.g. "1", "2", etc.
        usernameText.text = username;
        emailText.text = email;
        courseText.text = course;
        trophyText.text = trophy.ToString();
    }
}
