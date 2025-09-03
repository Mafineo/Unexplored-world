using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingsPageScript : PageScript
{
    [Header("Параметры")]
    public string PageName;
    public List<GameObject> Buildings = new List<GameObject>();

    private Text _PageName;
    private ItemPictureScript[] _Pictures;

    private void LoadBuildingPage(int ObjectID)
    {
        if (GameScript._MainBookScript.MainBookIsStatic)
        {
            GameScript._MainBookScript.HidePagesAfterClose();

            ObjectScript _ObjectScript = Buildings[ObjectID].GetComponent<ObjectScript>();
            if (_ObjectScript.PrefabName != "TownHall" && _ObjectScript.PrefabName != "Mine")
            {
                if (Buildings[ObjectID].TryGetComponent(out BuildScript BuildScript))
                {
                    InformationPageScript InformationPage = GameScript._MainBookScript.FindPage(GameScript._MainBookScript.FindPageID(_ObjectScript.PrefabName), true).GetComponent<InformationPageScript>();
                    InformationPage.PageIsHidden = false;
                    InterractPageScript BuildPage = GameScript._MainBookScript.FindPage(GameScript._MainBookScript.FindPageID("Build"), true).GetComponent<InterractPageScript>();
                    BuildScript.FillBuildPage(BuildPage);
                    BuildPage.PageIsHidden = false;
                    GameScript._MainBookScript.LoadPagesList();
                    GameScript._MainBookScript.TargetPage = _ObjectScript.PrefabName;
                }
            }
            else GameScript.Village.FindObject(_ObjectScript.PrefabName).GetComponent<SelectScript>().OpenObjectPage();
        }
    }

    public void Load()
    {
        _PageName = transform.GetChild(0).GetComponent<Text>();
        _Pictures = new ItemPictureScript[transform.GetChild(1).childCount];
        for (int PictureID = 0; PictureID < transform.GetChild(1).childCount; PictureID++) _Pictures[PictureID] = transform.GetChild(1).transform.GetChild(PictureID).GetComponent<ItemPictureScript>();

        _PageName.text = PageName;
        for (int ObjectID = 0; ObjectID < Buildings.Count; ObjectID++)
        {
            int Temp = ObjectID;
            _Pictures[ObjectID].Load(ItemPictureScript.PictureTypes.Button, Buildings[ObjectID].GetComponent<ObjectScript>().Icon, null, delegate { LoadBuildingPage(Temp); });
        }
    }
}
