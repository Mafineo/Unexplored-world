using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class VillagersPageScript : PageScript
{
    [Header("Параметры")]
    public string PageName;

    private Text _PageName;
    private ItemPictureScript[] _Pictures;
    private ItemPictureScript[] _PicturesPlaces;
    [HideInInspector] public VillagerInformationPageScript _VillagerInformationPageScript;
    [HideInInspector] public VillagerEquipmentPageScript _VillagerEquipmentPageScript;
    private HumanScript _HumanScript;
    private Sprite TownHallIcon;
    private Sprite MineIcon;
    private int LastVillagerID = -1;

    private void LoadVillagerPage(int VillagerID)
    {
        if (GameScript._MainBookScript.MainBookIsStatic)
        {
            if (GameScript.Village.Villagers[VillagerID].Location == GameScript._EnviromentScript.Location || !GameScript.Village.Villagers[VillagerID].IsOutside)
            {
                LastVillagerID = VillagerID;
                GameScript._MainBookScript.HidePagesAfterClose();

                _VillagerInformationPageScript.LoadInformation(VillagerID);
                _VillagerInformationPageScript.PageIsHidden = false;
                _VillagerEquipmentPageScript.LoadInformation(VillagerID);
                _VillagerEquipmentPageScript.PageIsHidden = false;
                GameScript._MainBookScript.LoadPagesList();
                GameScript._MainBookScript.TargetPage = "VillagerInformation";
            }
        }
    }

    public void Load()
    {
        _PageName = transform.GetChild(0).GetComponent<Text>();
        _Pictures = new ItemPictureScript[transform.GetChild(1).childCount];
        _PicturesPlaces = new ItemPictureScript[transform.GetChild(1).childCount];
        for (int PictureID = 0; PictureID < transform.GetChild(1).childCount; PictureID++) 
        {
            _Pictures[PictureID] = transform.GetChild(1).transform.GetChild(PictureID).GetComponent<ItemPictureScript>();
            _PicturesPlaces[PictureID] = transform.GetChild(1).transform.GetChild(PictureID).transform.GetChild(3).GetComponent<ItemPictureScript>();
        }
        _HumanScript = GameScript.FindGamePrefab("Human").GetComponent<HumanScript>();
        TownHallIcon = GameScript.FindGamePrefab("TownHall").GetComponent<ObjectScript>().Icon;
        MineIcon = GameScript.FindGamePrefab("Mine").GetComponent<ObjectScript>().Icon;

        _PageName.text = PageName;
        GameScript.Village.VillagersListChangedEvent += UpdateVillagersList;
        GameScript.Village.VillagerDeletedEvent += (int VillagerID) => { LastVillagerID = -1; };
    }

    public void UpdateVillagersList()
    {
        int ObjectID = 0;
        foreach (KeyValuePair<int, GameScript.VillageClass.VillagerClass> Villager in GameScript.Village.Villagers)
        {
            int Temp = Villager.Key;
            _Pictures[ObjectID].Load(ItemPictureScript.PictureTypes.Button, _HumanScript.FindClothes((Villager.Value.Items[1]) ? Villager.Value.Items[1].name : "PeasantClothes").FindClothesPart("Head"), Action: delegate { LoadVillagerPage(Temp); });
            _Pictures[ObjectID].UpdateText(Villager.Value.Name, (Villager.Value.IsOutside && Villager.Value.Location == GameScript._EnviromentScript.Location) ? 0 : 1);
            if (Villager.Value.IsOutside)
            {
                if (Villager.Value.Location != GameScript._EnviromentScript.Location)
                {
                    switch (Villager.Value.Location)
                    {
                        case EnviromentScript.Locations.Valley: _PicturesPlaces[ObjectID].Load(ItemPictureScript.PictureTypes.Picture, TownHallIcon); break;
                        case EnviromentScript.Locations.Cave: _PicturesPlaces[ObjectID].Load(ItemPictureScript.PictureTypes.Picture, MineIcon); break;
                    }
                }
                else _PicturesPlaces[ObjectID].gameObject.SetActive(false);
            }
            else _PicturesPlaces[ObjectID].gameObject.SetActive(false);
            ObjectID++;
        }
        for (; ObjectID < _Pictures.Length; ObjectID++) _Pictures[ObjectID].gameObject.SetActive(false);
        if (_VillagerInformationPageScript.gameObject.activeSelf)
        {
            if (LastVillagerID != -1)
            {
                _VillagerInformationPageScript.LoadInformation(LastVillagerID);
                _VillagerEquipmentPageScript.LoadInformation(LastVillagerID);
            }
        }
    }
}
