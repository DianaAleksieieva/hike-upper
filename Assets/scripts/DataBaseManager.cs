using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class DataBaseManager : MonoBehaviour
{
    [SerializeField] TMP_InputField Name;
    [SerializeField] TMP_InputField ID;
    // [SerializeField] TMP_Text loadNameText;
    // [SerializeField] TMP_Text loadIDText;
    // [SerializeField] TMP_InputField searchInput;
    
    private string userID;
    private DatabaseReference dataBaseReference;


    void Start()
    {
        // Initialize the Firebase reference
        dataBaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("Firebase Initialized!");
    }


    public void CreateUser() 
    {
        // Create a new User object
        User newUser = new User(Name.text, int.Parse(ID.text));
        string json = JsonUtility.ToJson(newUser);
        Debug.Log(json);

        // Generate a unique key and save the user data under that key
        string key = dataBaseReference.Child("users").Push().Key;
        dataBaseReference.Child("users").Child(key).SetRawJsonValueAsync(json);
    }

    // // Function to load user data based on the provided ID entered in the input field
    // public void LoadUserData()
    // {
    //     string searchID = searchInput.text; // Get the ID entered by the user

    //     // Query to find the user by ID
    //     dataBaseReference.Child("users")
    //         .OrderByChild("id")
    //         .EqualTo(int.Parse(searchID))  // Search by the ID entered
    //         .GetValueAsync().ContinueWithOnMainThread(task =>
    //         {
    //             if (task.IsCompleted && task.Result.Exists)
    //             {
    //                 // Manually iterate through the children to get the first match
    //                 foreach (DataSnapshot userSnapshot in task.Result.Children)
    //                 {
    //                     string name = userSnapshot.Child("name").Value.ToString();
    //                     string id = userSnapshot.Child("id").Value.ToString();

    //                     // Ensure UI update happens on the main thread
    //                     UnityMainThreadDispatcher.Instance().Enqueue(() =>
    //                     {
    //                         loadNameText.text = name;  // Update name on UI
    //                         loadIDText.text = id;      // Update ID on UI
    //                     });

    //                     Debug.Log("User Data Loaded: " + name + ", " + id);
    //                     break; // Exit after the first match
    //                 }
    //             }
    //             else
    //             {
    //                 Debug.LogError("No user found with ID: " + searchID);
    //             }
    //         });
    // }

}
