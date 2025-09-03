using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class TreeScript : ObstacleScript
{
    [Header("Дерево")]
    public GameObject Tree;
    public GameObject[] Fruits;
    public GameObject ShackeEffect;
    public GameObject ResourcesShackeEffect;
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
                        FindWorkDetails("Shacke").WorkPage._BigPictures[0].UpdateText("Трясти");
                        if (FindWorkDetails("Chop").WorkPage.gameObject.activeSelf)
                        {
                            for (int SlotID = 0; SlotID < FindWorkDetails("Chop").Reward.Items.Count; SlotID++)
                            {
                                ItemScript Temp = FindWorkDetails("Chop").Reward.Items[SlotID].Item;
                                FindWorkDetails("Chop").WorkPage._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, (FindWorkDetails("Chop").Reward.FindItem(Temp).Count + FindWorkDetails("Shacke").Reward.FindItem(Temp).Count).ToString());
                            }
                        }
                    }
                }
                else if (IsSelected) FindWorkDetails("Shacke").WorkPage._BigPictures[0].UpdateText(Mathf.CeilToInt(_ResoucesCount) + "%", _ResoucesCount / 100);
            }
            else
            {
                if (_ResoucesCount == 100 && IsSelected)
                {
                    for (int SlotID = 0; SlotID < FindWorkDetails("Chop").Reward.Items.Count; SlotID++)
                    {
                        ItemScript Temp = FindWorkDetails("Chop").Reward.Items[SlotID].Item;
                        FindWorkDetails("Chop").WorkPage._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, FindWorkDetails("Chop").Reward.FindItem(Temp).Count.ToString());
                    }
                }
                _ResoucesCount = value;
                if (_ResoucesCount <= 5)
                {
                    for (int ItemID = 0; ItemID < FindWorkDetails("Shacke").Reward.Items.Count; ItemID++) GameScript.Village.GiveItem(FindWorkDetails("Shacke").Reward.Items[ItemID].Item, FindWorkDetails("Shacke").Reward.Items[ItemID].Count);
                    _ResoucesCount = 0;
                    for (int FruitID = 0; FruitID < Fruits.Length; FruitID++) _FruitsTransforms[FruitID].localScale = Vector3.zero;
                    _SelectScript.SelectIsEnabled = true;
                }
            }
            if (FindWorkDetails("Shacke").IsStarted && _WorkerScript && !_WorkerScript.IsReturning && _ResoucesCount > 5) SetFruitState(100);
            else SetFruitState(_ResoucesCount);
            void SetFruitState(float Stage)
            {
                if (Stage > 50)
                {
                    for (int FruitID = 0; FruitID < Fruits.Length; FruitID++)
                    {
                        _FruitsSprites[FruitID].color = new Color(_FruitsSprites[FruitID].color.r, (255f - 215f * Stage / 100) / 255f, _FruitsSprites[FruitID].color.b);
                        _FruitsTransforms[FruitID].localScale = new Vector3(Stage / 100, Stage / 100, Stage / 100);
                    }
                }
                else for (int FruitID = 0; FruitID < Fruits.Length; FruitID++) _FruitsTransforms[FruitID].localScale = Vector3.zero;
            }
        }
    }

    private List<Transform> _FruitsTransforms = new List<Transform>();
    private List<SpriteRenderer> _FruitsSprites = new List<SpriteRenderer>();

    private void Awake()
    {
        foreach (GameObject Fruit in Fruits)
        {
            _FruitsTransforms.Add(Fruit.GetComponent<Transform>());
            _FruitsSprites.Add(Fruit.GetComponent<SpriteRenderer>());
        }
    }

    public override void UpdatePages()
    {
        InformationPage.PageIsHidden = false;
        if (Health > 0)
        {
            for (int SlotID = 0; SlotID < FindWorkDetails("Shacke").Reward.Items.Count; SlotID++)
            {
                ItemScript Temp = FindWorkDetails("Shacke").Reward.Items[SlotID].Item;
                FindWorkDetails("Shacke").WorkPage._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, FindWorkDetails("Shacke").Reward.FindItem(Temp).Count.ToString(), delegate { GameScript._MainBookScript.TargetPage = Temp.name + "|Item"; });
            }
            FindWorkDetails("Shacke").WorkPage._BigPictures[0].Load(ItemPictureScript.PictureTypes.Button, Action: delegate { StartWork("Shacke"); });
            FindWorkDetails("Shacke").WorkPage.PageIsHidden = false;

            for (int SlotID = 0; SlotID < FindWorkDetails("Chop").Reward.Items.Count; SlotID++)
            {
                ItemScript Temp = FindWorkDetails("Chop").Reward.Items[SlotID].Item;
                FindWorkDetails("Chop").WorkPage._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, (FindWorkDetails("Chop").Reward.FindItem(Temp).Count + ((FindWorkDetails("Shacke").Reward.FindItem(Temp) != null && ResoucesCount == 100) ? FindWorkDetails("Shacke").Reward.FindItem(Temp).Count : 0)).ToString(), delegate { GameScript._MainBookScript.TargetPage = Temp.name + "|Item"; });
            }
            FindWorkDetails("Chop").WorkPage._BigPictures[0].Load(ItemPictureScript.PictureTypes.Button, Text: "Рубить", Action: delegate { StartWork("Chop"); });
            FindWorkDetails("Chop").WorkPage.PageIsHidden = false;
            ResoucesCount = ResoucesCount;
        }
        else
        {
            for (int SlotID = 0; SlotID < FindWorkDetails("Uproot").Reward.Items.Count; SlotID++)
            {
                ItemScript Temp = FindWorkDetails("Uproot").Reward.Items[SlotID].Item;
                FindWorkDetails("Uproot").WorkPage._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, FindWorkDetails("Uproot").Reward.FindItem(Temp).Count.ToString(), delegate { GameScript._MainBookScript.TargetPage = Temp.name + "|Item"; });
            }
            FindWorkDetails("Uproot").WorkPage._BigPictures[0].Load(ItemPictureScript.PictureTypes.Button, Text: "Выкорчевать", Action: delegate { StartWork("Uproot"); });
            FindWorkDetails("Uproot").WorkPage.PageIsHidden = false;
        }

        GameScript._MainBookScript._Paper.WorkPages = new GameObject[0];
        if (Health > 0)
        {
            GameScript._MainBookScript._Paper.ClearPaper();
            string[] Temp = { PrefabName + "|Shacke", PrefabName + "|Chop" };
            GameScript._MainBookScript._Paper._Pictures[0].Load(ItemPictureScript.PictureTypes.Button, FindWorkDetails("Shacke").WorkPage._BigPictures[0].Icon, null, delegate { GameScript._MainBookScript.TargetPage = Temp[0]; });
            GameScript._MainBookScript._Paper._Pictures[1].Load(ItemPictureScript.PictureTypes.Button, FindWorkDetails("Chop").WorkPage._BigPictures[0].Icon, null, delegate { GameScript._MainBookScript.TargetPage = Temp[1]; });
            GameScript._MainBookScript._Paper.SetContentSize();
            GameScript._MainBookScript._Paper._EnabledPicturesCount = 2;
            GameScript._MainBookScript._Paper.WorkPages = new GameObject[2] { FindWorkDetails("Shacke").WorkPage.gameObject, FindWorkDetails("Chop").WorkPage.gameObject };
            GameScript._MainBookScript._Paper.Name = "Работа";
            GameScript._MainBookScript._Animator.SetBool("Paper", true);
        }

        UpdateWorkerButton();
    }

    IEnumerator WaitEndOfUproot()
    {
        _BoxCollider2D.enabled = false;
        while (!_Animator.GetCurrentAnimatorStateInfo(0).IsName("Uprooted") || _WorkerScript) yield return new WaitForEndOfFrame();
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
                _Animator.Play("Stump");
                if (FindWorkDetails("Uproot").IsStarted && WorkerActID == 2)
                {
                    _Animator.SetBool("Uproot", true);
                    _Animator.Play("Uprooted");
                    StartCoroutine(WaitEndOfUproot());
                }
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
        if (WorkName == "Shacke" && ResoucesCount == 100 || WorkName == "Chop" && Health > 0 || WorkName == "Uproot" && Health == 0 && !_Animator.GetBool("Uprooted") || GameScript.Loading) if (PrepareToWork(WorkName, WorkerPosition)) WorkCoroutine = StartCoroutine(WorkerControll());
        IEnumerator WorkerControll()
        {
            switch(WorkName)
            {
                case "Shacke":
                    _SelectScript.SelectIsEnabled = false;
                    GameScript._MainBookScript.Close();
                    FindWorkDetails("Shacke").IsStarted = true;
                    VillagersControllScript._VillagersControllScript.SkipTimeCurrentVillagerID = _WorkerScript.VillagerID;
                    if (WorkerActID == 0)
                    {
                        while (!_WorkerScript.Run(new Vector2(_Transform.position.x + 0.7f, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                        WorkerActID++;
                    }
                    if (WorkerActID == 1)
                    {
                        if (_WorkerScript._Transform.localScale.x < 0) _WorkerScript.Flip();
                        if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[0] == GameScript.FindGameItem("Log"))
                        {
                            _WorkerScript.SetItem("Stick");
                            _WorkerScript._Animator.speed = 1.3f;
                        }
                        else
                        {
                            _WorkerScript.SetItem(null);
                            _WorkerScript._Animator.speed = 1.2f;
                        }
                        _WorkerScript._Animator.SetBool("Attack", true);
                        for (; ResoucesCount > 5;) 
                        {
                            if (_WorkerScript.Health < 20) _WorkerScript._Animator.speed = 0.7f;
                            if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame(); 
                            Shacke();
                            GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 8;
                        }
                        _WorkerScript._Animator.SetBool("Attack", false);
                        _WorkerScript.SetItem(null); _WorkerScript._Animator.speed = 1f;
                        WorkerActID++;
                        if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                    }
                    if (WorkerActID == 2) _WorkerScript.StartReturn();
                    break;
                case "Chop":
                    _SelectScript.SelectIsEnabled = false;
                    GameScript._MainBookScript.Close();
                    FindWorkDetails("Chop").IsStarted = true;
                    if (WorkerActID == 0)
                    {
                        while (!_WorkerScript.Run(new Vector2(_Transform.position.x + 0.7f, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                        WorkerActID++;
                    }
                    if (WorkerActID == 1)
                    {
                        if (_WorkerScript._Transform.localScale.x < 0) _WorkerScript.Flip();
                        _WorkerScript.SetItem("Axe"); _WorkerScript._Animator.speed = 1.1f;
                        _WorkerScript._Animator.SetBool("Attack", true);
                        for (; Health > 0;)
                        {
                            if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed))
                            {
                                for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                                Instantiate(ChopEffect, new Vector3(_Transform.position.x - 0.2f, _Transform.position.y + 0.3f), Quaternion.identity);
                            }
                            Chop(100);
                            GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 10;
                        }
                        WorkerActID++;
                        _WorkerScript._Animator.SetBool("Attack", false);
                        if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                        _WorkerScript.SetItem(null); _WorkerScript._Animator.speed = 1f;
                    }
                    if (WorkerActID == 2)
                    {
                        while (!_WorkerScript.Run(new Vector2(_Transform.position.x - 0.3f, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                        WorkerActID++;
                    }
                    if (WorkerActID == 3)
                    {
                        if (_WorkerScript._Transform.localScale.x > 0) _WorkerScript.Flip();
                        if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (!_Animator.GetCurrentAnimatorStateInfo(0).IsName("Stump")) yield return new WaitForEndOfFrame();
                        WorkerActID++;
                    }
                    if (WorkerActID == 4) _WorkerScript.StartReturn();
                    break;
                case "Uproot":
                    _SelectScript.SelectIsEnabled = false;
                    GameScript._MainBookScript.Close();
                    FindWorkDetails("Uproot").IsStarted = true;
                    if (WorkerActID == 0)
                    {
                        while (!_WorkerScript.Run(new Vector2(_Transform.position.x + 0.7f, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                        WorkerActID++;
                    }
                    if (WorkerActID == 1)
                    {
                        if (_WorkerScript._Transform.localScale.x < 0) _WorkerScript.Flip();
                        _WorkerScript.SetItem("Shovel"); _WorkerScript._Animator.speed = 0.7f;
                        _WorkerScript._Animator.SetBool("Use_Shovel", true);
                        Uproot();
                        if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                        GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 30;
                        WorkerActID++;
                        _WorkerScript._Animator.SetBool("Use_Shovel", false);
                        if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                        _WorkerScript.SetItem(null); _WorkerScript._Animator.speed = 1;
                    }
                    if (WorkerActID == 2) _WorkerScript.StartReturn();
                    break;
            }
            _SelectScript.SelectIsEnabled = true;
        }
        void Shacke()
        {
            if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[0] != GameScript.FindGameItem("Log") && _WorkerScript.Health >= 20) _WorkerScript.Hit(5, DamageTypes.Hit);
            ResoucesCount -= 20;
            _Animator.SetTrigger("Hit");
            if (VillagersControllScript._VillagersControllScript.IsTimeSkipped)
            {
                if (ResoucesCount > 5) Instantiate(ShackeEffect, new Vector3(_Transform.position.x, _Transform.position.y + 1.6f), Quaternion.identity);
                else Instantiate(ResourcesShackeEffect, new Vector3(_Transform.position.x, _Transform.position.y + 1.6f), Quaternion.identity);
            }
        }
        void Chop(int Damage)
        {
            Health -= Damage;
            _Animator.SetTrigger("Hit");
            if (Health == 0)
            {
                if (ResoucesCount == 100) for (int ItemID = 0; ItemID < FindWorkDetails("Shacke").Reward.Items.Count; ItemID++) GameScript.Village.GiveItem(FindWorkDetails("Shacke").Reward.Items[ItemID].Item, FindWorkDetails("Shacke").Reward.Items[ItemID].Count);
                for (int ItemID = 0; ItemID < FindWorkDetails("Chop").Reward.Items.Count; ItemID++) GameScript.Village.GiveItem(FindWorkDetails("Chop").Reward.Items[ItemID].Item, FindWorkDetails("Chop").Reward.Items[ItemID].Count);
                _Animator.SetBool("Chopped", true);
                if (!VillagersControllScript._VillagersControllScript.IsTimeSkipped) _Animator.Play("Stump");
            }
        }
        void Uproot()
        {
            if (Health == 0)
            {
                for (int ItemID = 0; ItemID < FindWorkDetails("Uproot").Reward.Items.Count; ItemID++) GameScript.Village.GiveItem(FindWorkDetails("Uproot").Reward.Items[ItemID].Item, FindWorkDetails("Uproot").Reward.Items[ItemID].Count);
                _Animator.SetBool("Uprooted", true);
                if (!VillagersControllScript._VillagersControllScript.IsTimeSkipped) _Animator.Play("Uprooted");
                StartCoroutine(WaitEndOfUproot());
            }
        }
    }
}