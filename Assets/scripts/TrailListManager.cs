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
    public Image trailMap;

    private List<Trail> trails;

    void Start()
    {
        LoadTrailsFromDatabase();
        PopulateTrailList();
    }

    void LoadTrailsFromDatabase()
    {
        trails = new List<Trail>
        {
            new Trail("Breakneck Ridge Trail", "3.7 miles, loop", 4.8f, 1200, 41.4487f, -73.9750f),
            new Trail("Bear Mountain Loop", "4.0 miles, loop", 4.7f, 1100, 41.3129f, -73.9887f),
            new Trail("Anthony Nose", "1.9 miles, out & back", 4.6f, 675, 41.3246f, -73.9751f),
            new Trail("Cornish Loop", "4.3 miles, loop", 4.4f, 1050, 41.402326f, -74.047227f),
            new Trail("Overlook Mountain Trail", "4.6 miles, out & back", 4.7f, 1400, 42.0717f, -74.1193f),
            new Trail("Storm King Mountain Trail", "2.4 miles, loop", 4.5f, 1100, 41.4301f, -74.0041f),
            new Trail("Minnewaska State Park Loop", "5.0 miles, loop", 4.8f, 800, 41.7266f, -74.2364f),
            new Trail("Kaaterskill Falls Trail", "2.6 miles, out & back", 4.7f, 600, 42.1931f, -74.047227f),
            new Trail("Mohonk Preserve Trail", "6.4 miles, loop", 4.9f, 1000, 41.7686f, -74.1582f),
            new Trail("Black Rock Forest Trail", "5.6 miles, loop", 4.6f, 1200, 41.4073f, -74.0106f),
            new Trail("Ramapo Valley Reservation", "3.0 miles, loop", 4.7f, 650, 41.0793f, -74.1947f),
            new Trail("Beacon Trail", "4.4 miles, (out & back)", 4.7f, 1600, 41.4934f, -73.9591f)
        };
    }

    void PopulateTrailList()
    {
        foreach (var trail in trails)
        {
            GameObject buttonObj = Instantiate(trailButtonPrefab, contentHolder);

            // Update the button text
            TMP_Text buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
            buttonText.text = trail.Trailname;

            // Add listener to button
            Button button = buttonObj.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
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
        trailMap.sprite = trail.mapImage;
    }
}
