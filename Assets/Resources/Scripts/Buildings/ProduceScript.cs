using System.Collections;
using UnityEngine;
using System;

public class ProduceScript : BuildScript
{
    private float _ProduceProgress;
    public float ProduceProgress
    {
        get { return _ProduceProgress; }
        set
        {
            if (ProduceItem)
            {
                _ProduceProgress = value;
                if (_ProduceProgress >= 100)
                {
                    _ProduceProgress = 100;
                    if (IsSelected)
                    {
                        if (_WorkerScript && CollectingIsStarted) FindWorkDetails(ProduceItem.name).WorkPage._BigPictures[0].UpdateText("Собираем");
                        else FindWorkDetails(ProduceItem.name).WorkPage._BigPictures[0].UpdateText("Собрать");
                        UpdateWorkerButton();
                    }
                    ItemProducedEvent?.Invoke();
                }
                else if (IsSelected && FindWorkDetails(ProduceItem.name).WorkPage._BigPictures[0].AnimationCenterIsGone) FindWorkDetails(ProduceItem.name).WorkPage._BigPictures[0].UpdateText(Mathf.CeilToInt(_ProduceProgress) + "%", _ProduceProgress / 100);
            }
        }
    }
    [HideInInspector] public float ProduceSpeedCoefficient = 1;
    [HideInInspector] public float ProduceCountCoefficient = 1;
    private ItemScript _ProduceItem;
    public ItemScript ProduceItem
    {
        get { return _ProduceItem; }
        set
        {
            if (!value)
            {
                ProduceIsStarted = false;
                CollectingIsStarted = false;
                ProduceProgress = 0;
                ProduceSpeedCoefficient = 1;
                ProduceCountCoefficient = 1;
            }
            _ProduceItem = value;
        }
    }
    [HideInInspector] public bool ProduceIsStarted;
    [HideInInspector] public bool CollectingIsStarted;

    public Action ItemProducedEvent;

    private bool IsSelected;
    private Coroutine SetPictureCoroutine;

    private void Update() { Produce(); UpdateAct(); }

    private void Produce() { if (ProduceProgress < 100 && ProduceIsStarted) { ProduceProgress += ProduceItem.Recipe.Speed * ProduceSpeedCoefficient * Time.deltaTime; } }

    public virtual void UpdateAct() { }

    public override void UpdatePages()
    {
        if (BuildIsFinished)
        {
            InformationPage.PageIsHidden = false;

            if (WorkDetails.Count > 1)
            {
                GameScript._MainBookScript._Paper.ClearPaper();
                for (int PictureID = 0; PictureID < WorkDetails.Count; PictureID++)
                {
                    ItemScript IconItem = GameScript.FindGameItem(WorkDetails[PictureID].WorkName);
                    string Temp = "Produce_" + PictureID;
                    GameScript._MainBookScript._Paper._Pictures[PictureID].Load(ItemPictureScript.PictureTypes.Button, IconItem.Icon, null, delegate { GameScript._MainBookScript.TargetPage = Temp; });
                }
                GameScript._MainBookScript._Paper.SetContentSize();
                GameScript._MainBookScript._Paper._EnabledPicturesCount = WorkDetails.Count;
                GameScript._MainBookScript._Animator.SetBool("Paper", true);
            }
            GameScript._MainBookScript._Paper.WorkPages = new GameObject[WorkDetails.Count];
            GameScript._MainBookScript._Paper.Name = "Производство";

            for (int PageID = 0; PageID < WorkDetails.Count; PageID++)
            {
                WorkDetails[PageID].WorkPage.ClearPage();
                ItemScript Item = GameScript.FindGameItem(WorkDetails[PageID].WorkName);
                for (int ItemID = 0; ItemID < Item.Recipe.Items.Items.Count; ItemID++)
                {
                    string Temp = Item.Recipe.Items.Items[ItemID].Item.name;
                    WorkDetails[PageID].WorkPage._Pictures[ItemID].Load(ItemPictureScript.PictureTypes.Button, Item.Recipe.Items.Items[ItemID].Item.Icon, null, delegate { GameScript._MainBookScript.TargetPage = Temp + "|Item"; });
                    WorkDetails[PageID].WorkPage._Pictures[ItemID].SetTrackItem(Item.Recipe.Items.Items[ItemID].Item, Item.Recipe.Items.Items[ItemID].Count);
                }
                WorkDetails[PageID].WorkPage._BigPictures[0].Load(ItemPictureScript.PictureTypes.Button, Item.Icon, WorkDetails[PageID].Reward.Items[0].Count.ToString(), delegate { ClickItem(Item); });
                WorkDetails[PageID].WorkPage.PageIsHidden = false;
                GameScript._MainBookScript._Paper.WorkPages[PageID] = WorkDetails[PageID].WorkPage.gameObject;
            }

            ProduceProgress = ProduceProgress;
            UpdateWorkerButton();
        }
    }

    public void LoadProduce(string SaveString)
    {
        _SelectScript.OpenMainBookEvent += () => { IsSelected = true; UpdatePages(); };
        GameScript.Village.VillagersListChangedEvent += () => { if (IsSelected && BuildIsFinished) UpdateWorkerButton(); };
        GameScript._MainBookScript.CloseMainBookEvent += () => { IsSelected = false; };

        InformationPage = GameScript._MainBookScript.FindPage(GameScript._MainBookScript.FindPageID(PrefabName), true).GetComponent<PageScript>();
        if (!String.IsNullOrEmpty(SaveString))
        {
            ProduceSaveClass _Save = JsonUtility.FromJson<ProduceSaveClass>(SaveString);
            ProduceItem = GameScript.FindGameItem(_Save.ProduceItem);
            ProduceProgress = _Save.ProduceProgress;
            ProduceSpeedCoefficient = _Save.ProduceSpeedCoefficient;
            ProduceCountCoefficient = _Save.ProduceCountCoefficient;
            ProduceIsStarted = _Save.ProduceIsStarted;
            CollectingIsStarted = _Save.CollectingIsStarted;
            float SkippedTime = 0;
            while (SkippedTime < GameScript.SkipTime) { Produce(); SkippedTime += Time.deltaTime; }
            LoadWork(_Save.WorkSave);
        }
        else
        {
            ProduceSpeedCoefficient = 1;
            ProduceCountCoefficient = 1;
            LoadWork(null);
        }
        for (int WorkID = 0; WorkID < WorkDetails.Count; WorkID++) WorkDetails[WorkID].WorkPage = GameScript._MainBookScript.FindPage(GameScript._MainBookScript.FindPageID("Produce_" + WorkID), true).GetComponent<InterractPageScript>();
    }

    public string SaveProduce()
    {
        if (_WorkerScript && _WorkerScript.Health == 0 && ProduceItem && !ProduceIsStarted) ProduceItem = null;
        return JsonUtility.ToJson(new ProduceSaveClass((ProduceItem) ? ProduceItem.name : null, ProduceProgress, ProduceSpeedCoefficient, ProduceCountCoefficient, ProduceIsStarted, CollectingIsStarted, SaveWork()));
    }

    [Serializable]
    public class ProduceSaveClass
    {
        public string ProduceItem;
        public float ProduceProgress;
        public float ProduceSpeedCoefficient;
        public float ProduceCountCoefficient;
        public bool ProduceIsStarted;
        public bool CollectingIsStarted;
        public string WorkSave;
        public ProduceSaveClass(string ProduceItem, float ProduceProgress, float ProduceSpeedCoefficient, float ProduceCountCoefficient, bool ProduceIsStarted, bool CollectingIsStarted, string WorkSave)
        {
            this.ProduceItem = ProduceItem;
            this.ProduceProgress = ProduceProgress;
            this.ProduceSpeedCoefficient = ProduceSpeedCoefficient;
            this.ProduceCountCoefficient = ProduceCountCoefficient;
            this.ProduceIsStarted = ProduceIsStarted;
            this.CollectingIsStarted = CollectingIsStarted;
            this.WorkSave = WorkSave;
        }
    }

    public void ClickItem(ItemScript Item)
    {
        if (ProduceItemState == ProduceItemStates.NotProduce)
        {
            if (GameScript.Village.Villagers[FindWorkDetails(Item.name).WorkerID].HasItems(FindWorkDetails(Item.name).NecessaryItems.Items[0].NeededItems) && GameScript.Village.GetItems(Item.Recipe))
            {
                ProduceItem = Item; ProduceIsStarted = false;
                StartWork(ProduceItem.name);
                FindWorkDetails(ProduceItem.name).WorkPage.StartAct();
                FindWorkDetails(ProduceItem.name).WorkPage._BigPictures[0].StartAct();
                if (SetPictureCoroutine != null) StopCoroutine(SetPictureCoroutine);
                SetPictureCoroutine = StartCoroutine(StartSetPictureState(ProduceItem));
                GameScript._MainBookScript._Paper.WorkPages = new GameObject[0];
                GameScript._MainBookScript._Animator.SetBool("Paper", false);
            }
        }
        else if (GameScript.Village.Villagers[FindWorkDetails(ProduceItem.name).WorkerID].HasItems(FindWorkDetails(Item.name).NecessaryItems.Items[1].NeededItems) && ProduceItemState == ProduceItemStates.Produced && !CollectingIsStarted)
        {
            CollectingIsStarted = true;
            StartWork(ProduceItem.name);
            ProduceProgress = ProduceProgress;
            GameScript._MainBookScript._Paper.WorkPages = new GameObject[0];
            GameScript._MainBookScript._Animator.SetBool("Paper", false);
        }
        IEnumerator StartSetPictureState(ItemScript Item)
        {
            while (!FindWorkDetails(Item.name).WorkPage._BigPictures[0].AnimationCenterIsGone) yield return new WaitForEndOfFrame();
            ProduceProgress = ProduceProgress;
            UpdateWorkerButton();
        }
    }

    public void CollectProducedItem()
    {
        if (ProduceItemState == ProduceItemStates.Produced)
        {
            if (GameScript._MainBookScript.MainBookState == MainBookScript.MainBookStates.Opened && GameScript._MainBookScript.MainBookIsStatic && FindWorkDetails(ProduceItem.name).WorkPage.gameObject.activeSelf)
            {
                FindWorkDetails(ProduceItem.name).WorkPage._BigPictures[0].CollectAct();
                if (SetPictureCoroutine != null) StopCoroutine(SetPictureCoroutine);
                SetPictureCoroutine = StartCoroutine(StartSetPictureState(ProduceItem));
            }
            else SetPictureState(ProduceItem);
            ProduceItem = null;
        }
        IEnumerator StartSetPictureState(ItemScript Item)
        {
            while (!FindWorkDetails(Item.name).WorkPage._BigPictures[0].AnimationCenterIsGone) yield return new WaitForEndOfFrame();
            SetPictureState(Item);
        }
        void SetPictureState(ItemScript Item)
        {
            WorkDetailsClass Work = FindWorkDetails(Item.name);
            for (int ItemID = 0; ItemID < Work.Reward.Items.Count; ItemID++) GameScript.Village.GiveItem(Item, (int)(Math.Floor((float)(Work.Reward.Items[ItemID].Count * ProduceCountCoefficient))));
            if (Item && IsSelected) FindWorkDetails(Item.name).WorkPage._BigPictures[0].UpdateText(FindWorkDetails(Item.name).Reward.Items[0].Count.ToString());
            UpdateWorkerButton();
        }
    }

    public enum ProduceItemStates { NotProduce, Producing, Produced }
    public ProduceItemStates ProduceItemState
    {
        get
        {
            if (ProduceItem)
            {
                if (ProduceProgress == 100) return ProduceItemStates.Produced;
                else if (ProduceProgress >= 0 && ProduceProgress < 100) return ProduceItemStates.Producing;
            }
            return ProduceItemStates.NotProduce;
        }
    }

    public override bool PrepareToWork(string WorkName, float WorkerPosition = 0)
    {
        bool _CollectingIsStarted = CollectingIsStarted;
        bool Value = _PrepareToWork(WorkName, WorkerPosition);
        CollectingIsStarted = _CollectingIsStarted;
        return Value;
    }

    public override void StopAllWork()
    {
        if (ProduceItem && !ProduceIsStarted)
        {
            if (GameScript._MainBookScript.MainBookState == MainBookScript.MainBookStates.Opened && GameScript._MainBookScript.MainBookIsStatic && FindWorkDetails(ProduceItem.name).WorkPage.gameObject.activeSelf)
            {
                FindWorkDetails(ProduceItem.name).WorkPage._BigPictures[0].StartAct();
                if (SetPictureCoroutine != null) StopCoroutine(SetPictureCoroutine);
                SetPictureCoroutine = StartCoroutine(StartSetPictureState(ProduceItem));
            }
            else SetPictureState();
        }
        ClearProduce();
        if (WorkCoroutine != null) StopCoroutine(WorkCoroutine);

        IEnumerator StartSetPictureState(ItemScript Item)
        {
            while (!FindWorkDetails(Item.name).WorkPage._BigPictures[0].AnimationCenterIsGone) yield return new WaitForEndOfFrame();
            SetPictureState();
        }
        void SetPictureState()
        {
            if (ProduceItem && IsSelected) FindWorkDetails(ProduceItem.name).WorkPage._BigPictures[0].UpdateText(FindWorkDetails(ProduceItem.name).Reward.Items[0].Count.ToString());
            ProduceItem = null;
            UpdatePages();
        }
    }

    public virtual void ClearProduce() { }

    public override void LoadWorkSideButton(int WorkID)
    {
        if (ProduceItemState == ProduceItemStates.NotProduce || ProduceItemState == ProduceItemStates.Producing) LoadWorkSideButtons(WorkID, WorkDetails[WorkID].NecessaryItems.Items[0]);
        else LoadWorkSideButtons(WorkID, WorkDetails[WorkID].NecessaryItems.Items[1]);
    }
}
