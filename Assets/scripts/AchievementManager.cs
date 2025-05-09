using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Storage;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Database;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Linq;


public class AchievementManager : MonoBehaviour
{
    public GameObject achievementItemPrefab;
    public Transform contentPanel;

    private FirebaseStorage storage;
    private StorageReference storageRef;
    private const long MaxDownloadSizeBytes = 1 * 1024 * 1024; // 1MB
    private DatabaseReference dbRef;

    public TextMeshProUGUI achievementText;
    void Start()
    {
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.RootReference;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void LoadUserAchievements(List<Achievement> achievements)
    {
        if (achievements == null || achievements.Count == 0)
        {
            Debug.LogWarning("No achievements to display.");
            return;
        }

        // Clear old items to avoid duplication
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Instantiate fresh items
        foreach (var achievement in achievements)
        {
            GameObject item = Instantiate(achievementItemPrefab, contentPanel);
            TextMeshProUGUI captionText = item.transform.Find("Caption")?.GetComponent<TextMeshProUGUI>();
            Image imageComponent = item.transform.Find("Image")?.GetComponent<Image>();

            if (captionText != null)
            {
                string caption = achievement.Caption.Replace("_", " ");
                captionText.text = caption;
            }

            if (imageComponent != null)
                StartCoroutine(LoadImageFromStorage(achievement.Caption, imageComponent));
        }
    }

    private IEnumerator LoadImageFromStorage(string caption, Image imageComponent)
    {
        // Sanitize caption to filename (e.g. spaces → underscores)
        string fileName = caption.Replace(" ", "_") + ".png";
        string storagePath = $"achievements/{fileName}";
        StorageReference imgRef = storageRef.Child(storagePath);

        byte[] fileData = null;
        var downloadTask = imgRef.GetBytesAsync(MaxDownloadSizeBytes)
            .ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    Debug.LogError($"Failed to download {storagePath}: {t.Exception}");
                }
                else
                {
                    fileData = t.Result;
                }
            });

        // Wait until download completes
        yield return new WaitUntil(() => downloadTask.IsCompleted);

        if (fileData != null)
        {
            // Create a texture and sprite
            var tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            var sprite = Sprite.Create(tex,
                                      new Rect(0, 0, tex.width, tex.height),
                                      new Vector2(0.5f, 0.5f));
            imageComponent.sprite = sprite;
        }
    }

    private readonly Dictionary<string, Func<int, int, bool>> _milestoneChecks =
        new Dictionary<string, Func<int, int, bool>>()
    {
        // caption => (totalXP, level) => qualify?
        { "Eco_Enthusiast",    (xp, lvl) => xp >= 10 },
        { "Hiker",             (xp, lvl) => xp >= 50 },
        { "Zero_Waste",        (xp, lvl) => xp >= 100 || lvl >= 2 },
        { "On_Top",            (xp, lvl) => lvl >= 4 },
        { "Planet_Protector",  (xp, lvl) => xp >= 250 },
        { "Volunteer_Silver",  (xp, lvl) => xp >= 500 },
    };

    public async Task CheckAndGrantBasedOnMilestones(string userId)
    {
        
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("📴 No internet connection. Cannot grant achievements.");
            return;
        }

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        try
        {
            Debug.Log($"📄 Fetching user data for: {userId}...");
            // Fetch user data
            DataSnapshot snapshot = await dbRef.Child("users").Child(userId).GetValueAsync();

            if (snapshot == null || !snapshot.Exists)
            {
                Debug.LogError($"❌ User {userId} not found in Firebase.");
                return;
            }

            // Retrieve XP and Level from the snapshot
            int totalXP = int.Parse(snapshot.Child("xp").Value?.ToString() ?? "0");
            int level = int.Parse(snapshot.Child("level").Value?.ToString() ?? "0");

            Debug.Log($"📊 XP: {totalXP}, Level: {level}");

            // Fetch existing achievements from Firebase
            var existingMap = snapshot.Child("achievements").ChildrenCount > 0
                ? snapshot.Child("achievements")
                : null;

            // Parse existing achievements
            HashSet<string> earnedCaptions = new HashSet<string>();
            if (existingMap != null)
            {
                foreach (var entry in existingMap.Children)
                {
                    if (entry.HasChild("caption"))
                    {
                        string caption = entry.Child("caption").Value?.ToString();
                        if (!string.IsNullOrEmpty(caption))
                        {
                            earnedCaptions.Add(caption);
                        }
                    }
                }
            }

            Debug.Log($"📜 Already earned: {string.Join(", ", earnedCaptions)}");

            // Check milestones and determine which achievements to grant
            List<string> toGrant = _milestoneChecks
                .Where(kv => kv.Value(totalXP, level) && !earnedCaptions.Contains(kv.Key))
                .Select(kv => kv.Key)
                .ToList();

            if (toGrant.Count == 0)
            {
                Debug.Log("✅ No new achievements to grant.");
                return;
            }

            Debug.Log($"🎯 New achievements to grant: {string.Join(", ", toGrant)}");
            // Check if achievements field exists
            var achievementsNode = snapshot.HasChild("achievements")
                ? snapshot.Child("achievements")
                : null;

            // If not exist, create empty achievements object in DB
            if (achievementsNode == null)
            {
                Debug.Log("📁 'achievements' field not found. Initializing...");
                await dbRef.Child("users").Child(userId).Child("achievements").SetRawJsonValueAsync("{}");

                // Refresh snapshot and reassign
                snapshot = await dbRef.Child("users").Child(userId).GetValueAsync();
                achievementsNode = snapshot.Child("achievements");
            }
            // Prepare updates
            int nextIndex = (int)(existingMap?.ChildrenCount ?? 0) + 1;
            var updates = new Dictionary<string, object>();

            foreach (var caption in toGrant)
            {
                updates[$"achievements/{nextIndex}"] = new Dictionary<string, object>
            {
                { "caption", caption }
            };
                Debug.Log($"📝 Prepared update for: {caption} at achievements/{nextIndex}");
                nextIndex++;
            }


            // Perform the update
            Debug.Log("📤 Updating Firebase with new achievements...");
            await dbRef.Child("users").Child(userId).UpdateChildrenAsync(updates);
            achievementText.text = "Granted achievement: ";
            foreach (var c in toGrant)
            {
                string displayName = c.Replace("_", " ");
                Debug.Log($"🏆 Granted achievement: {c}");
                achievementText.text += $"{c} \n";
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"⚠️ Error while granting achievements for {userId}: {ex.Message}");
            if (ex.Message.Contains("offline"))
            {
                Debug.LogWarning("📡 Firebase client appears to be offline. Retry later.");
            }
        }
    }


}
