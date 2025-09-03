using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BushScript : ObstacleScript
{
    [Header("Куст")]
    public GameObject Bush;
    public GameObject[] Fruits;
    public GameObject[] GrowMasks;
    public GameObject ChopEffect;

    public override float ResoucesCount
    {
        get { return _ResoucesCount; }
        set
        {
            if (value >= _ResoucesCount)
            {
                _ResoucesCount = value;
                if (_ResoucesCount >= 100)
                {
                    _ResoucesCount = 100;
                    if (IsSelected)
                    {
                        FindWorkDetails("PickBerries").WorkPage._BigPictures[0].UpdateText("Собирать");
                        for (int SlotID = 0; SlotID < FindWorkDetails("PickBerries").Reward.Items.Count; SlotID++)
                        {
                            ItemScript Temp = FindWorkDetails("PickBerries").Reward.Items[SlotID].Item;
                            FindWorkDetails("PickBerries").WorkPage._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, FindWorkDetails("PickBerries").Reward.FindItem(Temp).Count.ToString(), delegate { GameScript._MainBookScript.TargetPage = Temp.name + "|Item"; });
                        }
                    }
                }
                else if (IsSelected) FindWorkDetails("PickBerries").WorkPage._BigPictures[0].UpdateText(Mathf.CeilToInt(_ResoucesCount) + "%", _ResoucesCount / 100);
            }
            else
            {
                _ResoucesCount = value;
                if (_ResoucesCount <= 55 && FindWorkDetails("PickBerries").IsStarted) for (int ItemID = 0; ItemID < FindWorkDetails("PickBerries").Reward.Items.Count; ItemID++) GameScript.Village.GiveItem(FindWorkDetails("PickBerries").Reward.Items[ItemID].Item, FindWorkDetails("PickBerries").Reward.Items[ItemID].Count);
                else
                {
                    if (_ResoucesCount <= 5)
                    {
                        _ResoucesCount = 0;
                        for (int ItemID = 0; ItemID < FindWorkDetails("PickFiber").Reward.Items.Count; ItemID++) GameScript.Village.GiveItem(FindWorkDetails("PickFiber").Reward.Items[ItemID].Item, FindWorkDetails("PickFiber").Reward.Items[ItemID].Count);
                    }
                }
            }
            if (FindWorkDetails("PickBerries").IsStarted && _WorkerScript && !_WorkerScript.IsReturning && _ResoucesCount > 55) SetFruitState(100);
            else SetFruitState(_ResoucesCount);
            void SetFruitState(float Stage)
            {
                if (Stage > 70)
                {
                    for (int FruitID = 0; FruitID < Fruits.Length; FruitID++)
                    {
                        _FruitsSprites[FruitID].color = new Color(_FruitsSprites[FruitID].color.r, (255f - 195f * Stage / 100) / 255f, _FruitsSprites[FruitID].color.b);
                        _FruitsTransforms[FruitID].localScale = new Vector3(Stage / 100, Stage / 100, Stage / 100);
                    }
                    for (int GrowMaskID = 0; GrowMaskID < GrowMasks.Length; GrowMaskID++) _GrowMasksTransforms[GrowMaskID].localScale = new Vector3(0.6f, 0.6f, 0.6f);
                }
                else if(Stage > 50)
                {
                    for (int FruitID = 0; FruitID < Fruits.Length; FruitID++) _FruitsTransforms[FruitID].localScale = Vector3.zero;
                    for (int GrowMaskID = 0; GrowMaskID < GrowMasks.Length; GrowMaskID++) _GrowMasksTransforms[GrowMaskID].localScale = new Vector3(0.6f, 0.6f, 0.6f);
                }
                else if (Stage <= 50)
                {
                    for (int FruitID = 0; FruitID < Fruits.Length; FruitID++) _FruitsTransforms[FruitID].localScale = Vector3.zero;
                    for (int GrowMaskID = 0; GrowMaskID < GrowMasks.Length; GrowMaskID++) _GrowMasksTransforms[GrowMaskID].localScale = new Vector3(0.6f * Stage / 50, 0.6f * Stage / 50, 0.6f * Stage / 50);
                }
            }
        }
    }

    private List<SpriteRenderer> _FruitsSprites = new List<SpriteRenderer>();
    private List<Transform> _FruitsTransforms = new List<Transform>();
    private List<Transform> _GrowMasksTransforms = new List<Transform>();

    private void Awake()
    {
        foreach (GameObject Fruit in Fruits)
        {
            _FruitsSprites.Add(Fruit.GetComponent<SpriteRenderer>());
            _FruitsTransforms.Add(Fruit.GetComponent<Transform>());
        }
        foreach (GameObject GrowMask in GrowMasks) _GrowMasksTransforms.Add(GrowMask.GetComponent<Transform>());
    }

    private void StartPick()
    {
        if (ResoucesCount == 100) StartWork("PickBerries");
        else if (ResoucesCount > 50) StartWork("PickFiber");
    }

    public override void UpdatePages()
    {
        InformationPage.PageIsHidden = false;
        if (ResoucesCount == 100)
        {
            for (int SlotID = 0; SlotID < FindWorkDetails("PickBerries").Reward.Items.Count; SlotID++)
            {
                ItemScript Temp = FindWorkDetails("PickBerries").Reward.Items[SlotID].Item;
                FindWorkDetails("PickBerries").WorkPage._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, FindWorkDetails("PickBerries").Reward.FindItem(Temp).Count.ToString(), delegate { GameScript._MainBookScript.TargetPage = Temp.name + "|Item"; });
            }
        }
        else
        {
            for (int SlotID = 0; SlotID < FindWorkDetails("PickFiber").Reward.Items.Count; SlotID++)
            {
                ItemScript Temp = FindWorkDetails("PickFiber").Reward.Items[SlotID].Item;
                FindWorkDetails("PickBerries").WorkPage._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, FindWorkDetails("PickFiber").Reward.FindItem(Temp).Count.ToString(), delegate { GameScript._MainBookScript.TargetPage = Temp.name + "|Item"; });
            }
        }
        FindWorkDetails("PickBerries").WorkPage._BigPictures[0].Load(ItemPictureScript.PictureTypes.Button, Action: delegate { StartPick(); });
        FindWorkDetails("PickBerries").WorkPage.PageIsHidden = false;

        for (int SlotID = 0; SlotID < FindWorkDetails("Chop").Reward.Items.Count; SlotID++)
        {
            ItemScript Temp = FindWorkDetails("Chop").Reward.Items[SlotID].Item;
            FindWorkDetails("Chop").WorkPage._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, FindWorkDetails("Chop").Reward.FindItem(Temp).Count.ToString(), delegate { GameScript._MainBookScript.TargetPage = Temp.name + "|Item"; });
        }
        FindWorkDetails("Chop").WorkPage._BigPictures[0].Load(ItemPictureScript.PictureTypes.Button, Text: "Рубить", Action: delegate { StartWork("Chop"); });
        FindWorkDetails("Chop").WorkPage.PageIsHidden = false;
        ResoucesCount = ResoucesCount;

        GameScript._MainBookScript._Paper.WorkPages = new GameObject[0];
        if (Health > 0)
        {
            GameScript._MainBookScript._Paper.ClearPaper();
            string[] Temp = { PrefabName + "|PickBerries", PrefabName + "|Chop" };
            GameScript._MainBookScript._Paper._Pictures[0].Load(ItemPictureScript.PictureTypes.Button, FindWorkDetails("PickBerries").WorkPage._BigPictures[0].Icon, null, delegate { GameScript._MainBookScript.TargetPage = Temp[0]; });
            GameScript._MainBookScript._Paper._Pictures[1].Load(ItemPictureScript.PictureTypes.Button, FindWorkDetails("Chop").WorkPage._BigPictures[0].Icon, null, delegate { GameScript._MainBookScript.TargetPage = Temp[1]; });
            GameScript._MainBookScript._Paper.SetContentSize();
            GameScript._MainBookScript._Paper._EnabledPicturesCount = 2;
            GameScript._MainBookScript._Paper.WorkPages = new GameObject[2] { FindWorkDetails("PickBerries").WorkPage.gameObject, FindWorkDetails("Chop").WorkPage.gameObject };
            GameScript._MainBookScript._Paper.Name = "Работа";
            GameScript._MainBookScript._Animator.SetBool("Paper", true);
        }

        UpdateWorkerButton();
    }

    IEnumerator WaitEndOfChop()
    {
        _BoxCollider2D.enabled = false;
        while (!_Animator.GetCurrentAnimatorStateInfo(0).IsName("Chopped") || _WorkerScript) yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }

    public override void Load(string SaveString)
    {
        if (!String.IsNullOrEmpty(SaveString))
        {
            SaveClass _Save = JsonUtility.FromJson<SaveClass>(SaveString);
            _Transform.position = new Vector3(_Save.Position, 0, _Transform.position.z);
            Health = _Save.Health;
            ResoucesCount = _Save.ResoucesCount;
            LoadObstacle(_Save.ObstacleSave);
            if (Health == 0)
            {
                _Animator.SetBool("Chopped", true);
                _Animator.Play("Chopped");
                StartCoroutine(WaitEndOfChop());
            }
        }
        else
        {
            Health = MaxHealth;
            ResoucesCount = 100;
            LoadObstacle(null);
        }
        ResoucesCount = ResoucesCount;
    }

    public override string Save() { return PrefabName + "|" + JsonUtility.ToJson(new SaveClass(_Transform.position.x, Health, ResoucesCount, SaveObstacle())); }

    [Serializable]
    public class SaveClass
    {
        public float Position;
        public int Health;
        public float ResoucesCount;
        public string ObstacleSave;
        public SaveClass(float Position, int Health, float ResoucesCount, string ObstacleSave)
        {
            this.Position = Position;
            this.Health = Health;
            this.ResoucesCount = ResoucesCount;
            this.ObstacleSave = ObstacleSave;
        }
    }

    public override void StartWork(string WorkName, float WorkerPosition = 0)
    {
        if (WorkName == "PickBerries" && ResoucesCount == 100 || WorkName == "PickFiber" && ResoucesCount > 50 || WorkName == "Chop" && Health > 0 || GameScript.Loading) if (PrepareToWork(WorkName, WorkerPosition)) WorkCoroutine = StartCoroutine(WorkerControll());
        IEnumerator WorkerControll()
        {
            switch (WorkName)
            {
                case "PickFiber":
                case "PickBerries":
                    _SelectScript.SelectIsEnabled = false;
                    GameScript._MainBookScript.Close();
                    FindWorkDetails(WorkName).IsStarted = true;
                    VillagersControllScript._VillagersControllScript.SkipTimeCurrentVillagerID = _WorkerScript.VillagerID;
                    if (WorkerActID == 0)
                    {
                        while (!_WorkerScript.Run(new Vector2(_Transform.position.x - 0.4f, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                        WorkerActID++;
                    }
                    if (WorkerActID == 1)
                    {
                        if (_WorkerScript._Transform.localScale.x > 0) _WorkerScript.Flip();
                        _WorkerScript.SetItem("Pouch"); _Animator.speed = 0.7f;
                        _WorkerScript._Animator.SetBool("Use_Pouch", true);
                        if (WorkName == "PickBerries")
                        {
                            for (; ResoucesCount > 55;)
                            {
                                if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                _WorkerScript.SetItem("Berry", false);
                                Pick();
                                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                _WorkerScript.SetItem("Pouch");
                                GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 3;
                            }
                        }
                        else
                        {
                            for (; ResoucesCount > 5;)
                            {
                                if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                _WorkerScript.SetItem("Fiber", false);
                                Pick();
                                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                _WorkerScript.SetItem("Pouch");
                                GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 3;
                            }
                        }
                        WorkerActID++;
                        _WorkerScript._Animator.SetBool("Use_Pouch", false);
                        _WorkerScript._Animator.speed = 1;
                        if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                    }
                    if(WorkerActID == 2) _WorkerScript.StartReturn();
                    break;
                case "Chop":
                    _SelectScript.SelectIsEnabled = false;
                    GameScript._MainBookScript.Close();
                    FindWorkDetails("Chop").IsStarted = true;
                    if (WorkerActID == 0)
                    {
                        while (!_WorkerScript.Run(new Vector2(_Transform.position.x - 0.6f, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                        WorkerActID++;
                    }
                    if (WorkerActID == 1)
                    {
                        if (_WorkerScript._Transform.localScale.x > 0) _WorkerScript.Flip();
                        _WorkerScript.SetItem("Axe"); _WorkerScript._Animator.speed = 0.9f;
                        _WorkerScript._Animator.SetBool("Attack", true);
                        for (; Health > 0;)
                        {
                            if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                            Chop(100);
                            GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 8;
                        }
                        WorkerActID++;
                        _WorkerScript._Animator.SetBool("Attack", false);
                        if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                        _WorkerScript.SetItem(null); _WorkerScript._Animator.speed = 1f;
                    }
                    if (WorkerActID == 2) _WorkerScript.StartReturn();
                    break;
            }
            _SelectScript.SelectIsEnabled = true;
        }
        void Pick() => ResoucesCount -= 10;
        void Chop(int Damage)
        {
            Health -= Damage;
            _Animator.SetTrigger("Hit");
            Instantiate(ChopEffect, new Vector3(_Transform.position.x, _Transform.position.y + 0.3f), Quaternion.identity);
            if (Health == 0)
            {
                for (int ItemID = 0; ItemID < FindWorkDetails("Chop").Reward.Items.Count; ItemID++) GameScript.Village.GiveItem(FindWorkDetails("Chop").Reward.Items[ItemID].Item, FindWorkDetails("Chop").Reward.Items[ItemID].Count);
                _Animator.SetBool("Chopped", true);
                StartCoroutine(WaitEndOfChop());
            }
        }
    }
}
