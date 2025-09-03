using UnityEngine;
using UnityEngine.UI;

public class ConfirmPageScript : PageScript
{
    [HideInInspector] public ItemPictureScript[] _BigPictures;
    [HideInInspector] public Text _Description;

    public void Load()
    {
        _Description = transform.GetChild(0).GetComponent<Text>();
        _BigPictures = new ItemPictureScript[2];
        _BigPictures[0] = transform.GetChild(1).GetComponent<ItemPictureScript>();
        _BigPictures[1] = transform.GetChild(2).GetComponent<ItemPictureScript>();
    }
}