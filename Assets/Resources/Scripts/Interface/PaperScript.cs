using UnityEngine;
using UnityEngine.UI;

public class PaperScript : MonoBehaviour
{
    public string Name { set { _Name.text = value; } }
    [HideInInspector] public int _EnabledPicturesCount;
    [HideInInspector] public RectTransform _PicturesContent;
    [HideInInspector] public ItemPictureScript[] _Pictures;
    [HideInInspector] public GameObject[] WorkPages;

    private Text _Name;

    public void Load()
    {
        _Name = transform.GetChild(0).GetComponent<Text>();
        _PicturesContent = transform.GetChild(1).transform.GetChild(0).GetComponent<RectTransform>();
        _Pictures = new ItemPictureScript[transform.GetChild(1).transform.GetChild(0).transform.childCount];
        for (int PictureID = 0; PictureID < transform.GetChild(1).transform.GetChild(0).transform.childCount; PictureID++) _Pictures[PictureID] = transform.GetChild(1).transform.GetChild(0).transform.GetChild(PictureID).GetComponent<ItemPictureScript>();
    }

    public void ClearPaper()
    {
        for (int PictureID = 0; PictureID < _Pictures.Length; PictureID++)
        { 
            _Pictures[PictureID].gameObject.SetActive(false);
            _Pictures[PictureID].SetTrackItem();
        }
        _EnabledPicturesCount = 0;
        WorkPages = new GameObject[0];
    }

    public void SetContentSize()
    {
        int LayersCount = 1;
        for (int PictureID = 0, Streak = 0; PictureID < _Pictures.Length; PictureID++, Streak++)
        {
            if (_Pictures[PictureID].gameObject.activeSelf)
            {
                if (Streak > 2)
                {
                    Streak = 0;
                    LayersCount++;
                }
            }
        }
        if (LayersCount < 4) LayersCount = 4;
        _PicturesContent.sizeDelta = new Vector2(_PicturesContent.sizeDelta.x, LayersCount * 100);
        _PicturesContent.localPosition = new Vector2(0, -500);
    }

    public bool NeedToOpenPaper(GameObject[] CurrentPages)
    {
        for (int CurrentPageID = 0; CurrentPageID < CurrentPages.Length; CurrentPageID++) for (int PageID = 0; PageID < WorkPages.Length; PageID++) if (CurrentPages[CurrentPageID] && CurrentPages[CurrentPageID].name == WorkPages[PageID].name) return true;
        return false;
    }
}
