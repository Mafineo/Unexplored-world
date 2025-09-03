using UnityEngine;
using UnityEngine.UI;

public class InterractPageScript : PageScript
{
    [Header("Параметры")]
    public string PageName;
    public bool ArrowUp;

    [HideInInspector] public ItemPictureScript[] _Pictures;
    [HideInInspector] public ItemPictureScript[] _BigPictures;
    [HideInInspector] public ItemPictureScript[] _SidePictures;
    private Text _PageName;
    private Transform _ArrowTransform;
    private Animator _Animator;

    public void Load()
    {
        _PageName = transform.GetChild(0).GetComponent<Text>();
        _ArrowTransform = transform.GetChild(1).GetComponent<Transform>();
        _Pictures = new ItemPictureScript[transform.GetChild(2).childCount];
        for (int PictureID = 0; PictureID < transform.GetChild(2).childCount; PictureID++) _Pictures[PictureID] = transform.GetChild(2).transform.GetChild(PictureID).GetComponent<ItemPictureScript>();
        _BigPictures = new ItemPictureScript[2];
        _BigPictures[0] = transform.GetChild(3).transform.GetChild(0).GetComponent<ItemPictureScript>();
        _BigPictures[1] = transform.GetChild(3).transform.GetChild(1).GetComponent<ItemPictureScript>();
        _SidePictures = new ItemPictureScript[2];
        _SidePictures[0] = transform.GetChild(4).transform.GetChild(0).GetComponent<ItemPictureScript>();
        _SidePictures[1] = transform.GetChild(4).transform.GetChild(1).GetComponent<ItemPictureScript>();
        GameScript._MainBookScript.MainBookClosedEvent += () => { _BigPictures[1].gameObject.SetActive(false); for (int PictureID = 0; PictureID < _SidePictures.Length; PictureID++) { _SidePictures[PictureID].gameObject.SetActive(false); } };
        foreach (ItemPictureScript BigPicture in _BigPictures) BigPicture._ParentPage = GetComponent<PageScript>();

        _PageName.text = PageName;
        if (ArrowUp) _ArrowTransform.localScale = new Vector3(1, -1, 1);
        else _ArrowTransform.localScale = new Vector3(1, 1, 1);

        _Animator = GetComponent<Animator>();
    }

    public void ClearPage() { for (int PictureID = 0; PictureID < _Pictures.Length; PictureID++) _Pictures[PictureID].Load(ItemPictureScript.PictureTypes.Empty); }

    public void StartAct() => _Animator.SetTrigger("Start");
}