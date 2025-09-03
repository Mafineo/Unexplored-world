using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WorkScript : ObjectScript
{
    public List<WorkDetailsClass> WorkDetails;

    private Coroutine _OpenPaperCoroutine;
    [HideInInspector] public HumanScript _WorkerScript;
    [HideInInspector] public int WorkerActID;
    [HideInInspector] public Coroutine WorkCoroutine;

    public virtual void UpdatePages() { }

    public void UpdateWorkerButton()
    {
        for (int WorkID = 0; WorkID < WorkDetails.Count; WorkID++)
        {
            if (WorkDetails[WorkID].WorkPage)
            {
                if (GameScript.Village.IsVillageAlive)
                {
                    GameScript.VillageClass.VillagerClass Villager = GameScript.Village.Villagers[WorkDetails[WorkID].WorkerID];
                    int Temp = WorkID;
                    WorkDetails[WorkID].WorkPage._BigPictures[1].Load(ItemPictureScript.PictureTypes.Button, GameScript._HumanScript.FindClothes(Villager.Items[1] ? Villager.Items[1].name : "PeasantClothes").FindClothesPart("Head"), Action: delegate { StartChangeWorker(Temp); });
                    WorkDetails[WorkID].WorkPage._BigPictures[1].UpdateText(Villager.Name, FillAmount: (Villager.IsOutside) ? ((Villager.Human && Villager.Human.GetComponent<HumanScript>().IsReturning) ? 1 : 0) : 1);
                }
                else
                {
                    int Temp = WorkDetails[WorkID].WorkerID;
                    WorkDetails[WorkID].WorkPage._BigPictures[1].Load(ItemPictureScript.PictureTypes.Button, Action: delegate { StartChangeWorker(Temp); });
                }
                for (int PictureID = 0; PictureID < WorkDetails[WorkID].WorkPage._SidePictures.Length; PictureID++) { WorkDetails[WorkID].WorkPage._SidePictures[PictureID].gameObject.SetActive(false); }

                LoadWorkSideButton(WorkID);
            }
        }
    }

    public virtual void LoadWorkSideButton(int WorkID) { }

    public void LoadWorkSideButtons(int WorkID, NecessaryItemsScript.ItemsGroupClass ItemsGroup)
    {
        if (GameScript.Village.IsVillageAlive)
        {
            ItemScript[][] Items = ItemsGroup.AllItems;
            for (int GroupID = 0; GroupID < Items.GetLength(0); GroupID++)
            {
                if (Items[GroupID].Length > 1 || Items[GroupID].Length > 0 && ItemsGroup.Items[GroupID].Items[0])
                {
                    for (int ItemID = 0; ItemID < Items[GroupID].Length; ItemID++)
                    {
                        int[] Temps = { WorkID, GroupID };
                        if (GameScript.Village.Villagers[WorkDetails[WorkID].WorkerID].Items[GroupID] == ItemsGroup.Items[GroupID].Items[ItemID])
                        {
                            WorkDetails[WorkID].WorkPage._SidePictures[GroupID].Load(ItemPictureScript.PictureTypes.Button, (GameScript.Village.Villagers[WorkDetails[WorkID].WorkerID].Items[GroupID]) ? GameScript.Village.Villagers[WorkDetails[WorkID].WorkerID].Items[GroupID].Icon : null, Action: delegate { StartChangeWorkerItem(Temps[0], Temps[1], ItemsGroup); });
                            break;
                        }
                        else if (ItemID == Items[GroupID].Length - 1)
                        {
                            if (!ItemsGroup.Items[GroupID].Items[0]) WorkDetails[WorkID].WorkPage._SidePictures[GroupID].Load(ItemPictureScript.PictureTypes.Button, null, Action: delegate { StartChangeWorkerItem(Temps[0], Temps[1], ItemsGroup); });
                            else
                            {
                                WorkDetails[WorkID].WorkPage._SidePictures[GroupID].Load(ItemPictureScript.PictureTypes.Button, ItemsGroup.Items[GroupID].Items[ItemID].Icon, Action: delegate { StartChangeWorkerItem(Temps[0], Temps[1], ItemsGroup); });
                                WorkDetails[WorkID].WorkPage._SidePictures[GroupID].UpdateText(FillAmount: 0);
                            }
                        }
                    }
                }
            }
        }
    }

    private void StartChangeWorker(int WorkID)
    {
        if (_OpenPaperCoroutine != null) StopCoroutine(_OpenPaperCoroutine);
        _OpenPaperCoroutine = StartCoroutine(StartOpenPaper());
        IEnumerator StartOpenPaper()
        {
            GameScript._MainBookScript._Animator.SetBool("Paper", false);
            while (!GameScript._MainBookScript._Animator.GetCurrentAnimatorStateInfo(0).IsName("Opened")) yield return new WaitForEndOfFrame();
            if (GameScript._MainBookScript._Paper.WorkPages.Length > 0 && GameScript._MainBookScript._Paper.WorkPages[0] == gameObject) UpdatePages();
            else
            {
                GameScript._MainBookScript._Paper.ClearPaper();
                int PictureID = 0;
                foreach (KeyValuePair<int, GameScript.VillageClass.VillagerClass> Villager in GameScript.Village.Villagers)
                {
                    int Temp = Villager.Key;
                    GameScript._MainBookScript._Paper._Pictures[PictureID].Load(ItemPictureScript.PictureTypes.Button, GameScript._HumanScript.FindClothes(Villager.Value.Items[1] ? Villager.Value.Items[1].name : "PeasantClothes").FindClothesPart("Head"), Action: delegate { ChangeWorker(WorkID, Temp); });
                    GameScript._MainBookScript._Paper._Pictures[PictureID].UpdateText(Villager.Value.Name, FillAmount: (Villager.Value.IsOutside) ? ((Villager.Value.Human && Villager.Value.Human.GetComponent<HumanScript>().IsReturning) ? 1 : 0) : 1);
                    PictureID++;
                }
                GameScript._MainBookScript._Paper.SetContentSize();
                GameScript._MainBookScript._Paper.WorkPages = new GameObject[] { gameObject };
                GameScript._MainBookScript._Paper._EnabledPicturesCount = 1;
                GameScript._MainBookScript._Paper.Name = "Рабочие";
                GameScript._MainBookScript._Animator.SetBool("Paper", true);
            }
        }
        void ChangeWorker(int WorkID, int NewWorkerID)
        {
            if (!_WorkerScript)
            {
                WorkDetails[WorkID].WorkerID = NewWorkerID;
                if (_OpenPaperCoroutine != null) StopCoroutine(_OpenPaperCoroutine);
                _OpenPaperCoroutine = StartCoroutine(StartOpenPaper());
                IEnumerator StartOpenPaper()
                {
                    GameScript._MainBookScript._Animator.SetBool("Paper", false);
                    while (!GameScript._MainBookScript._Animator.GetCurrentAnimatorStateInfo(0).IsName("Opened")) yield return new WaitForEndOfFrame();
                    UpdatePages();
                }
            }
        }
    }

    private void StartChangeWorkerItem(int WorkID, int SlotID, NecessaryItemsScript.ItemsGroupClass ItemsGroup)
    {
        if (_OpenPaperCoroutine != null) StopCoroutine(_OpenPaperCoroutine);
        _OpenPaperCoroutine = StartCoroutine(StartOpenPaper());
        IEnumerator StartOpenPaper()
        {
            GameScript._MainBookScript._Animator.SetBool("Paper", false);
            while (!GameScript._MainBookScript._Animator.GetCurrentAnimatorStateInfo(0).IsName("Opened")) yield return new WaitForEndOfFrame();
            if (GameScript._MainBookScript._Paper.WorkPages.Length > 0 && GameScript._MainBookScript._Paper.WorkPages[0] == gameObject) UpdatePages();
            else
            {
                GameScript._MainBookScript._Paper.ClearPaper();
                ItemScript[][] Items = ItemsGroup.AllItems;
                int PictureID = 0;
                if (!ItemsGroup.Items[SlotID].Items[0])
                {
                    ItemScript Temp = Items[SlotID][PictureID];
                    GameScript._MainBookScript._Paper._Pictures[PictureID].Load(ItemPictureScript.PictureTypes.Button, WorkDetails[WorkID].WorkPage._SidePictures[SlotID].Icon, Action: delegate { ChangeWorkerItem(WorkID, SlotID, Temp); });
                    PictureID++;
                }
                for (; PictureID < Items[SlotID].Length; PictureID++)
                {
                    ItemScript Temp = Items[SlotID][PictureID];
                    GameScript._MainBookScript._Paper._Pictures[PictureID].Load(ItemPictureScript.PictureTypes.Button, Items[SlotID][PictureID].Icon, Action: delegate { ChangeWorkerItem(WorkID, SlotID, Temp); });
                    GameScript._MainBookScript._Paper._Pictures[PictureID].SetTrackItem(Items[SlotID][PictureID]);
                }
                GameScript._MainBookScript._Paper.SetContentSize();
                GameScript._MainBookScript._Paper.WorkPages = new GameObject[] { gameObject };
                GameScript._MainBookScript._Paper._EnabledPicturesCount = 1;
                switch (SlotID)
                {
                    case 0: GameScript._MainBookScript._Paper.Name = "Инструменты"; break;
                    case 1: GameScript._MainBookScript._Paper.Name = "Одежда"; break;
                    case 2: GameScript._MainBookScript._Paper.Name = "Карманы"; break;
                    default: GameScript._MainBookScript._Paper.Name = "Сумка"; break;
                }
                GameScript._MainBookScript._Animator.SetBool("Paper", true);
            }
        }
        void ChangeWorkerItem(int WorkID, int SlotID, ItemScript Item)
        {
            if (AllWorkIsFinished || _WorkerScript && _WorkerScript.IsReturning || !GameScript.Village.Villagers[WorkDetails[WorkID].WorkerID].IsOutside)
            {
                if (GameScript.Village.Villagers[WorkDetails[WorkID].WorkerID].SetItem(SlotID, Item, GameScript.Village.Villagers[WorkDetails[WorkID].WorkerID].IsOutside))
                {
                    if (_OpenPaperCoroutine != null) StopCoroutine(_OpenPaperCoroutine);
                    _OpenPaperCoroutine = StartCoroutine(StartOpenPaper());
                }
            }
            IEnumerator StartOpenPaper()
            {
                GameScript._MainBookScript._Animator.SetBool("Paper", false);
                while (!GameScript._MainBookScript._Animator.GetCurrentAnimatorStateInfo(0).IsName("Opened")) yield return new WaitForEndOfFrame();
                UpdatePages();
            }
        }
    }

    public virtual void StartWork(string WorkName, float WorkerPosition = 0) { }

    public void FinishAllWork() 
    {
        foreach (WorkDetailsClass Details in WorkDetails)
        {
            WorkerActID = -1;
            Details.IsStarted = false;
        }
        StopAllWork();
    }

    public virtual void StopAllWork() { }

    public string StartedWork
    {
        get
        {
            foreach (WorkDetailsClass Details in WorkDetails) if (Details.IsStarted) return Details.WorkName;
            return null;
        }
    }

    public bool AllWorkIsFinished { get { foreach (WorkDetailsClass Details in WorkDetails) { if (Details.IsStarted) return false; } return true; } }

    public void CreateWorker(int VillagerID)
    {
        bool IsOutside = false;
        if (_WorkerScript)
        {
            _WorkerScript.TargetObject = null;
            _WorkerScript.StartReturn();
            IsOutside = true;
        }
        else
        {
            IsOutside = GameScript.Village.Villagers[VillagerID].IsOutside;
            _WorkerScript = GameScript.Village.Villagers[VillagerID].GoOutside().GetComponent<HumanScript>();
        }
        _WorkerScript.TargetObject = gameObject;
        _WorkerScript.StartEnter(GameScript.Loading || IsOutside);
    }

    public WorkDetailsClass FindWorkDetails(string Name)
    {
        foreach (WorkDetailsClass Work in WorkDetails) if (Work.WorkName == Name) return Work;
        return null;
    }

    public bool _PrepareToWork(string WorkName, float WorkerPosition = 0)
    {
        if (FindWorkDetails(WorkName).WorkerID >= 0) 
        {
            if (WorkerActID == -1 && !GameScript.Loading || _WorkerScript && _WorkerScript.IsReturning || WorkerActID != -1 && GameScript.Loading)
            {
                GameScript._MainBookScript._Paper.WorkPages = new GameObject[0];
                GameScript._MainBookScript._Animator.SetBool("Paper", false);
                if (_WorkerScript) FinishAllWork();
                CreateWorker(FindWorkDetails(WorkName).WorkerID);
                if (GameScript.Loading) _WorkerScript._Transform.position = new Vector3(WorkerPosition, 0, _WorkerScript._Transform.position.z);
                else WorkerActID = 0;
                return true;
            }
        }
        return false;
    }

    public virtual bool PrepareToWork(string WorkName, float WorkerPosition = 0) { return _PrepareToWork(WorkName, WorkerPosition); }

    public void ChangeWorkersAfterDelete(int VillagerID)
    {
        if (GameScript.Village.IsVillageAlive) { for (int WorkID = 0; WorkID < WorkDetails.Count; WorkID++) if (WorkDetails[WorkID].WorkerID == VillagerID) WorkDetails[WorkID].WorkerID = GameScript.Village.GetFisrtVillager(); }
        else { for (int WorkID = 0; WorkID < WorkDetails.Count; WorkID++) WorkDetails[WorkID].WorkerID = -1; }
    }

    public enum WorkPageTypes { Produce, Obstacle }
    public void LoadWork(string SaveString)
    {
        int[] DeadVillagers = new int[0];
        ObjectScript _ObjectScript = GetComponent<ObjectScript>();
        GameScript.Village.VillagerDeletedEvent += ChangeWorkersAfterDelete;
        if (!String.IsNullOrEmpty(SaveString))
        {
            WorkSaveClass _Save = JsonUtility.FromJson<WorkSaveClass>(SaveString);
            WorkerActID = _Save.WorkerActID;
            for (int WorkID = 0; WorkID < _Save.WorkDetails.Length; WorkID++)
            {
                WorkSaveClass.WorkClass Work = JsonUtility.FromJson<WorkSaveClass.WorkClass>(_Save.WorkDetails[WorkID]);
                WorkDetailsClass Temp = FindWorkDetails(Work.WorkName);
                if (Temp != null)
                {
                    FindWorkDetails(Work.WorkName).SetValues(Work.WorkerID, Work.IsStarted);
                    if (Work.IsStarted)
                    {
                        if (GameScript.Village.Villagers.ContainsKey(WorkDetails[WorkID].WorkerID)) StartWork(Work.WorkName, Work.WorkerPosition);
                        else
                        {
                            DeadVillagers = GameScript.ExpandArray(DeadVillagers);
                            DeadVillagers[DeadVillagers.Length - 1] = WorkDetails[WorkID].WorkerID;
                            FinishAllWork();
                        }
                    }
                }
            }
            for (int VillagerID = 0; VillagerID < DeadVillagers.Length; VillagerID++) ChangeWorkersAfterDelete(DeadVillagers[VillagerID]);
        }
        else WorkerActID = -1;
    }

    public string SaveWork()
    {
        string[] Works = new string[WorkDetails.Count];
        for (int WorkID = 0; WorkID < Works.Length; WorkID++) Works[WorkID] = JsonUtility.ToJson(new WorkSaveClass.WorkClass(WorkDetails[WorkID].WorkName, WorkDetails[WorkID].WorkerID, (_WorkerScript) ? _WorkerScript._Transform.position.x : 0, WorkDetails[WorkID].IsStarted));
        return JsonUtility.ToJson(new WorkSaveClass(WorkerActID, Works));
    }

    public class WorkSaveClass
    {
        public int WorkerActID;
        public string[] WorkDetails;
        public WorkSaveClass(int WorkerActID, string[] WorkDetails)
        {
            this.WorkerActID = WorkerActID;
            this.WorkDetails = WorkDetails;
        }
        public class WorkClass
        {
            public string WorkName;
            public int WorkerID;
            public float WorkerPosition;
            public bool IsStarted;
            public WorkClass(string WorkName, int WorkerID, float WorkerPosition, bool IsStarted)
            {
                this.WorkName = WorkName;
                this.WorkerID = WorkerID;
                this.WorkerPosition = WorkerPosition;
                this.IsStarted = IsStarted;
            }
        }
    }

    [Serializable]
    public class WorkDetailsClass
    {
        public string WorkName;
        public NecessaryItemsScript NecessaryItems;
        public ItemsPackScript Reward;
        [HideInInspector] public InterractPageScript WorkPage;
        [HideInInspector] public int WorkerID;
        [HideInInspector] public bool IsStarted;
        public void SetValues(int WorkerID, bool IsStarted)
        {
            this.WorkerID = WorkerID;
            this.IsStarted = IsStarted;
        }
    }
}
