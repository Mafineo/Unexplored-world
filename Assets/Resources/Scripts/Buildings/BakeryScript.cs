using UnityEngine;
using System;
using System.Collections;

public class BakeryScript : ProduceScript
{
    [Header("Пекарня")]
    public GameObject Smoke;
    public GameObject Poker;
    public GameObject Dough;
    public GameObject Bread;
    public GameObject Shutter;

    private ParticleSystem.MainModule _SmokeMainModule;
    private Animator _ShutterAnimator;
    private bool ProducedItemInStove = false;

    public override void Load(string SaveString)
    {
        _SmokeMainModule = Smoke.GetComponent<ParticleSystem>().main;
        _ShutterAnimator = Shutter.GetComponent<Animator>();

        if (!String.IsNullOrEmpty(SaveString))
        {
            SaveClass _Save = JsonUtility.FromJson<SaveClass>(SaveString);
            _Transform.position = new Vector3(_Save.Position, 0, _Transform.position.z);
            ProducedItemInStove = _Save.ProducedItemInStove;
            LoadBuild(_Save.BuildSave);
            LoadProduce(_Save.ProduceSave);
            if (ProduceItemState == ProduceItemStates.Produced && !ProducedItemInStove) Bread.SetActive(true);
        }
        else
        {
            LoadBuild(null);
            LoadProduce(null);
        }
        ItemProducedEvent += OnItemProduced;
    }

    public override string Save() { return PrefabName + "|" + JsonUtility.ToJson(new SaveClass(transform.position.x, SaveBuild(), SaveProduce(), ProducedItemInStove)); }

    [Serializable]
    public class SaveClass
    {
        public float Position;
        public string BuildSave;
        public string ProduceSave;

        public bool ProducedItemInStove;
        public SaveClass(float Position, string BuildSave, string ProduceSave, bool ProducedItemInStove)
        {
            this.Position = Position;
            this.BuildSave = BuildSave;
            this.ProduceSave = ProduceSave;

            this.ProducedItemInStove = ProducedItemInStove;
        }
    }

    public override void ClearProduce()
    {
        Dough.SetActive(false);
        Poker.SetActive(true);
        _ShutterAnimator.SetBool("Opened", false);
    }

    private void OnItemProduced()
    {
        _SmokeMainModule.loop = false;
        _ShutterAnimator.SetBool("Working", false);
    }

    public override void StartWork(string WorkName, float WorkerPosition = 0)
    {
        if (PrepareToWork(WorkName, WorkerPosition)) WorkCoroutine = StartCoroutine(WorkerControll());
        IEnumerator WorkerControll()
        {
            VillagersControllScript._VillagersControllScript.SkipTimeCurrentVillagerID = _WorkerScript.VillagerID;
            Coroutine CoroutineInside = null;
            if (!CollectingIsStarted)
            {
                switch (WorkName)
                {
                    case "Bread":
                        FindWorkDetails(WorkName).IsStarted = true;
                        if (WorkerActID == 0)
                        {
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            WorkerActID++;
                        }
                        if (WorkerActID == 1)
                        {
                            _WorkerScript._Transform.position = new Vector3(_WorkerScript._Transform.position.x, _WorkerScript._Transform.position.y, _Transform.position.z + 0.22f);
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x + 0.7f, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            WorkerActID++;
                        }
                        if (WorkerActID >= 2 && WorkerActID < 12)
                        {
                            _WorkerScript._Transform.position = new Vector3(_WorkerScript._Transform.position.x, _WorkerScript._Transform.position.y, _Transform.position.z + 0.22f);
                            if (_WorkerScript._Transform.localScale.x < 0) _WorkerScript.Flip();
                            _WorkerScript.SetItem("Dough"); _WorkerScript._Animator.speed = 1;
                            if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[1] == GameScript.FindGameItem("WorkClothes")) _WorkerScript._Animator.speed = 1.2f;
                            _WorkerScript._Animator.SetBool("Work_Bakery_Dough", true);
                            if (WorkerActID < 6 && GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[1] == GameScript.FindGameItem("WorkClothes")) WorkerActID = 6;
                            for (; WorkerActID < 12; WorkerActID++)
                            {
                                if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 3;
                            }
                            Dough.SetActive(true);
                            _WorkerScript.SetItem(null);
                            _WorkerScript._Animator.SetBool("Work_Bakery_Dough", false);
                            if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                        }
                        if (WorkerActID == 12)
                        {
                            Dough.SetActive(true);
                            _WorkerScript._Transform.position = new Vector3(_WorkerScript._Transform.position.x, _WorkerScript._Transform.position.y, _Transform.position.z + 0.22f);
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            _WorkerScript.ID = _WorkerScript.ID;
                            WorkerActID++;
                        }
                        if (WorkerActID == 13)
                        {
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            if (_WorkerScript._Transform.localScale.x > 0) _WorkerScript.Flip();
                            Poker.SetActive(false);
                            _WorkerScript.SetItem("Poker"); _WorkerScript._Animator.speed = 0.5f;
                            _WorkerScript._Animator.SetInteger("Use_Poker", 1);
                            _ShutterAnimator.SetBool("Opened", true);
                            if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                            Dough.SetActive(false);
                            _WorkerScript.SetItem("PokerDough", false);
                            if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                            _WorkerScript.Flip();
                            if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                            _WorkerScript.SetItem("Poker");
                            if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                            GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 10;
                            if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[1] == GameScript.FindGameItem("WorkClothes")) ProduceSpeedCoefficient = 1.5f;
                            ProduceIsStarted = true;
                            ProducedItemInStove = true;
                            _WorkerScript._Animator.SetInteger("Use_Poker", 0);
                            if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                            _ShutterAnimator.SetBool("Opened", false);
                            _ShutterAnimator.SetBool("Working", true);
                            _SmokeMainModule.prewarm = false;
                            _SmokeMainModule.loop = true;
                            Smoke.SetActive(true);
                            WorkerActID++;
                            while (ProduceItemState != ProduceItemStates.Produced)
                            {
                                VillagersControllScript._VillagersControllScript.SkipAllTime();
                                yield return new WaitForEndOfFrame();
                            }
                            WorkerActID++;
                        }
                        if (WorkerActID == 14)
                        {
                            _WorkerScript.SetItem("Poker");
                            _ShutterAnimator.SetBool("Opened", false);
                            _ShutterAnimator.SetBool("Working", true);
                            _ShutterAnimator.Play("Working");
                            _SmokeMainModule.prewarm = true;
                            _SmokeMainModule.loop = true;
                            Smoke.SetActive(true);
                            while (ProduceItemState != ProduceItemStates.Produced)
                            {
                                VillagersControllScript._VillagersControllScript.SkipAllTime();
                                yield return new WaitForEndOfFrame();
                            }
                            WorkerActID++;
                        }
                        if (WorkerActID == 15)
                        {
                            CoroutineInside = StartCoroutine(PutOnProducedItemFromTheStove());
                            while (CoroutineInside != null)
                            {
                                VillagersControllScript._VillagersControllScript.SkipAllTime();
                                yield return new WaitForEndOfFrame();
                            }
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
                    case "Bread":
                        FindWorkDetails(WorkName).IsStarted = true;
                        if (WorkerActID == 0)
                        {
                            if (ProducedItemInStove)
                            {
                                CoroutineInside = StartCoroutine(PutOnProducedItemFromTheStove());
                                while (CoroutineInside != null)
                                {
                                    VillagersControllScript._VillagersControllScript.SkipAllTime();
                                    yield return new WaitForEndOfFrame();
                                }
                            }
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            WorkerActID++;
                        }
                        if (WorkerActID == 1)
                        {
                            Bread.SetActive(false);
                            CollectProducedItem();
                            GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 5;
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
            IEnumerator PutOnProducedItemFromTheStove()
            {
                switch (WorkName)
                {
                    case "Bread":
                        OnItemProduced();
                        while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                        if (_WorkerScript._Transform.localScale.x < 0) _WorkerScript.Flip();
                        _WorkerScript.SetItem("Poker"); _WorkerScript._Animator.speed = 0.5f;
                        _WorkerScript._Animator.SetInteger("Use_Poker", 2);
                        _ShutterAnimator.SetBool("Opened", true);
                        if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                        _WorkerScript.SetItem("PokerBread", false);
                        if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                        _WorkerScript.Flip();
                        if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                        _WorkerScript.SetItem("Poker");
                        Bread.SetActive(true);
                        if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                        GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 10;
                        _WorkerScript._Animator.SetInteger("Use_Poker", 0);
                        if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                        _ShutterAnimator.SetBool("Opened", false);
                        _WorkerScript.SetItem(null);
                        Poker.SetActive(true);
                        break;
                }
                ProducedItemInStove = false;
                CoroutineInside = null;
            }
        }
    }
}