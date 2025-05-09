using TMPro;
using UnityEngine;

public class friendItemUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text levelText;

    public void SetUserData(User user)
    {
        nameText.text = user.name;
        levelText.text = "Lvl " + user.level.ToString();
    }
}
