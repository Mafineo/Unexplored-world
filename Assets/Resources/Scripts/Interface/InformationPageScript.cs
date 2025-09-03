using UnityEngine;
using UnityEngine.UI;

public class InformationPageScript : PageScript
{
    [Header("Параметры")]
    public string Name;
    public Sprite Icon;
    [TextArea] public string Description;

    private ItemPictureScript _Picture;
    private Text _Description;

    public void Load()
    {
        _Picture = transform.GetChild(0).GetComponent<ItemPictureScript>();
        _Description = transform.GetChild(1).GetComponent<Text>();

        _Picture.Load(ItemPictureScript.PictureTypes.Picture, Icon, Name);
        _Description.text = Description;
    }
}
