using System;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Extensions;
using Firebase.Storage;

public class PhotoUploader : MonoBehaviour
{
    private FirebaseStorage storage;
    private StorageReference storageRef;

    void Start()
{

    storage = FirebaseStorage.DefaultInstance;
     storageRef = storage.GetReferenceFromUrl("gs://hike-upper.firebasestorage.app");
    if (storage == null)
    {
        Debug.LogError("Failed to initialize Firebase Storage.");
        return;
    }

    storageRef = storage.GetReferenceFromUrl("gs://hike-upper.firebasestorage.app"); // <-- change this

    if (storageRef == null)
    {
        Debug.LogError("Failed to initialize Firebase Storage reference.");
    }
}


    public void UploadPhoto(Texture2D photo)
{
    if (photo == null)
    {
        Debug.LogError("The photo to upload is null.");
        return;
    }

    byte[] imageBytes = photo.EncodeToJPG(75);
    string fileName = $"photo_{DateTime.Now.Ticks}.jpg";
    Debug.Log("Uploading: " + fileName);

    StorageReference photoRef = storageRef.Child("photos/" + fileName);

    photoRef.PutBytesAsync(imageBytes).ContinueWithOnMainThread(task =>
    {
        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError("Upload failed: " + task.Exception);
        }
        else
        {
            Debug.Log("Photo uploaded successfully!");
            photoRef.GetDownloadUrlAsync().ContinueWithOnMainThread(urlTask =>
            {
                if (urlTask.IsCompleted)
                {
                    string downloadUrl = urlTask.Result.ToString();
                    Debug.Log("Download URL: " + downloadUrl);
                }
            });
        }
    });
}

}
