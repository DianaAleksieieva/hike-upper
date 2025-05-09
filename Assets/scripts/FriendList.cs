using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase;
using System.Threading.Tasks;

public class FriendList : MonoBehaviour
{
    public GameObject userFriendPrefab;
    public Transform contentHolder;
    private List<User> loadedFriends = new List<User>();

    private DatabaseReference dbRef;
    private string currentUserId;
    public TMP_InputField emailInputField;
    private ProfileLoader profileLoader;

    void Start()
    {
        profileLoader = FindFirstObjectByType<ProfileLoader>();
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
        currentUserId = profileLoader?.GetUserKey();
        if (!string.IsNullOrEmpty(currentUserId))
        {
            LoadFriendsFromDatabase();
        }
        else
        {
            Debug.LogError("User is not authenticated.");
        }
    }

    public async void LoadFriendsFromDatabase()
    {
        loadedFriends.Clear();
        foreach (Transform child in contentHolder)
        {
            Destroy(child.gameObject); // Clear previous UI elements
        }
        currentUserId = profileLoader?.GetUserKey();
        var friendsRef = dbRef.Child("users").Child(currentUserId).Child("friends");
        var snapshot = await friendsRef.GetValueAsync();

        if (snapshot.Exists)
        {
            Debug.Log($"Friends found for {currentUserId}: {snapshot.ChildrenCount}");
            foreach (var child in snapshot.Children)
            {
                string friendId = child.Key;

                var userSnapshot = await dbRef.Child("users").Child(friendId).GetValueAsync();
                if (userSnapshot.Exists)
                {
                    string name = userSnapshot.Child("name").Value?.ToString() ?? "Unknown";
                    int level = int.TryParse(userSnapshot.Child("level").Value?.ToString(), out int lvl) ? lvl : 0;

                    var user = new User(name, "email", friendId, level, 0, new Friend[0]);
                    loadedFriends.Add(user);

                    GameObject userGO = Instantiate(userFriendPrefab, contentHolder);
                    friendItemUI itemUI = userGO.GetComponent<friendItemUI>();
                    itemUI.SetUserData(user);
                }
            }
        }
        else
        {
            Debug.Log("No friends found.");
        }
    }


    public void AddFriend()
    {
        string friendEmail = emailInputField.text.Trim().ToLower();
        currentUserId = profileLoader.GetUserKey();
        if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(friendEmail))
        {
            Debug.LogError("Invalid current user ID or friend email.");
            return;
        }

        dbRef.Child("users").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || !task.Result.Exists)
            {
                Debug.LogError("❌ Failed to fetch users or none exist.");
                return;
            }

            DataSnapshot usersSnapshot = task.Result;
            string foundUserId = null;

            foreach (var child in usersSnapshot.Children)
            {
                string emailInDb = child.Child("email").Value?.ToString()?.ToLower();
                Debug.Log($"Checking user: {child.Key}, Email: {emailInDb}");
                if (emailInDb != null && emailInDb.Equals(friendEmail, System.StringComparison.OrdinalIgnoreCase))
                {
                    foundUserId = child.Key;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(foundUserId))
            {
                var friendsPath = dbRef.Child("users").Child(currentUserId).Child("friends");

                // Check if the 'friends' node exists
                friendsPath.GetValueAsync().ContinueWithOnMainThread(friendsTask =>
                {
                    if (friendsTask.IsFaulted)
                    {
                        Debug.LogError("❌ Failed to check friends node.");
                        return;
                    }

                    // Optionally create empty 'friends' node if it doesn't exist
                    if (!friendsTask.Result.Exists)
                    {
                        friendsPath.SetValueAsync(new Dictionary<string, object>()).ContinueWithOnMainThread(initTask =>
                        {
                            if (!initTask.IsCompletedSuccessfully)
                            {
                                Debug.LogError("❌ Failed to initialize friends list.");
                                return;
                            }

                            AddFriendToList(friendsPath, foundUserId, friendEmail);
                        });
                    }
                    else
                    {
                        AddFriendToList(friendsPath, foundUserId, friendEmail);
                    }
                });
            }
            else
            {
                Debug.LogWarning($"❌ No user found with email: {friendEmail}");
            }
        });
    }

    private void AddFriendToList(DatabaseReference friendsPath, string foundUserId, string friendEmail)
    {
        friendsPath.Child(foundUserId).SetValueAsync(true).ContinueWithOnMainThread(addTask =>
        {
            if (addTask.IsCompletedSuccessfully)
            {
                Debug.Log($"✅ Friend with email {friendEmail} added (UID: {foundUserId}).");
                LoadFriendsFromDatabase(); // Refresh list
            }
            else
            {
                Debug.LogError("❌ Failed to add friend: " + addTask.Exception?.Flatten().Message);
            }
        });
    }
    public void LoadFriendsOnPageOpen()
    {
        // Ensure the user is authenticated
        string currentUserId = profileLoader.GetUserKey();
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogError("User is not authenticated.");
            return;
        }

        // Load the friends when the page is opened
        LoadFriendsFromDatabase();
    }
}
