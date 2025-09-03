using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public class BuildScript : WorkScript
{
    [Header("Постройка")]
    public GameObject Building;
    public GameObject Build;
    public GameObject GreenBuild;
    public GameObject RedBuild;
    public List<GameObject> BuildStages = new List<GameObject>();
    public BuildInstructionScript BuildInstruction;

    private InterractPageScript BuildPage;
    private Dictionary<ItemScript, bool> AddedBuildItems = new Dictionary<ItemScript, bool>();
    public bool AllBuildItemsAreAdded
    {
        get
        {
            foreach (KeyValuePair<ItemScript, bool> Item in AddedBuildItems) if (!Item.Value) return false;
            return true;
        }
    }
    private bool _BuildPrepareMode;
    public bool BuildPrepareMode
    {
        get { return _BuildPrepareMode; }
        set
        {
            if (value)
            {
                _BuildPrepareMode = value;
                _SelectScript.SelectIsEnabled = false;
                Building.SetActive(false);
                Build.SetActive(true);
                SetPrepareModePosition(GameScript._CameraScript.transform.position);
            }
            else if (BuildSpaceIsFree)
            {
                _BuildPrepareMode = value;
                Load(null);
                Instantiate(GameScript.FindGamePrefab("BuildSmoke"), _Transform.position, Quaternion.identity);
            }
        }
    }
    [HideInInspector] public bool BuildIsFinished;

    private void OnMouseDrag() { if (_BuildPrepareMode) SetPrepareModePosition(GameScript._Camera.ScreenToWorldPoint(Input.mousePosition)); }
    private void OnMouseDown() { if (_BuildPrepareMode) GameScript._CameraScript.CameraIsFreezed = true; }
    private void OnMouseUp() { if (_BuildPrepareMode) GameScript._CameraScript.CameraIsFreezed = false; }

    private void SetPrepareModePosition(Vector2 Position)
    {
        float SetPosition = -41.6f + Width * 0.8f;
        for (; Position.x >= -40f && Mathf.Abs(SetPosition - Position.x) >= 0.8f && SetPosition < 40f - Width * 0.8f; SetPosition += 1.6f) ;
        _Transform.position = new Vector3(SetPosition, 0, _Transform.position.z);
        (BuildSpaceIsFree ? GreenBuild : RedBuild).SetActive(true);
        (BuildSpaceIsFree ? RedBuild : GreenBuild).SetActive(false);
    }

    private bool BuildSpaceIsFree
    {
        get
        {
            Collider2D[] Colliders = Physics2D.OverlapBoxAll(new Vector2(_Transform.position.x + _BoxCollider2D.offset.x, _Transform.position.y + _BoxCollider2D.offset.y), _BoxCollider2D.size - new Vector2(0.1f, 0.1f), 0);
            for (int ColliderID = 0; ColliderID < Colliders.Length; ColliderID++)
            {
                if (Colliders[ColliderID].gameObject != gameObject && Colliders[ColliderID].tag == "Object")
                {
                    ObjectScript Script = Colliders[ColliderID].GetComponent<ObjectScript>();
                    if (Script.Type == ObjectScript.Types.Building || Script.Type == ObjectScript.Types.Obstacle) return false;
                }
            }
            return true;
        }
    }

    private void StartBuild()
    {
        _SelectScript.SelectIsEnabled = true;
        foreach (ItemsPackScript.ItemClass Ingredient in BuildInstruction.Recipe.Items) AddedBuildItems.Add(Ingredient.Item, false);
        Building.SetActive(false); Build.SetActive(true); GreenBuild.SetActive(false); RedBuild.SetActive(false);
        BuildStages[0].SetActive(true);
        GameScript._MainBookScript.Close();
    }

    private void FinishBuild(bool Loading = false)
    {
        if (AllBuildItemsAreAdded)
        {
            BuildIsFinished = true;
            Health = MaxHealth;
            Building.SetActive(true); Build.SetActive(false);
            BuildPage.PageIsHidden = true;
            GameScript._MainBookScript.Close();
            if (!Loading) Instantiate(GameScript.FindGamePrefab("BuildSmoke"), _Transform.position, Quaternion.identity);
        }
    }

    private void ShowBuildProgress()
    {
        float AddedItemsCount = 0;
        foreach (KeyValuePair<ItemScript, bool> Item in AddedBuildItems) if (Item.Value) AddedItemsCount++;
        if (AddedItemsCount == AddedBuildItems.Count) BuildPage._BigPictures[0].UpdateText("Закончить");
        else BuildPage._BigPictures[0].UpdateText(Mathf.CeilToInt(AddedItemsCount / BuildInstruction.Recipe.Items.Count * 100) + "%", AddedItemsCount / BuildInstruction.Recipe.Items.Count);
    }

    private void StopBuild()
    {
        StartCoroutine(ShowBuild());
        IEnumerator ShowBuild()
        {
            GameScript._MainBookScript.Take();
            while (GameScript._MainBookScript.MainBookState != MainBookScript.MainBookStates.Closed) yield return new WaitForEndOfFrame();
            GameScript._CameraScript.CameraIsFreezed = true;
            Vector2 TargetPosition = _Transform.position;
            while (!GameScript.DistanceIsDone(GameScript._CameraScript._Transform.position, new Vector2(TargetPosition.x, 3f)) || GameScript._Camera.orthographicSize < 4f)
            {
                GameScript._CameraScript._Transform.position = Vector3.MoveTowards(GameScript._CameraScript._Transform.position, new Vector3(TargetPosition.x, 3f, -10f), 10f * Time.deltaTime);
                if (GameScript._Camera.orthographicSize < 4f) GameScript._Camera.orthographicSize += 4f * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            GameScript._MainBookScript.TakeForConfirm();
            GameScript._CameraScript.CameraIsFreezed = true;
            ConfirmPageScript _ConfirmPageScript = GameScript._MainBookScript.FindPage(GameScript._MainBookScript.FindPageID("Confirm"), true).GetComponent<ConfirmPageScript>();
            _ConfirmPageScript._Description.text = "Действительно хочешь отменить стройку? Ты получишь 50% вложенных ресурсов\n";
            _ConfirmPageScript._BigPictures[0].Load(ItemPictureScript.PictureTypes.Button, Action: delegate { if (GameScript.Village.IsVillageAlive) DestroyBuild(); });
            _ConfirmPageScript._BigPictures[1].Load(ItemPictureScript.PictureTypes.Button, Action: delegate { GameScript._MainBookScript.Take(); });
        }
    }

    private void DestroyBuild()
    {
        foreach (KeyValuePair<ItemScript, bool> Item in AddedBuildItems) if (Item.Value) GameScript.Village.GiveItem(Item.Key, (int)(Math.Floor((float)(BuildInstruction.Recipe.FindItem(Item.Key).Count) / 2)));
        GameScript._MainBookScript.Take();
        Instantiate(GameScript.FindGamePrefab("BuildSmoke"), _Transform.position, Quaternion.identity);
        GameScript.Village.DestroyObject(ID);
    }

    private void ShowBuildStage()
    {
        for (int StageID = 0; StageID < BuildStages.Count; StageID++) BuildStages[StageID].gameObject.SetActive(false);
        BuildStages[0].SetActive(true);
        for (int StageID = 0; StageID < BuildStages.Count - 1; StageID++)
        {
            for (int ItemID = 0; ItemID < BuildInstruction.Stages[StageID].StageComponents.Count; ItemID++) if (!AddedBuildItems[BuildInstruction.Stages[StageID].StageComponents[ItemID]]) return;
            BuildStages[StageID].SetActive(false);
            BuildStages[StageID + 1].SetActive(true);
        }
    }

    private void UpdateBuildPages()
    {
        if (!BuildIsFinished)
        {
            InformationPage.PageIsHidden = false;
            BuildPage.PageIsHidden = false;
            BuildPage.ClearPage();
            for (int SlotID = 0; SlotID < BuildInstruction.Recipe.Items.Count; SlotID++)
            {
                ItemScript Temp = BuildInstruction.Recipe.Items[SlotID].Item;
                if (AddedBuildItems[Temp]) BuildPage._Pictures[CurrentSlot(Temp)].Load(ItemPictureScript.PictureTypes.Picture, Temp.Icon, "V", null);
                else
                {
                    BuildPage._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, Action: delegate { AddBuildItem(Temp); });
                    BuildPage._Pictures[SlotID].SetTrackItem(Temp, BuildInstruction.Recipe.FindItem(Temp).Count);
                }
            }
            BuildPage._BigPictures[0].Load(ItemPictureScript.PictureTypes.Button, Action: delegate { FinishBuild(); });
            if (PrefabName != "Mine") BuildPage._BigPictures[1].Load(ItemPictureScript.PictureTypes.Button, Text: "Отмена", Action: delegate { if (GameScript.Village.IsVillageAlive) StopBuild(); });
            ShowBuildProgress();
            int CurrentSlot(ItemScript Item)
            {
                for (int SlotID = 0; SlotID < BuildInstruction.Recipe.Items.Count; SlotID++) if (BuildInstruction.Recipe.Items[SlotID].Item == Item) return SlotID;
                return 0;
            }
        }
    }

    public void LoadBuild(string SaveString)
    {
        _SelectScript.OpenMainBookEvent += UpdateBuildPages;
        BuildPage = GameScript._MainBookScript.FindPage(GameScript._MainBookScript.FindPageID("Build"), true).GetComponent<InterractPageScript>();

        StartBuild();
        if (!String.IsNullOrEmpty(SaveString))
        {
            BuildSaveClass _Save = JsonUtility.FromJson<BuildSaveClass>(SaveString);
            if (_Save.BuildIsFinished)
            {
                if (BuildInstruction.Recipe) foreach (ItemsPackScript.ItemClass Ingredient in BuildInstruction.Recipe.Items) AddedBuildItems[Ingredient.Item] = true;
                FinishBuild(true);
            }
            else
            {
                for (int ItemID = 0; ItemID < _Save.BuildItems.Length; ItemID++)
                {
                    BuildSaveClass.BuildItemClass AddedBuildItem = JsonUtility.FromJson<BuildSaveClass.BuildItemClass>(_Save.BuildItems[ItemID]);
                    ItemScript Script = GameScript.FindGameItem(AddedBuildItem.ItemName);
                    if (AddedBuildItems.ContainsKey(Script) && AddedBuildItem.ItemIsAdded) AddedBuildItems[Script] = true;
                }
                ShowBuildStage();
            }
        }
    }

    public string SaveBuild()
    {
        BuildSaveClass _Save = new BuildSaveClass();
        _Save.BuildIsFinished = BuildIsFinished;
        _Save.BuildItems = new string[AddedBuildItems.Count];
        int ItemID = 0;
        foreach (KeyValuePair<ItemScript, bool> Item in AddedBuildItems)
        {
            BuildSaveClass.BuildItemClass AddedBuildItem = new BuildSaveClass.BuildItemClass(Item.Key.name, Item.Value);
            _Save.BuildItems[ItemID] = JsonUtility.ToJson(AddedBuildItem);
            ItemID++;
        }
        return JsonUtility.ToJson(_Save);
    }

    [Serializable]
    public class BuildSaveClass
    {
        public bool BuildIsFinished;
        public string[] BuildItems;
        [Serializable]
        public class BuildItemClass
        {
            public string ItemName;
            public bool ItemIsAdded;
            public BuildItemClass(string ItemName, bool ItemIsAdded)
            {
                this.ItemName = ItemName;
                this.ItemIsAdded = ItemIsAdded;
            }
        }
    }

    public void FillBuildPage(InterractPageScript Page)
    {
        Page.ClearPage();
        for (int SlotID = 0; SlotID < BuildInstruction.Recipe.Items.Count; SlotID++)
        {
            ItemScript Temp = BuildInstruction.Recipe.Items[SlotID].Item;
            Page._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, BuildInstruction.Recipe.Items[SlotID].Count.ToString(), delegate { GameScript._MainBookScript.TargetPage = Temp.name + "|Item"; });
        }
        Page._BigPictures[0].Load(ItemPictureScript.PictureTypes.Button, Text: "Разместить", Action: delegate { GameScript.Village.PlaceNewBuilding(GetComponent<ObjectScript>().PrefabName); });
    }

    public void CancelBuild()
    {
        GameScript._MainBookScript.Take();
        GameScript.Village.DestroyObject(ID);
    }

    public void AddBuildItem(ItemScript Item)
    {
        if (GameScript.Village.IsVillageAlive && GameScript.Village.GiveItem(Item, -BuildInstruction.Recipe.FindItem(Item).Count))
        {
            BuildPage._Pictures[CurrentSlot()].Load(ItemPictureScript.PictureTypes.Picture, Item.Icon, "V", null);
            AddedBuildItems[Item] = true;
            ShowBuildStage();
            ShowBuildProgress();
        }
        int CurrentSlot()
        {
            for (int SlotID = 0; SlotID < BuildInstruction.Recipe.Items.Count; SlotID++) if (BuildInstruction.Recipe.Items[SlotID].Item == Item) return SlotID;
            return 0;
        }
    }
}