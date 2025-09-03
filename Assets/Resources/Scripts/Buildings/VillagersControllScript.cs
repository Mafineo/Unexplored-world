using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using System.IO;

public class VillagersControllScript : ObjectScript
{
    [HideInInspector] public static VillagersControllScript _VillagersControllScript;
    private float VillagersInsideTimer = 0;

    [HideInInspector] public Dictionary<int, float> VillagersSkippedTime = new Dictionary<int, float>();
    [HideInInspector] public int _SkipTimeCurrentVillagerID = -1;
    public int SkipTimeCurrentVillagerID
    {
        get { return _SkipTimeCurrentVillagerID; }
        set
        {
            _SkipTimeCurrentVillagerID = value;
            VillagersInsideTimerCycles = 0;
        }
    }
    [HideInInspector] public int VillagersInsideTimerCycles = 0;

    public bool SkipTime(float Seconds)
    {
        if (!IsTimeSkipped)
        {
            VillagersSkippedTime[SkipTimeCurrentVillagerID] += Seconds;
            CheckCycles();
        }
        return IsTimeSkipped;
        void CheckCycles()
        {
            float Temp = VillagersSkippedTime[SkipTimeCurrentVillagerID];
            int Cycles = 0;
            for (; Temp >= 6; Temp -= 6, Cycles++) ;
            if (VillagersInsideTimerCycles < Cycles)
            {
                VillagersInsideTimerCycles++;
                VillagersInsideTimerCycle(SkipTimeCurrentVillagerID);
                CheckCycles();
            }
        }
    }

    public void SkipAllTime() { for (; !IsTimeSkipped; VillagersSkippedTime[SkipTimeCurrentVillagerID] += 6) VillagersInsideTimerCycle(SkipTimeCurrentVillagerID); }

    public void SkipTimeForAllVillagers() 
    {
        foreach (KeyValuePair<int, GameScript.VillageClass.VillagerClass> Villager in GameScript.Village.Villagers)
        {
            SkipTimeCurrentVillagerID = Villager.Key;
            SkipAllTime();
        }
    }

    public bool IsTimeSkipped { get { return VillagersSkippedTime[SkipTimeCurrentVillagerID] >= GameScript.SkipTime; } }

    private void Update()
    {
        VillagersInsideTimer += Time.deltaTime;
        if (VillagersInsideTimer >= 6)
        {
            foreach (KeyValuePair<int, GameScript.VillageClass.VillagerClass> Villager in GameScript.Village.Villagers) VillagersInsideTimerCycle(Villager.Key);
            VillagersInsideTimer = 0;
        }
    }

    public void VillagersInsideTimerCycle(int VillagerID)
    {
        if (GameScript.Village.Villagers[VillagerID].Location == GameScript._EnviromentScript.Location)
        {
            GameScript.Village.Villagers[VillagerID].Satiety -= 6;
            if (!GameScript.Village.Villagers[VillagerID].IsOutside && GameScript.Village.Villagers[VillagerID].Health > 0 && GameScript.Village.Villagers[VillagerID].Satiety > 0) GameScript.Village.Villagers[VillagerID].Hit(1, DamageTypes.Sleeping);
            else if (GameScript.Village.Villagers[VillagerID].Satiety == 0) GameScript.Village.Villagers[VillagerID].Hit(1, DamageTypes.Starve);
        }
    }

    public void LoadVillagersControll(string SaveString)
    {
        _VillagersControllScript = GetComponent<VillagersControllScript>();
        foreach (KeyValuePair<int, GameScript.VillageClass.VillagerClass> Villager in GameScript.Village.Villagers) VillagersSkippedTime.Add(Villager.Key, 0);

        if (!String.IsNullOrEmpty(SaveString))
        {
            VillagersControllSaveClass _Save = JsonUtility.FromJson<VillagersControllSaveClass>(SaveString);
            VillagersInsideTimer = _Save.VillagersHealTimer;
            VillagersInsideTimer = (VillagersInsideTimer + GameScript.SkipTime) % 6;
            foreach (KeyValuePair<int, GameScript.VillageClass.VillagerClass> Villager in GameScript.Village.Villagers)
            {
                if(!String.IsNullOrEmpty(Villager.Value.Death) && Villager.Value.Location == GameScript._EnviromentScript.Location)
                {
                    GameScript.VillageClass.VillagerClass.SaveClass.DeathClass _Death = JsonUtility.FromJson<GameScript.VillageClass.VillagerClass.SaveClass.DeathClass>(Villager.Value.Death);
                    HumanScript _HumanScript = Villager.Value.GoOutside().GetComponent<HumanScript>();
                    _HumanScript.StartEnter(true);
                    _HumanScript._Transform.position = new Vector3(_Death.Position, 0, _HumanScript.transform.position.z);
                    if (!_Death.Rotation) _HumanScript.Flip();
                    _HumanScript.Die(Enum.IsDefined(typeof(DamageTypes), _Death.DamageType) ? (DamageTypes)Enum.Parse(typeof(DamageTypes), _Death.DamageType) : DamageTypes.Starve);
                }
            }
        }
    }

    public string SaveVillagersControll()
    {
        return JsonUtility.ToJson(new VillagersControllSaveClass(VillagersInsideTimer));
    }

    [Serializable]
    public class VillagersControllSaveClass
    {
        public float VillagersHealTimer;
        public VillagersControllSaveClass(float VillagersHealTimer)
        {
            this.VillagersHealTimer = VillagersHealTimer;
        }
    }
}
