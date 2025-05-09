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
    public GameObject photoAddedPanel;

    void Start()
    {

        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://hike-upper.firebasestorage.app");
        if (storage == null)
        {
            Debug.LogError("Failed to initialize Firebase Storage.");
            return;
        }
    }


    public async Task UploadPhoto(Texture2D photo)
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

        try
        {
            // Await the upload process
            await photoRef.PutBytesAsync(imageBytes);
            Debug.Log("Photo uploaded successfully!");

            // After the photo is uploaded, get the download URL
            Uri downloadUrl = await photoRef.GetDownloadUrlAsync();
            Debug.Log("Download URL: " + downloadUrl);
            
            // Show the panel when the upload is complete
            photoAddedPanel.SetActive(true);
        }
        catch (Exception ex)
        {
            Debug.LogError("Upload failed: " + ex.Message);
        }
    }
    public void closePanel() {
        photoAddedPanel.SetActive(false);
    }
}
