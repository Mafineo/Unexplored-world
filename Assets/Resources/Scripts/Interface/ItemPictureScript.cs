using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class ItemPictureScript : MonoBehaviour
{
    [Header("Параметры")]
    public Sprite Icon;

    private Image _Background;
    private Button _Button;
    private Image _Image;
    private Image _FilledImage;
    private GameObject _TextLine;
    private Image _TextLineImage;
    private Text _Text;
    private Animator _Animator;
    private ItemScript _TrackItem;
    private int _TrackAvailableItems;
    private int _TrackVillagerID;
    public enum PictureTypes { Empty, Picture, Button }
    [HideInInspector] public PageScript _ParentPage;
    [HideInInspector] public bool AnimationCenterIsGone = true;
    private bool IsLoaded = false;

    public Color PictureColor
    {
        set
        {
            _Background.color = value;
            _FilledImage.color = value;
            _TextLineImage.color = value;
        }
    }

    private void LoadDetails()
    {
        _Button = GetComponent<Button>();
        _Background = transform.GetChild(0).GetComponent<Image>();
        _Image = transform.GetChild(1).GetComponent<Image>();
        _FilledImage = transform.GetChild(1).transform.GetChild(0).GetComponent<Image>();
        _TextLine = transform.GetChild(2).gameObject;
        _TextLineImage = _TextLine.GetComponent<Image>();
        _Text = transform.GetChild(2).transform.GetChild(0).GetComponent<Text>();
        _Animator = GetComponent<Animator>();
    }

    private void ShowItems()
    {
        if (_TrackVillagerID == -1)
        {
            if (_TrackAvailableItems == 0) _Text.text = GameScript.Village.GetItemsCount(_TrackItem).ToString();
            else _Text.text = GameScript.Village.GetItemsCount(_TrackItem).ToString() + "/" + _TrackAvailableItems;
        }
        else _Text.text = GameScript.Village.Villagers[_TrackVillagerID].InventoryItemsCount(_TrackItem).ToString();
        _TextLine.SetActive(true);
    }

    public void Load(PictureTypes PictureType, Sprite Picture = null, string Text = null, UnityAction Action = null)
    {
        if(!IsLoaded)
        {
            LoadDetails();
            IsLoaded = true;
        }

        _Button.onClick.RemoveAllListeners();
        if (_TrackItem) GameScript.Village.ItemsChangedEvent -= ShowItems;
        if (PictureType == PictureTypes.Empty)
        {
            _Button.enabled = false;
            _Image.enabled = false;
            _FilledImage.enabled = false;
            _TextLine.SetActive(false);
            _Text.enabled = false;
        }
        else if (PictureType == PictureTypes.Picture)
        {
            _Button.enabled = false;
            if (Picture) _Image.sprite = Picture;
            else _Image.sprite = Icon;
            _Image.enabled = true;
            if (Picture) _FilledImage.sprite = Picture;
            else _FilledImage.sprite = Icon;
            _FilledImage.fillAmount = 1;
            _FilledImage.enabled = true;
            _Text.text = Text;
            _TextLine.SetActive(!String.IsNullOrEmpty(_Text.text));
            _Text.enabled = true;
        }
        else if (PictureType == PictureTypes.Button)
        {
            _Button.onClick.RemoveAllListeners();
            _Button.enabled = true;
            if (Picture)
            {
                _Image.sprite = Picture;
                _Image.enabled = true;
                _FilledImage.sprite = Picture;
                _FilledImage.fillAmount = 1;
                _FilledImage.enabled = true;
            }
            else if (Icon)
            {
                _Image.sprite = Icon;
                _Image.enabled = true;
                _FilledImage.sprite = Icon;
                _FilledImage.fillAmount = 1;
                _FilledImage.enabled = true;
            }
            else
            {
                _Image.enabled = false;
                _FilledImage.enabled = false;
            }
            _Text.text = Text;
            _TextLine.SetActive(!String.IsNullOrEmpty(_Text.text));
            _Text.enabled = true;
            _Button.onClick.AddListener(Action);
        }
        gameObject.SetActive(true);
        SetTrackItem(null);
    }

    public void UpdateText(string Text = null, float FillAmount = 1)
    {
        _FilledImage.fillAmount = FillAmount;
        _Text.text = Text;
        _TextLine.SetActive(!String.IsNullOrEmpty(_Text.text));
        _Text.enabled = true;
    }

    public void SetTrackItem(ItemScript TrackItem = null, int TrackAvailableItems = 0, int TrackVillagerID = -1)
    {
        if (_TrackItem) GameScript.Village.ItemsChangedEvent -= ShowItems;
        if (TrackItem)
        {
            _TrackItem = TrackItem;
            _TrackAvailableItems = TrackAvailableItems;
            _TrackVillagerID = TrackVillagerID;
            ShowItems();
            GameScript.Village.ItemsChangedEvent += ShowItems;
        }
    }

    public void CollectAct()
    {
        _ParentPage.PageIsStatic = false;
        AnimationCenterIsGone = false;
        _Animator.SetTrigger("Collect");
        _Button.enabled = false;
    }

    public void StartAct()
    {
        _ParentPage.PageIsStatic = false;
        AnimationCenterIsGone = false;
        _Animator.SetTrigger("Start");
        _Button.enabled = false;
    }

    public void AnimationCenter() => AnimationCenterIsGone = true;

    public void MakePageStatic()
    {
        _ParentPage.PageIsStatic = true;
        _Button.enabled = true;
    }
}
