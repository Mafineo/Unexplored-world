using System.Collections.Generic;
using UnityEngine;
using System;

public class TownHallScript : VillagersControllScript
{
    [Header("Ратуша")]
    public GameObject Smoke;
    private ParticleSystem.MainModule _SmokeMainModule;

    private void UpdatePages() => InformationPage.PageIsHidden = false;

    public override void Load(string SaveString)
    {
        _SelectScript.OpenMainBookEvent += UpdatePages;
        Smoke.SetActive(true);
        _SmokeMainModule = Smoke.GetComponent<ParticleSystem>().main;
        GameScript.Village.VillagerDeletedEvent += (int VillagerID) => { if (!GameScript.Village.IsVillageAlive) _SmokeMainModule.loop = false; };

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
}