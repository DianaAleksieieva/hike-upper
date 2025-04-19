using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhotoCaptureManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject HomePage;
    public GameObject TakePhotoPage;

    [Header("Camera Preview")]
    public RawImage cameraPreview;
    public AspectRatioFitter aspectFitter;

    private WebCamTexture webcamTexture;
    
    public PhotoUploader photoUploader;  

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

    IEnumerator AdjustPreview()
    {
        // Wait for at least 1 frame to ensure width/height are available
        yield return new WaitUntil(() => webcamTexture.width > 100);

        float ratio = (float)webcamTexture.width / webcamTexture.height;
        aspectFitter.aspectRatio = ratio;

        // Flip preview if needed
        cameraPreview.rectTransform.localEulerAngles = new Vector3(0, 0, -webcamTexture.videoRotationAngle);

        // Fix mirroring for front camera
        bool isFrontFacing = webcamTexture.videoVerticallyMirrored;
        cameraPreview.rectTransform.localScale = new Vector3(1, isFrontFacing ? -1 : 1, 1);
    }

    void StartCamera()
    {
        if (photoUploader == null)
        {
            photoUploader = Object.FindAnyObjectByType<PhotoUploader>();

        }

        if (photoUploader == null)
        {
            Debug.LogError("PhotoUploader not found in the scene.");
        }
        else
        {
            Debug.Log("PhotoUploader found: " + photoUploader);
        }

        if (webcamTexture == null)
        {
            webcamTexture = new WebCamTexture();
            cameraPreview.texture = webcamTexture;
        }

        webcamTexture.Play();
        StartCoroutine(AdjustPreview());
    }


    void StopCamera()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
            webcamTexture.Stop();
    }

    public void CapturePhoto()
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
            photoUploader.UploadPhoto(photo);
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
