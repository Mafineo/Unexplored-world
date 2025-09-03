using UnityEngine;
using UnityEngine.UI;

public class UsePageScript : PageScript
{
    [Header("Параметры")]
    [TextArea] public string Description;

    private Text _Description;
    [HideInInspector] public ItemPictureScript _BigPicture;

    public void Load()
    {
        _Description = transform.GetChild(0).GetComponent<Text>();
        _BigPicture = transform.GetChild(1).GetComponent<ItemPictureScript>();

        _Description.text = Description;
        _Description.enabled = true;
    }
}