using UnityEngine;
using System;
using System.Collections;

public class FoundryScript : ProduceScript
{
    [Header("Плавильня")]
    public GameObject Smoke;
    public GameObject Coal;
    public GameObject IronIngot;
    public GameObject GoldIngot;
    public GameObject Shutter;
    public GameObject[] LiquidMetal;
    public Sprite[] CoalStates;
    public GameObject LiquidMetalSmokeEffect;
    public bool IsCoalInStorage { set { _CoalSpriteRenderer.sprite = CoalStates[value ? 0 : 1]; } }
    private ParticleSystem.MainModule _SmokeMainModule;
    private Animator _ShutterAnimator;
    private SpriteRenderer _CoalSpriteRenderer;
    private Animator[] _LiquidMetalAnimator;
    private bool _CoalIsTaken;
    private Coroutine _ShowCoalCountCoroutine;

    private bool ProducedItemInStove = false;

    private void ShowCoalCount()
    {
        if (_ShowCoalCountCoroutine != null) StopCoroutine(_ShowCoalCountCoroutine);
        _ShowCoalCountCoroutine = StartCoroutine(WorkerControll());
        IEnumerator WorkerControll()
        {
            yield return new WaitForEndOfFrame();
            if (_CoalIsTaken || !_CoalIsTaken && !ProduceItem) IsCoalInStorage = GameScript.Village.Items["Coal"] > 0;
        }
    }

    IEnumerator WaitForLiquidMetal()
    {
        while (ProduceProgress <= 50) yield return new WaitForEndOfFrame();
        for (int AnimatorID = 0; AnimatorID < LiquidMetal.Length; AnimatorID++) _LiquidMetalAnimator[AnimatorID].SetBool("Metal", true);
    }

    public override void Load(string SaveString)
    {
        _SmokeMainModule = Smoke.GetComponent<ParticleSystem>().main;
        _ShutterAnimator = Shutter.GetComponent<Animator>();
        _LiquidMetalAnimator = new Animator[LiquidMetal.Length];
        for (int AnimatorID = 0; AnimatorID < LiquidMetal.Length; AnimatorID++) _LiquidMetalAnimator[AnimatorID] = LiquidMetal[AnimatorID].GetComponent<Animator>();
        _CoalSpriteRenderer = Coal.GetComponent<SpriteRenderer>();

        if (!String.IsNullOrEmpty(SaveString))
        {
            SaveClass _Save = JsonUtility.FromJson<SaveClass>(SaveString);
            _Transform.position = new Vector3(_Save.Position, 0, _Transform.position.z);
            ProducedItemInStove = _Save.ProducedItemInStove;
            LoadBuild(_Save.BuildSave);
            LoadProduce(_Save.ProduceSave);
            if (ProduceItemState == ProduceItemStates.Produced && !ProducedItemInStove)
            {
                switch (ProduceItem.name)
                {
                    case "IronIngot": IronIngot.SetActive(true); break;
                    case "GoldIngot": GoldIngot.SetActive(true); break;
                }
            }
            else if (ProduceIsStarted && ProduceItemState != ProduceScript.ProduceItemStates.Produced)
            {
                _SmokeMainModule.prewarm = true;
                _SmokeMainModule.loop = true;
                Smoke.SetActive(true);
                _ShutterAnimator.SetBool("Working", true);
                _ShutterAnimator.Play("Working");
                StartCoroutine(WaitForLiquidMetal());
            }
        }
        else
        {
            LoadBuild(null);
            LoadProduce(null);
        }
        IsCoalInStorage = GameScript.Village.Items["Coal"] > 0;
        GameScript.Village.ItemsChangedEvent += ShowCoalCount;
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
        _CoalIsTaken = false;
        _ShutterAnimator.SetBool("Opened", false);
    }

    private void OnItemProduced()
    {
        for (int AnimatorID = 0; AnimatorID < LiquidMetal.Length; AnimatorID++) _LiquidMetalAnimator[AnimatorID].SetBool("Metal", false);
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
                    case "IronIngot":
                    case "GoldIngot":

                        FindWorkDetails(WorkName).IsStarted = true;
                        _CoalIsTaken = false;
                        if (WorkerActID == 0)
                        {
                            _WorkerScript.SetItem("Pouch");
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x - 0.6f, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            WorkerActID++;
                        }
                        if (WorkerActID >= 1 && WorkerActID < 4)
                        {
                            if (_WorkerScript._Transform.localScale.x < 0) _WorkerScript.Flip();
                            _WorkerScript._Animator.SetBool("Use_Pouch", true); _WorkerScript._Animator.speed = 1;
                            if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[1] == GameScript.FindGameItem("WorkClothes")) _WorkerScript._Animator.speed = 1.3f;
                            for (; WorkerActID - 1 < 3; WorkerActID++)
                            {
                                if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                _WorkerScript.SetItem("Coal", false);
                                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                _WorkerScript.SetItem("Pouch");
                                GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 5;
                            }
                            IsCoalInStorage = GameScript.Village.Items["Coal"] > 0;
                            _WorkerScript._Animator.SetBool("Use_Pouch", false);
                        }
                        _CoalIsTaken = true;
                        if (WorkerActID == 4)
                        {
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x + 1.2f, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            WorkerActID++;
                        }
                        if (WorkerActID >= 5 && WorkerActID < 11)
                        {
                            if (_WorkerScript._Transform.localScale.x < 0) _WorkerScript.Flip();
                            _ShutterAnimator.SetBool("Opened", true);
                            _WorkerScript._Animator.SetBool("Use_Pouch", true); _WorkerScript._Animator.speed = 1;
                            if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[1] == GameScript.FindGameItem("WorkClothes")) _WorkerScript._Animator.speed = 1.3f;
                            for (; WorkerActID - 5 < 5; WorkerActID++)
                            {
                                if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                _WorkerScript.SetItem("Pouch");
                                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                switch (ProduceItem.name)
                                {
                                    case "IronIngot": _WorkerScript.SetItem("IronOre", false); break;
                                    case "GoldIngot": _WorkerScript.SetItem("GoldOre", false); break;
                                }
                                GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 5;
                            }
                            _WorkerScript.SetItem("Pouch");
                            _WorkerScript._Animator.SetBool("Use_Pouch", false);
                            if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                            WorkerActID++;
                        }
                        if (WorkerActID == 11)
                        {
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x + 1.2f, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            if (_WorkerScript._Transform.localScale.x < 0) _WorkerScript.Flip();
                            _ShutterAnimator.SetBool("Opened", false);
                            _SmokeMainModule.prewarm = false;
                            _SmokeMainModule.loop = true;
                            _ShutterAnimator.SetBool("Working", true);
                            Smoke.SetActive(true);
                            if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[1] == GameScript.FindGameItem("WorkClothes")) ProduceSpeedCoefficient = 1.1f;
                            ProduceIsStarted = true;
                            ProducedItemInStove = true;
                            StartCoroutine(WaitForLiquidMetal());
                            WorkerActID++;
                        }
                        if (WorkerActID == 12)
                        {
                            while (!_WorkerScript.Run(new Vector2(_Transform.position.x + 1.2f, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            if (_WorkerScript._Transform.localScale.x < 0) _WorkerScript.Flip();
                            while (ProduceItemState != ProduceItemStates.Produced)
                            {
                                VillagersControllScript._VillagersControllScript.SkipAllTime();
                                yield return new WaitForEndOfFrame();
                            }
                            WorkerActID++;
                        }
                        if (WorkerActID == 13)
                        {
                            CoroutineInside = StartCoroutine(PutOnProducedItemFromTheStove());
                            while (CoroutineInside != null)
                            {
                                VillagersControllScript._VillagersControllScript.SkipAllTime();
                                yield return new WaitForEndOfFrame();
                            }
                            WorkerActID++;
                        }
                        if (WorkerActID == 14) _WorkerScript.StartReturn();
                        _CoalIsTaken = false;
                        break;
                }
            }
            else
            {
                switch (WorkName)
                {
                    case "IronIngot":
                    case "GoldIngot":
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
                            else while (!_WorkerScript.Run(new Vector2(_Transform.position.x, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                            _WorkerScript.SetItem("Pouch");
                            WorkerActID++;
                        }
                        if (WorkerActID == 1)
                        {
                            IronIngot.SetActive(false);
                            GoldIngot.SetActive(false);
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
                OnItemProduced();
                while (!_WorkerScript.Run(new Vector2(_Transform.position.x - 1, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                if (_WorkerScript._Transform.localScale.x > 0) _WorkerScript.Flip();
                _WorkerScript.SetItem("Poker"); _WorkerScript._Animator.speed = 0.5f;
                _WorkerScript._Animator.SetInteger("Use_Poker", 2);
                if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                switch (ProduceItem.name)
                {
                    case "IronIngot": _WorkerScript.SetItem("PokerIronIngot", false); break;
                    case "GoldIngot": _WorkerScript.SetItem("PokerGoldIngot", false); break;
                }
                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) Instantiate(LiquidMetalSmokeEffect, new Vector2(_Transform.position.x - 0.1f, _Transform.position.y + 0.3f), Quaternion.identity);
                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                _WorkerScript.SetItem("Poker");
                switch (ProduceItem.name)
                {
                    case "IronIngot": IronIngot.SetActive(true); break;
                    case "GoldIngot": GoldIngot.SetActive(true); break;
                }
                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 15;
                _WorkerScript._Animator.SetInteger("Use_Poker", 0);
                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                _WorkerScript.SetItem(null);
                ProducedItemInStove = false;
                CoroutineInside = null;
            }
        }
    }
}
