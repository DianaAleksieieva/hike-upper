using UnityEngine;
using System.Collections;
using Firebase.Auth;

public class NavigationManager : MonoBehaviour
{
    public GameObject FirstPage;
    public GameObject LoginPage;
    public GameObject SignUpPage;
    public GameObject ForgotPage;
    public GameObject HomePage;
    public GameObject PhotoPage;
    public GameObject ProfilePage;
    public GameObject HikeListPage;
    public GameObject HikeItemPage;
    public GameObject CommunityPage;
    public GameObject ReportPage;
    private string userKey;
    private ProfileLoader profileLoader;

    void Start()
    {
        profileLoader = FindFirstObjectByType<ProfileLoader>();

    }
    public void OnLoginButtonPressed()
    {
        StartCoroutine(WaitForUserKeyThenGoHome());
    }
    private IEnumerator WaitForUserKeyThenGoHome()
    {
        float timeout = 3f;
        float elapsed = 0f;

        while (string.IsNullOrEmpty(profileLoader.GetUserKey()) && elapsed < timeout)
        {
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        userKey = profileLoader.GetUserKey();
        Debug.Log("UserKey: " + userKey);

        if (!string.IsNullOrEmpty(userKey))
        {
            PhotoPage.SetActive(false);
            ProfilePage.SetActive(false);
            HikeListPage.SetActive(false);
            HikeItemPage.SetActive(false);
            CommunityPage.SetActive(false);
            ReportPage.SetActive(false);
            SignUpPage.SetActive(false);
            LoginPage.SetActive(false);
            ForgotPage.SetActive(false);
            FirstPage.SetActive(false);
            HomePage.SetActive(true);
        }
        else
        {
            Debug.LogWarning("UserKey still empty after timeout.");
        }
        
    }

    public void toProfilePage()
    {
        HomePage.SetActive(false);
        ProfilePage.SetActive(true);
    }
    public void toHikeListPage()
    {
        HomePage.SetActive(false);
        HikeListPage.SetActive(true);
    }
    public void toHikeItemPage()
    {
        HikeListPage.SetActive(false);
        HikeItemPage.SetActive(true);
    }
    public void toCommunityPage()
    {
        HomePage.SetActive(false);
        CommunityPage.SetActive(true);
    }
    public void toReportPage()
    {
        HomePage.SetActive(false);
        ReportPage.SetActive(true);
    }
    public void toLoginPage()
    {
        FirstPage.SetActive(false);
        SignUpPage.SetActive(false);
        ForgotPage.SetActive(false);
        LoginPage.SetActive(true);

    }
    public void toSignUpPage()
    {
        FirstPage.SetActive(false);
        LoginPage.SetActive(false);
        SignUpPage.SetActive(true);
    }
    public void toForgotPage()
    {
        LoginPage.SetActive(false);
        ForgotPage.SetActive(true);
    }
    public void QuitApp()
    {
        Application.Quit();
    }
    public void LogOut()
    {
        HomePage.SetActive(false);
        ProfilePage.SetActive(false);
        LoginPage.SetActive(true);
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();
    }
    public void backHome()
    {
        PhotoPage.SetActive(false);
        ProfilePage.SetActive(false);
        HikeListPage.SetActive(false);
        HikeItemPage.SetActive(false);
        CommunityPage.SetActive(false);
        ReportPage.SetActive(false);
        SignUpPage.SetActive(false);
        LoginPage.SetActive(false);
        ForgotPage.SetActive(false);
        FirstPage.SetActive(false);
        HomePage.SetActive(true);
    }
}
