using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System;
public class ReportIssue : MonoBehaviour
{
    public GameObject FeedbackPanel;
    public TMP_InputField reportInput;
    private DatabaseReference dbReference;
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });
    }

    public void onReportIssueClick()
    {
        FeedbackPanel.SetActive(true);
        Invoke(nameof(HideFeedbackPanel), 2f);
        
    }

    private void HideFeedbackPanel()
    {
        FeedbackPanel.SetActive(false);
    }
    public void SubmitProblem()
    {
        string problemText = reportInput.text.Trim();

        if (string.IsNullOrEmpty(problemText))
        {
            Debug.LogWarning("Problem report is empty!");
            return;
        }

        string problemID = dbReference.Child("problemReports").Push().Key;
        ProblemReport report = new ProblemReport(problemText, DateTime.UtcNow.ToString("s"));

        string json = JsonUtility.ToJson(report);
        dbReference.Child("problemReports").Child(problemID).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Problem report submitted successfully.");
                reportInput.text = ""; 
            }
            else
            {
                Debug.LogError("Error submitting problem: " + task.Exception);
            }
        });
    }
}

[Serializable]
public class ProblemReport
{
    public string message;
    public string timestamp;

    public ProblemReport(string message, string timestamp)
    {
        this.message = message;
        this.timestamp = timestamp;
    }
}

