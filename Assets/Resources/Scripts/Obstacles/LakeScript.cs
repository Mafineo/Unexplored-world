using UnityEngine;
using System;
using System.Collections;

public class LakeScript : ObstacleScript
{
    [Header("ќзеро")]
    public GameObject WaterSplashEffect;
    public GameObject ClayLayer;

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
                    if (IsSelected) FindWorkDetails("PickClay").WorkPage._BigPictures[0].UpdateText("—обрать");
                }
                else if (IsSelected) FindWorkDetails("PickClay").WorkPage._BigPictures[0].UpdateText(Mathf.CeilToInt(_ResoucesCount) + "%", _ResoucesCount / 100);
            }
            else
            {
                _ResoucesCount = value;
                if (_ResoucesCount <= 5)
                {
                    for (int ItemID = 0; ItemID < FindWorkDetails("PickClay").Reward.Items.Count; ItemID++) GameScript.Village.GiveItem(FindWorkDetails("PickClay").Reward.Items[ItemID].Item, FindWorkDetails("PickClay").Reward.Items[ItemID].Count);
                    _ResoucesCount = 0;
                }
            }
            _ClayLayerSprite.color = new Color(1, 1, 1, 255f / 100 * _ResoucesCount / 255f);
        }
    }

    private SpriteRenderer _ClayLayerSprite;

    private void Awake() => _ClayLayerSprite = ClayLayer.GetComponent<SpriteRenderer>();

    public override void UpdatePages()
    {
        InformationPage.PageIsHidden = false;
        for (int SlotID = 0; SlotID < FindWorkDetails("PickClay").Reward.Items.Count; SlotID++)
        {
            ItemScript Temp = FindWorkDetails("PickClay").Reward.Items[SlotID].Item;
            FindWorkDetails("PickClay").WorkPage._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, FindWorkDetails("PickClay").Reward.FindItem(Temp).Count.ToString(), delegate { GameScript._MainBookScript.TargetPage = Temp.name + "|Item"; });
        }
        FindWorkDetails("PickClay").WorkPage._BigPictures[0].Load(ItemPictureScript.PictureTypes.Button, Action: delegate { StartWork("PickClay"); });
        FindWorkDetails("PickClay").WorkPage.PageIsHidden = false;
        ResoucesCount = ResoucesCount;

        UpdateWorkerButton();
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
        if (WorkName == "PickClay" && ResoucesCount == 100 || GameScript.Loading) if (PrepareToWork(WorkName, WorkerPosition)) WorkCoroutine = StartCoroutine(WorkerControll());
        IEnumerator WorkerControll()
        {
            switch (WorkName)
            {
                case "PickClay":
                    _SelectScript.SelectIsEnabled = false;
                    GameScript._MainBookScript.Close();
                    FindWorkDetails("PickClay").IsStarted = true;
                    _WorkerScript.SetItem("Pouch");
                    int Offset = (_WorkerScript._Transform.position.x > _Transform.position.x) ? 1 : -1;
                    VillagersControllScript._VillagersControllScript.SkipTimeCurrentVillagerID = _WorkerScript.VillagerID;
                    if (WorkerActID == 0)
                    {
                        while (!_WorkerScript.Run(new Vector2(_Transform.position.x + 3.2f * Offset, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                        WorkerActID++;
                    }
                    if (WorkerActID == 1)
                    {
                        _WorkerScript.NameText.gameObject.SetActive(false);
                        _WorkerScript._Animator.SetBool("Dive", true);
                        _WorkerScript._Animator.speed = 1;
                        if (VillagersControllScript._VillagersControllScript.SkipTime(4.5f))
                        {
                            for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                            Instantiate(WaterSplashEffect, new Vector3(_Transform.position.x + 1.7f * Offset, _Transform.position.y - 0.2f), Quaternion.identity);
                            for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                            _WorkerScript.SetEffect("AirBubbles", true);
                            for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                        }
                        else _WorkerScript.SetEffect("AirBubbles", true);
                        GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 15;
                        WorkerActID++;
                    }
                    if (WorkerActID == 2)
                    {
                        _WorkerScript.SetEffect("AirBubbles", true);
                        _WorkerScript.NameText.gameObject.SetActive(false);
                        _WorkerScript.SetItem("Pouch"); _WorkerScript._Animator.speed = 1f;
                        _WorkerScript._Animator.SetBool("Dive", true);
                        _WorkerScript._Animator.SetBool("Work_Lake_Clay", true);
                        _WorkerScript._Animator.Play("Work_Lake_Clay");
                        for (; ResoucesCount > 5;)
                        {
                            if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                            _WorkerScript.SetItem("Clay", false);
                            if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) yield return new WaitForEndOfFrame(); 
                            PickClay();
                            if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                            _WorkerScript.SetItem("Pouch");
                            GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 5;
                        }
                        WorkerActID++;
                        _WorkerScript._Animator.SetBool("Work_Lake_Clay", false);
                        _WorkerScript._Animator.SetBool("Dive", false); _WorkerScript._Animator.speed = 1;
                        if (VillagersControllScript._VillagersControllScript.SkipTime(7))
                        {
                            for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                            _WorkerScript.SetEffect("AirBubbles", false);
                            for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                            Instantiate(WaterSplashEffect, new Vector3(_Transform.position.x + 2.25f * Offset, _Transform.position.y - 0.2f), Quaternion.identity);
                            for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                        }
                        else _WorkerScript.SetEffect("AirBubbles", false);
                        GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 15;
                        _WorkerScript.NameText.gameObject.SetActive(true);
                        if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                    }
                    if (WorkerActID == 3) _WorkerScript.StartReturn();
                    break;
            }
            _SelectScript.SelectIsEnabled = true;
        }
        void PickClay() => ResoucesCount -= 15;
    }
}