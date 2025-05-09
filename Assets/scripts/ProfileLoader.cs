using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


public class ProfileLoader : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI emailText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI pointsText;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    private FirebaseAuth auth;
    private FirebaseUser currentUser;
    private AchievementManager achievementManager;
    private DatabaseReference dbRef;
    public string UserId { get; private set; }
    public event Action<User> OnUserLoaded;
    [SerializeField] Slider progressBar;
    [SerializeField] TMP_Text scoreText;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
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

    public void LogInUser()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("🚫 Email or Password is empty!");
            return;
        }

        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("🚫 SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("❌ SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception?.Flatten().InnerException?.Message);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            currentUser = result.User;

            Debug.LogFormat("✅ User signed in successfully: {0} ({1})", currentUser.Email, currentUser.UserId);

            LoadUserProfile();
        });
        emailInput.text = "";
        passwordInput.text = "";
    }

    public string GetUserKey()
    {
        return currentUser != null ? currentUser.UserId : string.Empty;
    }


    public void LoadUserProfile()
    {
        if (currentUser == null)
        {
            Debug.LogError("🚫 Cannot load profile. User not logged in.");
            return;
        }

        string userId = currentUser.UserId;

        dbRef.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Error fetching user data: " + task.Exception);
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                
                string name = snapshot.Child("name").Value?.ToString() ?? "Unknown";
                string email = snapshot.Child("email").Value?.ToString() ?? "No email";
                string password = snapshot.Child("password").Value?.ToString() ?? ""; // optional fallback
                int level = int.Parse(snapshot.Child("level").Value?.ToString() ?? "1");
                int xp = int.Parse(snapshot.Child("xp").Value?.ToString() ?? "0");
                progressBar.maxValue = 100;
                int xpInt = (int)xp; 
                progressBar.value = xpInt;
                Debug.Log($"Loaded: {name}, {email}, Level: {level}");

                // Update UI
                nameText.text = name;
                emailText.text = email;
                levelText.text = "Level: " + level;
                pointsText.text = "Points: " + xp + "/100";
                

                // Notify listeners
                User loadedUser = new User(name, email, password, level, xp, new Friend[0]);
                OnUserLoaded?.Invoke(loadedUser);

                //loading achivements
                List<Achievement> achievements = new List<Achievement>();
                var achievementNodes = snapshot.Child("achievements").Children;

                Debug.Log($"Found {achievementNodes.Count()} achievement nodes.");

                foreach (var node in achievementNodes)
                {
                    // Debug what we actually have
                    Debug.Log($"Raw node: key={node.Key}, value={node.Value}, hasCaptionChild={node.HasChild("caption")}");

                    string caption;

                    if (node.HasChild("caption"))
                    {
                        // nested object { caption: "Eco Enthusiast", ... }
                        caption = node.Child("caption").Value?.ToString();
                    }
                    else
                    {
                        // simple string entry "eco_enthusiast"
                        caption = node.Value?.ToString();
                    }

                    if (!string.IsNullOrEmpty(caption))
                    {
                        achievements.Add(new Achievement(caption));
                        Debug.Log($" → queued caption: {caption}");
                    }
                    else
                    {
                        Debug.LogWarning("Skipping empty achievement node");
                    }
                }
                achievementManager?.LoadUserAchievements(achievements);
            }
            
        });
    }
    public async Task AddXP(int xpToAdd)
    {
        if (currentUser == null)
        {
            Debug.LogError("🚫 Cannot add XP. User not logged in.");
            return;
        }

        string userId = currentUser.UserId;

        try
        {
            DataSnapshot snapshot = await dbRef.Child("users").Child(userId).GetValueAsync();

            if (snapshot == null || !snapshot.Exists)
            {
                Debug.LogError("User data not found.");
                return;
            }

            int currentXP = int.Parse(snapshot.Child("xp").Value?.ToString() ?? "0");
            int currentLevel = int.Parse(snapshot.Child("level").Value?.ToString() ?? "0");
            scoreText.text = currentLevel.ToString();
            progressBar.value = currentXP;

            currentXP += xpToAdd;

            if (currentXP >= 100)
            {
                currentLevel += 1;
                currentXP = 0;
                Debug.Log("🎉 Level Up! New Level: " + currentLevel);
            }

            await dbRef.Child("users").Child(userId).Child("xp").SetValueAsync(currentXP);
            await dbRef.Child("users").Child(userId).Child("level").SetValueAsync(currentLevel);

            Debug.Log($"✅ XP updated: {currentXP}, Level: {currentLevel}");
            LoadUserProfile();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Failed to update XP or level: {ex.Message}");
        }

    }
}
