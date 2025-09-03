using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoragePageScript : PageScript
{
    [Header("Параметры")]
    public string PageName;
    public List<ItemScript> Resources = new List<ItemScript>();

    private Text _PageName;
    private ItemPictureScript[] _Pictures;

    public void Load()
    {
        _PageName = transform.GetChild(0).GetComponent<Text>();
        _Pictures = new ItemPictureScript[transform.GetChild(1).childCount];
        for (int PictureID = 0; PictureID < transform.GetChild(1).childCount; PictureID++) _Pictures[PictureID] = transform.GetChild(1).transform.GetChild(PictureID).GetComponent<ItemPictureScript>();

        _PageName.text = PageName;
        for (int ResourceID = 0; ResourceID < Resources.Count; ResourceID++)
        {
            string Temp = Resources[ResourceID].name;
            _Pictures[ResourceID].Load(ItemPictureScript.PictureTypes.Button, Resources[ResourceID].Icon, null, delegate { GameScript._MainBookScript.TargetPage = Temp + "|Item"; });
            _Pictures[ResourceID].SetTrackItem(Resources[ResourceID]);
        }
    }
}
