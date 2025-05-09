using Firebase.Database;
using Firebase.Extensions;
using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class DataBaseManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField Name;
    [SerializeField] private TMP_InputField Email;
    [SerializeField] private TMP_InputField Password;
    [SerializeField] private Button SignUpButton;

    private DatabaseReference dbRef;
    private FirebaseAuth auth;

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

    }



    public void CreateUser()
    {
        string name = Name.text.Trim();
        string email = Email.text.Trim();
        string password = Password.text;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("⚠️ Please fill in all fields.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(authTask =>
        {
            if (authTask.IsCanceled)
            {
                Debug.LogError("🚫 CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (authTask.IsFaulted)
            {
                if (authTask.Exception != null)
                {
                    foreach (var ex in authTask.Exception.Flatten().InnerExceptions)
                    {
                        if (ex is FirebaseException firebaseEx)
                            Debug.LogError($"❌ Firebase error: Code={firebaseEx.ErrorCode}, Message={firebaseEx.Message}");
                        else
                            Debug.LogError("❌ Non-Firebase error: " + ex.Message);
                    }
                }
                return;
            }

            // ✅ Success
            FirebaseUser newUserAuth = authTask.Result.User;
            Debug.LogFormat("✅ Firebase user created successfully: {0} ({1})", newUserAuth.Email, newUserAuth.UserId);

            // Prepare user profile
            User newUser = new User(name, email, password, level: 0, xp: 0, friends: new Friend[0]);
            string json = JsonUtility.ToJson(newUser);

            dbRef.Child("users").Child(newUserAuth.UserId)
                .SetRawJsonValueAsync(json).ContinueWithOnMainThread(dbTask =>
            {
                if (dbTask.IsCompletedSuccessfully)
                {
                    Debug.Log("✅ User profile successfully saved to database!");
                }
                else
                {
                    Debug.LogError("❌ Failed to save user profile to database: " + dbTask.Exception?.Message);
                }
            });
        });
    }

}