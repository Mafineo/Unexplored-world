using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;

public class CaveDescentScript : VillagersControllScript
{
    [Header("Веревка")]
    private UsePageScript ClimbPage;
    [HideInInspector] public List<GameObject> _RopeUsers = new List<GameObject>();
    public int FindRopeUserID(GameObject RopeUser)
    {
        for (int ID = 0; ID < _RopeUsers.Count; ID++) if (_RopeUsers[ID] == RopeUser) return ID;
        return -1;
    }
    [HideInInspector] public GameObject _RopeUser;

    private void UpdatePages()
    {
        InformationPage.PageIsHidden = false;
        ClimbPage.PageIsHidden = false;
    }

    public override void Load(string SaveString)
    {
        _SelectScript.OpenMainBookEvent += UpdatePages;
        ClimbPage = GameScript._MainBookScript.FindPage(GameScript._MainBookScript.FindPageID(PrefabName + "|Climb"), true).GetComponent<UsePageScript>();
        ClimbPage._BigPicture.Load(ItemPictureScript.PictureTypes.Button, Action: delegate { GameScript._EnviromentScript.StartLoadLocation("ValleyScene"); });

        if (!String.IsNullOrEmpty(SaveString))
        {
            SaveClass _Save = JsonUtility.FromJson<SaveClass>(SaveString);
            _Transform.position = new Vector3(_Save.Position, 0, _Transform.position.z);
            LoadVillagersControll(_Save.VillagersControll);
        }
        else LoadVillagersControll(null);
    }

    public override string Save() { return PrefabName + "|" + JsonUtility.ToJson(new SaveClass(transform.position.x, SaveVillagersControll())); }

    [Serializable]
    public class SaveClass
    {
        public float Position;
        public string VillagersControll;
        public SaveClass(float Position, string VillagersControll)
        {
            this.Position = Position;
            this.VillagersControll = VillagersControll;
        }
    }

    public bool SetRopeUser(GameObject RopeUser)
    {
        if (RopeUser && !_RopeUser || !RopeUser)
        {
            _RopeUser = RopeUser;
            return true;
        }
        else return false;
    }
}
