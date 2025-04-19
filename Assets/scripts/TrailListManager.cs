using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrailListManager : MonoBehaviour
{
    public GameObject trailButtonPrefab;
    public Transform contentHolder; 
    public GameObject itemPanel;

    public TMP_Text trailName;
    public TMP_Text distance;
    public TMP_Text time;
    public TMP_Text elevation;
    public TMP_Text lat;
    public TMP_Text lon;

    private List<Trail> trails;

    void Start()
    {
        LoadTrailsFromDatabase();
        PopulateTrailList();
         Debug.Log(trails[0].Trailname); 
    }

    void LoadTrailsFromDatabase()
    {
        trails = new List<Trail>
        {
            new Trail("Breakneck Trail", "3.7 miles, loop", 4.0f, 1200, 48.8566f, 2.3522f),
            new Trail("Forest Path", "some", 4.0f, 2200, 48.1234f, 2.1234f),
        };
    }

    void PopulateTrailList()
    {
        Debug.Log("Populating Trail List");
        foreach (var trail in trails)
        {
            GameObject buttonObj = Instantiate(trailButtonPrefab, contentHolder);

            // Update the button text
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            buttonText.text = trail.Trailname;

            // Debug to check if both trails are being processed
            Debug.Log("Creating button for trail: " + trail.Trailname);

            // Add listener to button
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                Debug.Log("Button clicked! Showing info for " + trail.Trailname);
                ShowTrailInfo(trail);
            });
        }
    }


    void ShowTrailInfo(Trail trail)
    {
        itemPanel.SetActive(true);
        trailName.text = trail.Trailname;
        distance.text = trail.distance;
        time.text = $"{trail.time} h";
        elevation.text = $"{trail.elevation} ft";
        lat.text = trail.location.latitude.ToString();
        lon.text = trail.location.longitude.ToString();
    }
}
