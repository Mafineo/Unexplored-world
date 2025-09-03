using UnityEngine;
using System;

public class MineScript : BuildScript
{
    [Header("Шахта")]
    private UsePageScript GetDownPage;

    public override void UpdatePages()
    {
        if (BuildIsFinished)
        {
            InformationPage.PageIsHidden = false;
            GetDownPage.PageIsHidden = false;
        }
    }

    public override void Load(string SaveString)
    {
        _SelectScript.OpenMainBookEvent += UpdatePages;
        GetDownPage = GameScript._MainBookScript.FindPage(GameScript._MainBookScript.FindPageID(PrefabName + "|GetDown"), true).GetComponent<UsePageScript>();
        GetDownPage._BigPicture.Load(ItemPictureScript.PictureTypes.Button, Action: delegate { GameScript._EnviromentScript.StartLoadLocation("CaveScene"); });

        if (!String.IsNullOrEmpty(SaveString))
        {
            SaveClass _Save = JsonUtility.FromJson<SaveClass>(SaveString);
            _Transform.position = new Vector3(_Save.Position, 0, _Transform.position.z);
            LoadBuild(_Save.BuildSave);
        }
        else LoadBuild(null);
    }

    public override string Save() { return PrefabName + "|" + JsonUtility.ToJson(new SaveClass(transform.position.x, SaveBuild())); }

    [Serializable]
    public class SaveClass
    {
        public float Position;
        public string BuildSave;
        public SaveClass(float Position, string BuildSave)
        {
            this.Position = Position; 
            this.BuildSave = BuildSave;
        }
    }
}
