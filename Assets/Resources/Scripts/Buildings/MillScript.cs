using UnityEngine;
using System;
using System.Collections;

public class MillScript : ProduceScript
{
    [Header("Мельница")]
    public GameObject Blades;
    private Transform _BladesTransform;

    public override void UpdateAct() { if (BuildIsFinished && Blades) _BladesTransform.Rotate(new Vector3(0, 0, 90) * GameScript._EnviromentScript.WindSpeed * Time.deltaTime); }

    public override void Load(string SaveString)
    {
        if (Blades) _BladesTransform = Blades.GetComponent<Transform>(); 

        if (!String.IsNullOrEmpty(SaveString))
        {
            SaveClass _Save = JsonUtility.FromJson<SaveClass>(SaveString);
            _Transform.position = new Vector3(_Save.Position, 0, _Transform.position.z);
            LoadBuild(_Save.BuildSave);
            LoadProduce(_Save.ProduceSave);
        }
        else
        {
            LoadBuild(null);
            LoadProduce(null);
        }
    }

    public override string Save() { return PrefabName + "|" + JsonUtility.ToJson(new SaveClass(transform.position.x, SaveBuild(), SaveProduce())); }

    [Serializable]
    public class SaveClass
    {
        public float Position;
        public string BuildSave;
        public string ProduceSave;
        public SaveClass(float Position, string BuildSave, string ProduceSave)
        {
            this.Position = Position;
            this.BuildSave = BuildSave;
            this.ProduceSave = ProduceSave;
        }
    }

    public override void StartWork(string WorkName, float WorkerPosition = 0)
    {
        if (PrepareToWork(WorkName, WorkerPosition)) WorkCoroutine = StartCoroutine(WorkerControll());
        IEnumerator WorkerControll()
        {
            VillagersControllScript._VillagersControllScript.SkipTimeCurrentVillagerID = _WorkerScript.VillagerID;
            if (!CollectingIsStarted)
            {
                switch (WorkName)
                {
                    case "Flour":
                        FindWorkDetails(WorkName).IsStarted = true;
                        if (WorkerActID == 0)
                        {
                            _WorkerScript.SetItem("Pouch");
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            WorkerActID++;
                        }
                        if (WorkerActID == 1)
                        {
                            _WorkerScript._Transform.localScale = new Vector3(0, 0, 0);
                            if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) yield return new WaitForSeconds(1f);
                            GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 12;
                            ProduceIsStarted = true;
                            _WorkerScript.SetItem(null);
                            _WorkerScript._Transform.localScale = new Vector3(1, 1, 1);
                            WorkerActID++;
                        }
                        if (WorkerActID == 2) _WorkerScript.StartReturn();
                        break;
                }
            }
            else
            {
                switch (WorkName)
                {
                    case "Flour":
                        FindWorkDetails(WorkName).IsStarted = true;
                        if (WorkerActID == 0)
                        {
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            WorkerActID++;
                        }
                        if (WorkerActID == 1)
                        {
                            _WorkerScript._Transform.localScale = new Vector3(0, 0, 0);
                            if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) yield return new WaitForSeconds(1f);
                            CollectProducedItem();
                            GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 12;
                            _WorkerScript._Transform.localScale = new Vector3(1, 1, 1);
                            WorkerActID++;
                        }
                        if (WorkerActID == 2)
                        {
                            _WorkerScript.SetItem("Pouch");
                            _WorkerScript.StartReturn();
                        }
                        break;
                }
            }
        }
    }
}
