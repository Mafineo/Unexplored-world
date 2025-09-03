using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Collections;

public class VillagerEquipmentPageScript : PageScript
{
    private Image _VillagerStrengthBar;
    private Image _VillagerAgilityBar;
    private Image _VillagerIntelligenceBar;
    private ItemPictureScript[] _Pictures;
    private GameObject _BagInventory;
    private Coroutine _OpenPaperCoroutine;
    private int _TrackVillagerID;
    private VillagersPageScript _VillagersPageScript;
    private int OpenedPaperSlot = -1;

    public void OpenPaper(int SlotID)
    {
        if (_OpenPaperCoroutine != null) StopCoroutine(_OpenPaperCoroutine);
        _OpenPaperCoroutine = StartCoroutine(StartOpenPaper());
        IEnumerator StartOpenPaper()
        {
            GameScript._MainBookScript._Animator.SetBool("Paper", false);
            while (!GameScript._MainBookScript._Animator.GetCurrentAnimatorStateInfo(0).IsName("Opened")) yield return new WaitForEndOfFrame();
            if (GameScript._MainBookScript._Paper.WorkPages.Length > 0 && GameScript._MainBookScript._Paper.WorkPages[0] == gameObject && SlotID == OpenedPaperSlot)
            {
                GameScript._MainBookScript._Paper.WorkPages = new GameObject[0];
                GameScript._MainBookScript._Animator.SetBool("Paper", false);
                OpenedPaperSlot = -1;
            }
            else
            {
                GameScript._MainBookScript._Paper.ClearPaper();
                GameScript._MainBookScript._Paper._Pictures[0].Load(ItemPictureScript.PictureTypes.Button, (SlotID != 2) ? _Pictures[SlotID].Icon : null, Action: delegate { SetSlotItem(SlotID, null); });
                int PictureID = 1;
                foreach (KeyValuePair<string, int> Item in GameScript.Village.Items)
                {
                    ItemScript _ItemScript = GameScript.FindGameItem(Item.Key);
                    if (Item.Value > 0)
                    {
                        if (SlotID == 0 && _ItemScript.IsInstrument && !_ItemScript.IsClothes && _ItemScript.name != "Handbag" && _ItemScript.name != "Bag" ||
                            SlotID == 1 && !_ItemScript.IsInstrument && _ItemScript.IsClothes && _ItemScript.name != "Handbag" && _ItemScript.name != "Bag" ||
                            SlotID == 2 && _ItemScript.IsInstrument || SlotID == 2 && _ItemScript.IsClothes ||
                            SlotID > 2 && _ItemScript.name != "Handbag" && _ItemScript.name != "Bag" && _ItemScript.IsInstrument ||
                            SlotID > 2 && _ItemScript.name != "Handbag" && _ItemScript.name != "Bag" && _ItemScript.IsClothes ||
                            SlotID >= 2 && _ItemScript.name != "Handbag" && _ItemScript.name != "Bag" && _ItemScript.HealCount > 0 ||
                            SlotID >= 2 && _ItemScript.name != "Handbag" && _ItemScript.name != "Bag" && _ItemScript.FeedCount > 0)
                        {
                            int Temp = SlotID;
                            GameScript._MainBookScript._Paper._Pictures[PictureID].Load(ItemPictureScript.PictureTypes.Button, _ItemScript.Icon, Action: delegate { SetSlotItem(Temp, _ItemScript); });
                            GameScript._MainBookScript._Paper._Pictures[PictureID].SetTrackItem(_ItemScript);
                            PictureID++;
                        }
                    }
                    GameScript._MainBookScript._Paper.SetContentSize();
                }
                GameScript._MainBookScript._Paper.WorkPages = new GameObject[] { gameObject };
                GameScript._MainBookScript._Paper._EnabledPicturesCount = PictureID;
                switch (SlotID)
                {
                    case 0: GameScript._MainBookScript._Paper.Name = "Инструменты"; break;
                    case 1: GameScript._MainBookScript._Paper.Name = "Одежда"; break;
                    case 2: GameScript._MainBookScript._Paper.Name = "Карманы"; break;
                    default: GameScript._MainBookScript._Paper.Name = "Сумка"; break;
                }
                GameScript._MainBookScript._Animator.SetBool("Paper", true);
                OpenedPaperSlot = SlotID;
            }
        }
    }

    public void SetSlotItem(int SlotID, ItemScript Item)
    {
        if (GameScript.Village.Villagers[_TrackVillagerID].SetItem(SlotID, Item))
        {
            GameScript._MainBookScript._Animator.SetBool("Paper", false);
            LoadInformation(_TrackVillagerID);
            _VillagersPageScript._VillagerInformationPageScript.LoadInformation(_TrackVillagerID);
            OpenedPaperSlot = -1;
        }
    }

    public void Load()
    {
        _VillagerStrengthBar = transform.GetChild(1).transform.GetChild(1).GetComponent<Image>();
        _VillagerAgilityBar = transform.GetChild(2).transform.GetChild(1).GetComponent<Image>();
        _VillagerIntelligenceBar = transform.GetChild(3).transform.GetChild(1).GetComponent<Image>();
        _Pictures = new ItemPictureScript[transform.GetChild(4).transform.GetChild(0).childCount + transform.GetChild(4).transform.GetChild(1).childCount];
        for (int PictureID = 0; PictureID < transform.GetChild(4).transform.GetChild(0).childCount; PictureID++) _Pictures[PictureID] = transform.GetChild(4).transform.GetChild(0).transform.GetChild(PictureID).GetComponent<ItemPictureScript>();
        for (int PictureID = 0; PictureID < transform.GetChild(4).transform.GetChild(1).childCount; PictureID++) _Pictures[PictureID + transform.GetChild(4).transform.GetChild(0).childCount] = transform.GetChild(4).transform.GetChild(1).transform.GetChild(PictureID).GetComponent<ItemPictureScript>();
        _BagInventory = transform.GetChild(4).transform.GetChild(1).gameObject;
        _VillagersPageScript = GameScript._MainBookScript.FindPage(GameScript._MainBookScript.FindPageID("Villagers|List"), true).GetComponent<VillagersPageScript>();
        _VillagersPageScript._VillagerEquipmentPageScript = GetComponent<VillagerEquipmentPageScript>();
    }

    public void LoadInformation(int VillagerID)
    {
        _VillagerStrengthBar.fillAmount = 0.2f * GameScript.Village.Villagers[VillagerID].Characteristics.Strength;
        _VillagerAgilityBar.fillAmount = 0.2f * GameScript.Village.Villagers[VillagerID].Characteristics.Agility;
        _VillagerIntelligenceBar.fillAmount = 0.2f * GameScript.Village.Villagers[VillagerID].Characteristics.Intelligence;
        for(int SlotID = 0; SlotID < 3; SlotID++)
        {
            int Temp = SlotID;
            _Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, (GameScript.Village.Villagers[VillagerID].Items[SlotID]) ? GameScript.FindGameItem(GameScript.Village.Villagers[VillagerID].Items[SlotID].name).Icon : null, Action: delegate { OpenPaper(Temp); });
            _Pictures[SlotID].UpdateText(FillAmount: (GameScript.Village.Villagers[VillagerID].Items[SlotID]) ? 1 : 0);
        }
        if (GameScript.Village.Villagers[VillagerID].Items[2] && GameScript.Village.Villagers[VillagerID].Items[2].name == "Handbag" || GameScript.Village.Villagers[VillagerID].Items[2] && GameScript.Village.Villagers[VillagerID].Items[2].name == "Bag")
        {
            for (int SlotID = 3; SlotID < (GameScript.Village.Villagers[VillagerID].Items[2].name == "Handbag" ? 5 : 6); SlotID++)
            {
                int Temp = SlotID;
                _Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, (GameScript.Village.Villagers[VillagerID].Items[SlotID]) ? GameScript.FindGameItem(GameScript.Village.Villagers[VillagerID].Items[SlotID].name).Icon : null, Action: delegate { OpenPaper(Temp); });
                _Pictures[SlotID].UpdateText(FillAmount: (GameScript.Village.Villagers[VillagerID].Items[SlotID]) ? 1 : 0);
            }
            _BagInventory.SetActive(true);
        }
        else _BagInventory.SetActive(false);
        _TrackVillagerID = VillagerID;
    }
}
