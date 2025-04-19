using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrailButton : MonoBehaviour
{
    public TMP_Text labelText; // assign in inspector (inside prefab)

    private Trail trailData;
    private System.Action<Trail> onClicked;

    public void Setup(Trail trail, System.Action<Trail> clickCallback)
    {
        trailData = trail;
        onClicked = clickCallback;
        labelText.text = trail.Trailname;

        GetComponent<Button>().onClick.AddListener(() => onClicked?.Invoke(trailData));
    }
}
