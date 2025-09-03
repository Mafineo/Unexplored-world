using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class GardenBedScript : ProduceScript
{
    [Header("Грядка")]
    public GameObject DigEffect;
    public GameObject SeedsEffect;
    public List<FieldClass> Fields = new List<FieldClass>();
    private FieldClass _CurrentField;
    private const int PlantGrowStagesCount = 4;

    IEnumerator ShowProduceProgress()
    {
        _CurrentField = FindField(ProduceItem.name);
        while (ProduceItem && _CurrentField != null)
        {
            if (ProduceIsStarted || CollectingIsStarted)
            {
                for (int StageID = 0; StageID < PlantGrowStagesCount; StageID++)
                {
                    if (ProduceProgress >= 0 && ProduceProgress < 33) ShowGrowStage(0);
                    else if (ProduceProgress >= 30 && ProduceProgress < 60) ShowGrowStage(1);
                    else if (ProduceProgress >= 60 && ProduceProgress < 100) ShowGrowStage(2);
                    else if (ProduceProgress == 100) ShowGrowStage(3);
                }
                _CurrentField.Field.SetActive(true);
            }
            yield return new WaitForSeconds(5f);
        }
        void ShowGrowStage(int Stage)
        {
            for (int StageID = 0; StageID < PlantGrowStagesCount; StageID++)
            {
                if (StageID == Stage) _CurrentField.Stages[StageID].SetActive(true);
                else _CurrentField.Stages[StageID].SetActive(false);
            }
        }
    }

    private FieldClass FindField(string Name)
    {
        foreach (FieldClass Field in Fields) if (Field.Name == Name) return Field;
        return null;
    }

    public override void Load(string SaveString)
    {
        if (!String.IsNullOrEmpty(SaveString))
        {
            SaveClass _Save = JsonUtility.FromJson<SaveClass>(SaveString);
            _Transform.position = new Vector3(_Save.Position, 0, _Transform.position.z);
            LoadBuild(_Save.BuildSave);
            LoadProduce(_Save.ProduceSave);
            if (ProduceIsStarted || CollectingIsStarted) StartCoroutine(ShowProduceProgress());
        }
        else
        {
            LoadBuild(null);
            LoadProduce(null);
        }
        ItemProducedEvent += () =>
        {
            StopCoroutine(ShowProduceProgress());
            StartCoroutine(ShowProduceProgress());
        };
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
                    case "Wheat":
                        FindWorkDetails(WorkName).IsStarted = true;
                        GameObject Effect;
                        if (WorkerActID == 0)
                        {
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            WorkerActID++;
                        }
                        if (WorkerActID >= 1 && WorkerActID < 13)
                        {
                            for (int Side = (WorkerActID < 6) ? 1 : 2; Side <= 2; Side++)
                            {
                                while (!_WorkerScript.Run(new Vector2(_Transform.position.x + (0.8f - 1.6f * (Side - 1)), _Transform.position.y + 0.2f))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                                if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[0] == GameScript.FindGameItem("Shovel"))
                                {
                                    _WorkerScript.SetItem("Shovel");
                                    _WorkerScript._Animator.speed = 0.7f;
                                }
                                else
                                {
                                    _WorkerScript.SetItem(null);
                                    _WorkerScript._Animator.speed = 1;
                                }
                                if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[1] == GameScript.FindGameItem("FarmClothes")) _WorkerScript._Animator.speed = 1.2f;
                                if (_WorkerScript._Transform.localScale.x > 0 && Side == 1 || _WorkerScript._Transform.localScale.x < 0 && Side == 0) _WorkerScript.Flip();
                                if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[0] == GameScript.FindGameItem("Shovel"))
                                {
                                    _WorkerScript._Animator.SetBool("Use_Shovel", true);
                                    for (; WorkerActID - 1 < 6 * Side; WorkerActID += 3)
                                    {
                                        if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed))
                                        {
                                            for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                            Effect = Instantiate(DigEffect, new Vector2(_WorkerScript._Transform.position.x + ((_WorkerScript._Transform.localScale.x > 0) ? -0.35f : 0.35f), _WorkerScript._Transform.position.y), Quaternion.identity); 
                                            Effect.transform.rotation = (_WorkerScript._Transform.localScale.x > 0) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                                        }
                                        GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 5;
                                    }
                                    _WorkerScript._Animator.SetBool("Use_Shovel", false);
                                }
                                else
                                {
                                    _WorkerScript._Animator.SetBool("Dig", true);
                                    for (; WorkerActID - 1 < 6 * Side; WorkerActID++)
                                    {
                                        if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed))
                                        {
                                            for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                            Effect = Instantiate(DigEffect, new Vector2(_WorkerScript._Transform.position.x + ((_WorkerScript._Transform.localScale.x > 0) ? -0.35f : 0.35f), _WorkerScript._Transform.position.y), Quaternion.identity); 
                                            Effect.transform.rotation = (_WorkerScript._Transform.localScale.x > 0) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                                        }
                                        GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 3;
                                        if (_WorkerScript.Health >= 20) _WorkerScript.Hit(1, DamageTypes.Hit);
                                    }
                                    _WorkerScript._Animator.SetBool("Dig", false);
                                }
                                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                            }
                        }
                        if (WorkerActID >= 13 && WorkerActID < 15)
                        {
                            _WorkerScript.SetItem("Pouch");
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y + 0.2f))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            for (; WorkerActID - 13 < 2; WorkerActID++)
                            {
                                if (_WorkerScript._Transform.localScale.x > 0 && WorkerActID - 13 == 1 || _WorkerScript._Transform.localScale.x < 0 && WorkerActID - 13 == 0) _WorkerScript.Flip();
                                _WorkerScript._Animator.SetBool("Sow", true);
                                if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed))
                                {
                                    for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                    Effect = Instantiate(SeedsEffect, _WorkerScript.FindBodyPart("LeftArm").Part.transform.GetChild(0).transform.position, Quaternion.identity);
                                    Effect.transform.rotation = (_WorkerScript._Transform.localScale.x > 0) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                                }
                                GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 5;
                                _WorkerScript._Animator.SetBool("Sow", false);
                                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                            }
                            if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[0] == GameScript.FindGameItem("Shovel")) ProduceSpeedCoefficient = 1.5f;
                            ProduceIsStarted = true;
                            StartCoroutine(ShowProduceProgress());
                            _WorkerScript.SetItem(null);
                        }
                        if (WorkerActID == 15)
                        {
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            WorkerActID++;
                        }
                        if (WorkerActID == 16) _WorkerScript.StartReturn();
                        break;
                }
            }
            else
            {
                switch (WorkName)
                {
                    case "Wheat":
                        FindWorkDetails(WorkName).IsStarted = true;
                        GameObject Effect;
                        if (WorkerActID == 0)
                        {
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            WorkerActID++;
                        }
                        if (WorkerActID >= 1 && WorkerActID < 13)
                        {
                            for (int Side = (WorkerActID < 6) ? 1 : 2; Side <= 2; Side++)
                            {
                                while (!_WorkerScript.Run(new Vector2(_Transform.position.x + (0.8f - 1.6f * (Side - 1)), _Transform.position.y + 0.2f))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                                if (_WorkerScript._Transform.localScale.x > 0 && Side == 1 || _WorkerScript._Transform.localScale.x < 0 && Side == 0) _WorkerScript.Flip();
                                if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[0] == GameScript.FindGameItem("Hoe"))
                                {
                                    ProduceCountCoefficient = 1.5f;
                                    _WorkerScript.SetItem("Hoe");
                                    if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[1] == GameScript.FindGameItem("FarmClothes")) _WorkerScript._Animator.speed = 1f;
                                    else _WorkerScript._Animator.speed = 0.8f;
                                    _WorkerScript._Animator.SetBool("Use_Hoe", true);
                                    for (; WorkerActID - 1 < 6 * Side; WorkerActID += 3)
                                    {
                                        if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed))
                                        {
                                            for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                            Effect = Instantiate(DigEffect, new Vector2(_WorkerScript._Transform.position.x + ((_WorkerScript._Transform.localScale.x > 0) ? -0.35f : 0.35f), _WorkerScript._Transform.position.y), Quaternion.identity);
                                            Effect.transform.rotation = (_WorkerScript._Transform.localScale.x > 0) ? Quaternion.Euler(0, 180, 0) : Quaternion.Euler(0, 0, 0);
                                        }
                                        GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 5;
                                    }
                                    _WorkerScript._Animator.SetBool("Use_Hoe", false);
                                }
                                else
                                {
                                    _WorkerScript.SetItem(null);
                                    if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[1] == GameScript.FindGameItem("FarmClothes")) _WorkerScript._Animator.speed = 1.2f;
                                    else _WorkerScript._Animator.speed = 1f;
                                    _WorkerScript._Animator.SetBool("Dig", true);
                                    for (; WorkerActID - 1 < 6 * Side; WorkerActID++) 
                                    {
                                        if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed))
                                        {     
                                            for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                            Effect = Instantiate(DigEffect, new Vector2(_WorkerScript._Transform.position.x + ((_WorkerScript._Transform.localScale.x > 0) ? -0.35f : 0.35f), _WorkerScript._Transform.position.y), Quaternion.identity);
                                            Effect.transform.rotation = (_WorkerScript._Transform.localScale.x > 0) ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, 180, 0);
                                        }
                                        GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 3;
                                        if (_WorkerScript.Health >= 20) _WorkerScript.Hit(1, DamageTypes.Hit);
    }
                                    _WorkerScript._Animator.SetBool("Dig", false);
                                }
                                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                            }
                            CollectProducedItem();
                            HideProduceProgress();
                        } 
                        if (WorkerActID == 13)
                        {
                            _WorkerScript.SetItem("Pouch");
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            WorkerActID++;
                        }
                        if (WorkerActID == 14)
                        {
                            _WorkerScript.SetItem("Pouch");
                            _WorkerScript.StartReturn();
                        }
                        break;
                }
            }
        }
        void HideProduceProgress()
        {
            _CurrentField.Field.SetActive(false);
            for (int StageID = 0; StageID < _CurrentField.Stages.Count; StageID++) _CurrentField.Stages[StageID].SetActive(false);
            _CurrentField = null;
        }
    }

    [Serializable]
    public class FieldClass
    {
        public string Name;
        public GameObject Field;
        public List<GameObject> Stages = new List<GameObject>();
    }
}
