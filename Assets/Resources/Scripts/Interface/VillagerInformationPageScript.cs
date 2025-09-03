using UnityEngine.UI;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class VillagerInformationPageScript : PageScript
{
    private ItemPictureScript _VillagerPicture;
    private Text _VillagerHealthBarText;
    private Image _VillagerHealthBar;
    private GameObject VillagerSatietyBar;
    private Text _VillagerSatietyBarText;
    private Image _VillagerSatietyBar;
    private Text _CurrectWork;
    private ItemPictureScript[] Buttons;
    private VillagersPageScript _VillagersPageScript;
    private Coroutine _OpenPaperCoroutine;

    public void Load()
    {
        _VillagerPicture = transform.GetChild(1).GetComponent<ItemPictureScript>();
        _CurrectWork = transform.GetChild(2).GetComponent<Text>();
        _VillagerHealthBarText = transform.GetChild(3).transform.GetChild(0).GetComponent<Text>();
        _VillagerHealthBar = transform.GetChild(3).transform.GetChild(1).GetComponent<Image>();
        VillagerSatietyBar = transform.GetChild(4).gameObject;
        _VillagerSatietyBarText = transform.GetChild(4).transform.GetChild(0).GetComponent<Text>();
        _VillagerSatietyBar = transform.GetChild(4).transform.GetChild(1).GetComponent<Image>();
        _VillagersPageScript = GameScript._MainBookScript.FindPage(GameScript._MainBookScript.FindPageID("Villagers|List"), true).GetComponent<VillagersPageScript>();
        Buttons = new ItemPictureScript[3] { transform.GetChild(5).GetComponent<ItemPictureScript>(), transform.GetChild(6).GetComponent<ItemPictureScript>(), transform.GetChild(7).GetComponent<ItemPictureScript>() };
        _VillagersPageScript._VillagerInformationPageScript = GetComponent<VillagerInformationPageScript>();
    }

    public void LoadInformation(int VillagerID)
    {
        _VillagerPicture.Load(ItemPictureScript.PictureTypes.Picture, GameScript._HumanScript.FindClothes((GameScript.Village.Villagers[VillagerID].Items[1]) ? GameScript.Village.Villagers[VillagerID].Items[1].name : "PeasantClothes").FindClothesPart("Head"), GameScript.Village.Villagers[VillagerID].Name);

        _VillagerHealthBarText.text = "Здоровье: " + (int)(Math.Floor((float)(GameScript.Village.Villagers[VillagerID].Health) / GameScript._HumanScript.MaxHealth * 100)) + "%";
        _VillagerHealthBar.fillAmount = (float)(GameScript.Village.Villagers[VillagerID].Health) / GameScript._HumanScript.MaxHealth;

        if (GameScript.Village.Villagers[VillagerID].Health > 20) _VillagerPicture.PictureColor = Color.white;
        else _VillagerPicture.PictureColor = new Color(1f, (175f + 80 * (float)GameScript.Village.Villagers[VillagerID].Health / 20) / 255f, (175f + 80 * (float)GameScript.Village.Villagers[VillagerID].Health / 20) / 255f);

        if (GameScript.Village.Villagers[VillagerID].Health > 0)
        {
            VillagerSatietyBar.SetActive(true);
            _VillagerSatietyBarText.text = "Сытость: " + Math.Floor((float)(GameScript.Village.Villagers[VillagerID].Satiety) / 1000 * 100) + "%";
            _VillagerSatietyBar.fillAmount = (float)(GameScript.Village.Villagers[VillagerID].Satiety) / 1000;

            Buttons[0].Load(ItemPictureScript.PictureTypes.Button, Text: "Лечить", Action: delegate { StartUseItems(VillagerID, ItemsTypes.Heal); });
            Buttons[1].Load(ItemPictureScript.PictureTypes.Button, Text: "Кормить", Action: delegate { StartUseItems(VillagerID, ItemsTypes.Feed); });
            Buttons[2].gameObject.SetActive(false);

            _CurrectWork.text = "Сейчас:\n";
            if (GameScript.Village.Villagers[VillagerID].IsOutside)
            {
                if (GameScript.Village.Villagers[VillagerID]._Script && GameScript.Village.Villagers[VillagerID]._Script.IsReturning) _CurrectWork.text += "Возвращаеться";
                else _CurrectWork.text += "Работает";
            }
            else _CurrectWork.text += "Отдыхает";
        }
        else
        {
            VillagerSatietyBar.SetActive(false);

            Buttons[0].gameObject.SetActive(false);
            Buttons[1].gameObject.SetActive(false);
            Buttons[2].Load(ItemPictureScript.PictureTypes.Button, Text: "Убрать", Action: delegate { DeleteVillager(VillagerID); });
            _CurrectWork.text = null;
        }
    }

    private enum ItemsTypes { Heal, Feed }
    private void StartUseItems(int VillagerID, ItemsTypes ItemsType)
    {
        if (_OpenPaperCoroutine != null) StopCoroutine(_OpenPaperCoroutine);
        _OpenPaperCoroutine = StartCoroutine(StartOpenPaper());
        IEnumerator StartOpenPaper()
        {
            GameScript._MainBookScript._Animator.SetBool("Paper", false);
            while (!GameScript._MainBookScript._Animator.GetCurrentAnimatorStateInfo(0).IsName("Opened")) yield return new WaitForEndOfFrame();
            if (GameScript._MainBookScript._Paper.WorkPages.Length > 0 && GameScript._MainBookScript._Paper.WorkPages[0] == gameObject) GameScript._MainBookScript._Paper.WorkPages = new GameObject[0];
            else
            {
                GameScript._MainBookScript._Paper.ClearPaper();
                int PictureID = 0;
                foreach (KeyValuePair<string, int> Item in GameScript.Village.Items)
                {
                    ItemScript _ItemScript = GameScript.FindGameItem(Item.Key);
                    if (GameScript.Village.Villagers[VillagerID].InventoryItemsCount(_ItemScript) > 0)
                    {
                        if (ItemsType == ItemsTypes.Heal && _ItemScript.HealCount > 0 || ItemsType == ItemsTypes.Feed && _ItemScript.FeedCount > 0)
                        {
                            int Temp = PictureID;
                            GameScript._MainBookScript._Paper._Pictures[PictureID].Load(ItemPictureScript.PictureTypes.Button, _ItemScript.Icon, Action: delegate { GameScript.Village.Villagers[VillagerID].UseItem(_ItemScript); });
                            GameScript._MainBookScript._Paper._Pictures[PictureID].SetTrackItem(_ItemScript, TrackVillagerID: VillagerID);
                            PictureID++;
                        }
                    }
                    GameScript._MainBookScript._Paper.SetContentSize();
                }
                GameScript._MainBookScript._Paper.WorkPages = new GameObject[] { gameObject };
                GameScript._MainBookScript._Paper._EnabledPicturesCount = 1;
                switch(ItemsType)
                {
                    case ItemsTypes.Heal: GameScript._MainBookScript._Paper.Name = "Лечить"; break;
                    case ItemsTypes.Feed: GameScript._MainBookScript._Paper.Name = "Кормить"; break;
                }
                GameScript._MainBookScript._Animator.SetBool("Paper", true);
            }
        }
    }

    private void DeleteVillager(int VillagerID)
    {
        if (GameScript.Village.IsVillageAlive)
        {
            if (GameScript.Village.Villagers[VillagerID].Health == 0)
            {
                if (GameScript.Village.Villagers[VillagerID].Human) GameScript.Village.DestroyObject(GameScript.Village.Villagers[VillagerID].Human.GetComponent<HumanScript>().ID);
                GameScript.Village.Villagers.Remove(VillagerID);
                GameScript.Village.VillagerDeletedEvent?.Invoke(VillagerID);
                GameScript.Village.VillagersListChangedEvent?.Invoke();
                GameScript._MainBookScript.Close();
            }
        }
    }
}
