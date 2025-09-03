using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public class MainBookScript : MonoBehaviour
{
    private Coroutine _StartFindTargetPage;
    public string TargetPage
    {
        set
        {
            if (_StartFindTargetPage != null)
            {
                StopCoroutine(_StartFindTargetPage);
                _Animator.speed = 1;
            }
            if (MainBookIsStatic) _StartFindTargetPage = StartCoroutine(StartFindTargetPage());
            IEnumerator StartFindTargetPage()
            {
                if (GameScript._MainBookScript.MainBookState != MainBookStates.Opened) GameScript._MainBookScript.Take();
                while (GameScript._MainBookScript.MainBookState != MainBookStates.Opened) yield return new WaitForEndOfFrame();
                int Target = -1;
                for (int PageID = 0; PageID < PagesList.Count; PageID++)
                {
                    if (PagesList[PageID].gameObject.name == value)
                    {
                        Target = PageID;
                        break;
                    }
                }
                if (Target % 2 != 0) Target--;
                while (true)
                {
                    if (Target == CurentPageID)
                    {
                        _Animator.speed = 1;
                        break;
                    }
                    else
                    {
                        if (_Animator.GetCurrentAnimatorStateInfo(0).IsName("OpenedPaper") || _Animator.GetCurrentAnimatorStateInfo(0).IsName("ClosingPaper")) _Animator.speed = 1;
                        else _Animator.speed = 8;
                        if (Target > CurentPageID)
                        {
                            if (Math.Abs(Target - CurentPageID) >= 8) CurentPageID += 2;
                            FlipPageRight();
                        }
                        else if (Target < CurentPageID)
                        {
                            if (Math.Abs(Target - CurentPageID) >= 8) CurentPageID -= 2;
                            FlipPageLeft();
                        }
                    }
                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }
    [HideInInspector] public int CurentPageID = 0;

    public Action CloseMainBookEvent;
    public Action MainBookClosedEvent;

    private List<GameObject> PagesContent;
    private List<GameObject> PagesList;
    private List<PageScript> PagesScripts;
    private Transform _BookContent;
    private Transform _LeftPassivePage;
    private Transform _LeftActivePage;
    private Transform _RightPassivePage;
    private Transform _RightActivePage;
    private GameObject _EmptyPage;
    private GameObject _MainBookScriptButton;
    [HideInInspector] public PaperScript _Paper;
    [HideInInspector] public Animator _Animator;
    private const float PagePositionOffset = 1.848f;

    private void Awake()
    {
        _Paper = transform.GetChild(0).GetComponent<PaperScript>();
        _BookContent = transform.GetChild(3).GetComponent<Transform>();
        _LeftPassivePage = transform.GetChild(2).transform.GetChild(0).transform.GetChild(0).GetComponent<Transform>();
        _LeftActivePage = transform.GetChild(2).transform.GetChild(0).transform.GetChild(1).GetComponent<Transform>();
        _RightPassivePage = transform.GetChild(2).transform.GetChild(1).transform.GetChild(0).GetComponent<Transform>();
        _RightActivePage = transform.GetChild(2).transform.GetChild(1).transform.GetChild(1).GetComponent<Transform>();
        _EmptyPage = transform.GetChild(3).transform.GetChild(transform.GetChild(2).childCount - 1).gameObject;
        _MainBookScriptButton = GameObject.Find("MainBookButton");
        _Animator = GetComponent<Animator>();
    }

    private void LoadPagesContent()
    {
        _Paper.Load();
        PagesContent = new List<GameObject>();
        PagesScripts = new List<PageScript>();
        for (int PageID = 0; PageID < _BookContent.childCount; PageID++)
        {
            PagesContent.Add(_BookContent.GetChild(PageID).gameObject);
            PagesScripts.Add(PagesContent[PagesContent.Count - 1].GetComponent<PageScript>());
            PagesContent[PagesContent.Count - 1].gameObject.SetActive(true);
            PagesContent[PagesContent.Count - 1].gameObject.SetActive(false);
            if (PagesContent[PagesContent.Count - 1].TryGetComponent(out InformationPageScript InformationPageScript)) InformationPageScript.Load();
            else if (PagesContent[PagesContent.Count - 1].TryGetComponent(out InterractPageScript InterractPageScript)) InterractPageScript.Load();
            else if (PagesContent[PagesContent.Count - 1].TryGetComponent(out BuildingsPageScript BuildingsPageScript)) BuildingsPageScript.Load();
            else if (PagesContent[PagesContent.Count - 1].TryGetComponent(out StoragePageScript StoragePageScript)) StoragePageScript.Load();
            else if (PagesContent[PagesContent.Count - 1].TryGetComponent(out UsePageScript UsePageScript)) UsePageScript.Load();
            else if (PagesContent[PagesContent.Count - 1].TryGetComponent(out ConfirmPageScript ConfirmPageScript)) ConfirmPageScript.Load();
            else if (PagesContent[PagesContent.Count - 1].TryGetComponent(out VillagersPageScript VillagersPageScript)) VillagersPageScript.Load();
            else if (PagesContent[PagesContent.Count - 1].TryGetComponent(out VillagerInformationPageScript VillagerInformationPageScript)) VillagerInformationPageScript.Load();
            else if (PagesContent[PagesContent.Count - 1].TryGetComponent(out VillagerEquipmentPageScript VillagerEquipmentPageScript)) VillagerEquipmentPageScript.Load();
        }
    }

    private void LoadPage(GameObject Page, Vector3 Position, Quaternion Rotation, Transform PageParent)
    {
        if (PageParent.transform.childCount > 0 && PageParent.transform.GetChild(0).gameObject != Page)
        {
            Transform OldPageTransform = PageParent.transform.GetChild(0).GetComponent<Transform>();
            OldPageTransform.gameObject.SetActive(false);
            OldPageTransform.position = _BookContent.position;
            OldPageTransform.rotation = _BookContent.rotation;
            OldPageTransform.SetParent(_BookContent);
        }
        if (Page)
        {
            Transform PageTransform = Page.GetComponent<Transform>();
            PageTransform.SetParent(PageParent.transform);
            PageTransform.position = Position;
            PageTransform.rotation = Rotation;
            Page.SetActive(true);
        }
    }

    public void Load()
    {
        LoadPagesContent();
        LoadPagesList();
    }

    public void LoadPagesList()
    {
        PagesList = new List<GameObject>();
        int AddedPagesCount = 0;
        int HiddenPagesCount = 0;
        for (int PageID = 0; PageID < PagesContent.Count; PageID++)
        {
            if (GameScript._EnviromentScript.Location != EnviromentScript.Locations.Cave && !PagesScripts[PageID].PageIsHidden || GameScript._EnviromentScript.Location == EnviromentScript.Locations.Cave && !PagesScripts[PageID].PageIsHidden && !PagesScripts[PageID].HideInCave)
            {
                if (PagesScripts[PageID].PageIsImportant && (PageID - HiddenPagesCount + AddedPagesCount) % 2 != 0)
                {
                    PagesList.Add(_EmptyPage);
                    AddedPagesCount++;
                }
                PagesList.Add(PagesContent[PageID]);
            }
            else HiddenPagesCount++;
        }
    } 

    public void HidePagesAfterClose()
    {
        for (int PageID = 0; PageID < PagesContent.Count; PageID++)
        {
            if (PagesScripts[PageID].HidePageAfterClose)
            {
                PagesScripts[PageID].PageIsHidden = true;
                CurentPageID = 0;
            }
        }
        LoadPagesList();
    }

    public void Take()
    {
        if (MainBookIsStatic)
        {
            if (MainBookState == MainBookStates.Closed)
            {
                LoadPagesList();
                LoadPage(FindPage(CurentPageID), _LeftPassivePage.position - new Vector3(PagePositionOffset / 4 * GameScript._Camera.orthographicSize, 0, 0), Quaternion.identity, _LeftPassivePage);
                LoadPage(FindPage(CurentPageID + 1), _RightPassivePage.position + new Vector3(PagePositionOffset / 4 * GameScript._Camera.orthographicSize, 0, 0), Quaternion.identity, _RightPassivePage);
                _Animator.SetTrigger("Change");
                GameScript._CameraScript.CameraIsFreezed = true;
            }
            else if (MainBookState == MainBookStates.Opened)
            {
                _Animator.SetBool("Paper", false);
                HidePagesAfterClose();
                _Animator.SetTrigger("Change");
                GameScript._CameraScript.CameraIsFreezed = false;
                _MainBookScriptButton.SetActive(true);
                CloseMainBookEvent?.Invoke();
                StartCoroutine(WaitForBookClose());
            }
        }
        IEnumerator WaitForBookClose()
        {
            while (MainBookState != MainBookStates.Closed) yield return new WaitForEndOfFrame();
            _Paper.ClearPaper();
            GameScript.Village.VillagersListChangedEvent?.Invoke();
            MainBookClosedEvent?.Invoke();
        }
    }

    public void TakeForConfirm()
    {
        if (MainBookIsStatic)
        {
            if (MainBookState == MainBookStates.Closed)
            {
                LoadPage(FindPage(FindPageID("Confirm"), true), _LeftPassivePage.position - new Vector3(PagePositionOffset / 4 * GameScript._Camera.orthographicSize, 0, 0), Quaternion.identity, _LeftPassivePage);
                LoadPage(null, _RightPassivePage.position + new Vector3(PagePositionOffset / 4 * GameScript._Camera.orthographicSize, 0, 0), Quaternion.identity, _RightPassivePage);
                _Animator.SetTrigger("Confirm");
                _MainBookScriptButton.SetActive(false);
            }
            else if (MainBookState == MainBookStates.Opened) StartCoroutine(CloseBookAndConfirm());
            IEnumerator CloseBookAndConfirm()
            {
                Take();
                while (MainBookState != MainBookStates.Closed) yield return new WaitForEndOfFrame();
                TakeForConfirm();
            }
        }
    }

    public void Close()
    {
        if (MainBookState != MainBookStates.Closed)
        {
            Take();
            CurentPageID = 0;
        }
    }

    public void FlipPageLeft()
    {
        if (MainBookState == MainBookStates.Opened && MainBookIsStatic) if (FindPage(CurentPageID - 2)) StartCoroutine(FlipPage());
        IEnumerator FlipPage()
        {
            _Animator.SetBool("Paper", false);
            while (!_Animator.GetCurrentAnimatorStateInfo(0).IsName("Opened")) yield return new WaitForEndOfFrame();
            _Animator.SetTrigger("FlipLeft");
            LoadPage(FindPage(CurentPageID - 2), _LeftPassivePage.position - new Vector3(PagePositionOffset / 4 * GameScript._Camera.orthographicSize, 0, 0), Quaternion.identity, _LeftPassivePage);
            LoadPage(FindPage(CurentPageID), _LeftActivePage.position - new Vector3(0, 0, PagePositionOffset / 4 * GameScript._Camera.orthographicSize), Quaternion.Euler(0, -90, 0), _LeftActivePage);
            LoadPage(FindPage(CurentPageID - 1), _RightActivePage.position - new Vector3(0, 0, PagePositionOffset / 4 * GameScript._Camera.orthographicSize), Quaternion.Euler(0, 90, 0), _RightActivePage);
            CurentPageID -= 2;
        }
    }

    public void FlipPageRight()
    {
        if (MainBookState == MainBookStates.Opened && MainBookIsStatic) if (FindPage(CurentPageID + 2)) StartCoroutine(FlipPage());
        IEnumerator FlipPage()
        {
            _Animator.SetBool("Paper", false);
            while (!_Animator.GetCurrentAnimatorStateInfo(0).IsName("Opened")) yield return new WaitForEndOfFrame();
            _Animator.SetTrigger("FlipRight");
            LoadPage(FindPage(CurentPageID + 1), _RightActivePage.position - new Vector3(0, 0, PagePositionOffset / 4 * GameScript._Camera.orthographicSize), Quaternion.Euler(0, 90, 0), _RightActivePage);
            LoadPage(FindPage(CurentPageID + 2), _LeftActivePage.position - new Vector3(0, 0, PagePositionOffset / 4 * GameScript._Camera.orthographicSize), Quaternion.Euler(0, -90, 0), _LeftActivePage);
            LoadPage(FindPage(CurentPageID + 3), _RightPassivePage.position + new Vector3(PagePositionOffset / 4 * GameScript._Camera.orthographicSize, 0, 0), Quaternion.identity, _RightPassivePage);
            CurentPageID += 2;
        }
    }

    public void PageFlipEnd()
    {
        LoadPage(FindPage(CurentPageID), _LeftPassivePage.position - new Vector3(PagePositionOffset / 4 * GameScript._Camera.orthographicSize, 0, 0), Quaternion.identity, _LeftPassivePage);
        LoadPage(FindPage(CurentPageID + 1), _RightPassivePage.position + new Vector3(PagePositionOffset / 4 * GameScript._Camera.orthographicSize, 0, 0), Quaternion.identity, _RightPassivePage);
        if (_Paper._EnabledPicturesCount > 0)
        {
            GameObject[] Pages = { FindPage(CurentPageID), FindPage(CurentPageID + 1) };
            if (_Paper.NeedToOpenPaper(Pages)) _Animator.SetBool("Paper", true);
        }
    }

    public int FindPageID(string Name) { for (int PageID = 0; PageID < PagesContent.Count; PageID++) if (PagesContent[PageID].name == Name) return PageID; return 0; }

    public GameObject FindPage(int PageID, bool ShowHiddenPages = false)
    {
        if (!ShowHiddenPages && PageID >= 0 && PageID < PagesList.Count) return PagesList[PageID];
        else if (ShowHiddenPages && PageID >= 0 && PageID < PagesContent.Count) return PagesContent[PageID];
        else return null;
    }

    public bool MainBookIsStatic
    {
        get
        {
            for (int PagesContentID = 0; PagesContentID < PagesScripts.Count; PagesContentID++) if (!PagesScripts[PagesContentID].PageIsStatic) return false;
            if (MainBookState == MainBookStates.Change) return false;
            if (GameScript._EnviromentScript._AsyncOperation != null) return false;
            return true;
        }
    }

    public enum MainBookStates { Closed, Change, Opened }
    public MainBookStates MainBookState
    {
        get
        {
            if (_Animator.GetCurrentAnimatorStateInfo(0).IsName("Closed")) return MainBookStates.Closed;
            else if (_Animator.GetCurrentAnimatorStateInfo(0).IsName("Opened") || _Animator.GetCurrentAnimatorStateInfo(0).IsName("Confirm") || _Animator.GetCurrentAnimatorStateInfo(0).IsName("OpenedPaper")) return MainBookStates.Opened;
            else return MainBookStates.Change;
        }
    }
}
