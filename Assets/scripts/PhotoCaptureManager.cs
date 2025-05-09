using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Firebase.Database;
using Firebase.Extensions;
using System.Threading.Tasks;
using System;

public class PhotoCaptureManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject HomePage;
    public GameObject TakePhotoPage;

    [Header("Camera Preview")]
    public RawImage cameraPreview;
    public AspectRatioFitter aspectFitter;

    private WebCamTexture webcamTexture;
    public Quaternion baseRotation;

    public PhotoUploader photoUploader;
    private ProfileLoader profileLoader;
    public GrantAchievement grantAchievement;
    private DatabaseReference dbRef;

    private string userKey;


    void Start()
    {
        // scoreManager = FindFirstObjectByType<ScoreManager>();
        profileLoader = FindFirstObjectByType<ProfileLoader>();
        grantAchievement = FindFirstObjectByType<GrantAchievement>();

    }


    public void OpenCameraPanel()
    {
        HomePage.SetActive(false);
        TakePhotoPage.SetActive(true);
        StartCamera();
    }

    public void BackToMenu()
    {
        StopCamera();
        TakePhotoPage.SetActive(false);
        HomePage.SetActive(true);
    }


    void StartCamera()
    {
        if (photoUploader == null)
        {
            photoUploader = UnityEngine.Object.FindAnyObjectByType<PhotoUploader>();
        }
        else
        {
            Debug.Log("PhotoUploader found: " + photoUploader);
        }

        if (webcamTexture == null)
        {
            WebCamDevice[] devices = WebCamTexture.devices;

            if (devices.Length == 0)
            {
                Debug.LogError("No camera devices found.");
                return;
            }

            // Pick BACK camera if available
            string backCameraName = null;
            foreach (var device in devices)
            {
                if (!device.isFrontFacing)
                {
                    backCameraName = device.name;
                    break;
                }
            }

            if (backCameraName == null)
            {
                // fallback if no back camera
                backCameraName = devices[0].name;
            }

            Debug.Log("Using camera: " + backCameraName);

            webcamTexture = new WebCamTexture(backCameraName, 1080, 1920); // Force portrait resolution
            cameraPreview.texture = webcamTexture;
        }

        webcamTexture.Play();
        StartCoroutine(AdjustPreview());
    }
    IEnumerator AdjustPreview()
    {
        // Wait for at least 1 frame to ensure width/height are available
        yield return new WaitUntil(() => webcamTexture.width > 100);

        float ratio = (float)webcamTexture.width / webcamTexture.height;
        aspectFitter.aspectRatio = ratio;

        // Get the angle of rotation from the webcam
        float rotationAngle = webcamTexture.videoRotationAngle;

        // Rotate the RawImage based on the video rotation angle
        cameraPreview.rectTransform.rotation = Quaternion.Euler(0, 0, -rotationAngle); // Adjust for correct orientation

        // Handle front-facing camera mirroring
        bool isFrontFacing = webcamTexture.videoVerticallyMirrored;
        cameraPreview.rectTransform.localScale = new Vector3(1, isFrontFacing ? -1 : 1, 1);
    }

    void StopCamera()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
            webcamTexture.Stop();
    }
    public async Task OnPhotoCaptured()
    {
        await profileLoader.AddXP(10);
        await Task.Delay(500);
        userKey = profileLoader.GetUserKey();
        if (grantAchievement == null)
            Debug.LogError("❌ GrantAchievement not found in scene!");
        else
        {
            await grantAchievement.GrantUserAchievement(userKey);
        }
    }
    public async void CapturePhoto()
    {
        Texture2D photo = new Texture2D(webcamTexture.width, webcamTexture.height);
        photo.SetPixels(webcamTexture.GetPixels());
        photo.Apply();

        if (photo == null)
        {
            Debug.LogError("Captured photo is null.");
            return;
        }

        if (photoUploader != null)
        {
            Debug.Log("Uploading photo...");
            await photoUploader.UploadPhoto(photo);

            // After uploading the photo, call OnPhotoCaptured to handle XP and achievements.
            await OnPhotoCaptured();
        }
        else
        {
            Debug.LogWarning("PhotoUploader reference not set!");
        }
    }

    void OnDestroy()
    {
        StopCamera();
    }



}
