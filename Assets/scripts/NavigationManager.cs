using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public GameObject HomePage;
    public GameObject PhotoPage;
    public GameObject ProfilePage;
    public GameObject HikeListPage;
    public GameObject HikeItemPage;
    public GameObject CommunityPage;
    public GameObject ReportPage;

    public void backHome() {
        HomePage.SetActive(true);
        PhotoPage.SetActive(false);
        ProfilePage.SetActive(false);
        HikeListPage.SetActive(false);
        HikeItemPage.SetActive(false);
        CommunityPage.SetActive(false);
        ReportPage.SetActive(false);
    }

    public void toProfilePage() {
        HomePage.SetActive(false);
        ProfilePage.SetActive(true);
    }
    public void toHikeListPage() {
        HomePage.SetActive(false);
        HikeListPage.SetActive(true);
    }
    public void toHikeItemPage() {
        HikeListPage.SetActive(false);
        HikeItemPage.SetActive(true);
    }
    public void toCommunityPage() {
        HomePage.SetActive(false);
        CommunityPage.SetActive(true);
    }
    public void toReportPage() {
        HomePage.SetActive(false);
        ReportPage.SetActive(true);
    }
}
