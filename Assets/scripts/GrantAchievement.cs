using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Storage;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Database;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class GrantAchievement : MonoBehaviour
{
    private AchievementManager achievementManager;
    private DatabaseReference dbRef;

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("Firebase ready!");
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
        achievementManager = FindFirstObjectByType<AchievementManager>();
    }

    // Example of calling the CheckAndGrantBasedOnMilestones function from AchievementManager
    public async Task GrantUserAchievement(string userId)
    {
        try
        {
            // Fetch user data from Firebase (using async/await)
            var snapshot = await dbRef.Child("users").Child(userId).GetValueAsync();

            if (snapshot.Exists)
            {
                if (achievementManager != null)
                {
                    // Proceed to check and grant achievements if the user data is fetched
                    await achievementManager.CheckAndGrantBasedOnMilestones(userId);
    
                }
                else
                {
                    Debug.LogError("AchievementManager not found.");
                }
            }
            else
            {
                Debug.LogError($"User {userId} does not exist in the database.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error fetching user data: {ex.Message}");
        }
    }

}
