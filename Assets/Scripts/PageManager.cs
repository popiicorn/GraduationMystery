using UnityEngine;

public class PageManager : MonoBehaviour
{
    public string[] pagePrefabNames = { "Page_00", "Page_01", "Page_02" };
    private GameObject currentPage;
    private int currentPageIndex = 0;

    void Start()
    {
        LoadPage(currentPageIndex);
    }

    public void NextPage()
    {
        if (currentPageIndex < pagePrefabNames.Length - 1)
        {
            currentPageIndex++;
            //Debug.Log($"ŽŸ‚Ìƒy[ƒW‚Ö: {currentPageIndex}");
            LoadPage(currentPageIndex);
        }
    }

    public void PreviousPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            LoadPage(currentPageIndex);
        }
    }

    void LoadPage(int index)
    {
        if (currentPage != null)
            Destroy(currentPage);

        GameObject pagePrefab = Resources.Load<GameObject>($"PagePrefabs/{pagePrefabNames[index]}");
        currentPage = Instantiate(pagePrefab, transform);
    }
}
