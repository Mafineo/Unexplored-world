using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ObstacleScript : WorkScript
{
    [Header("Препятствие")]
    public float ResourcesRecoverySpeed;
    [HideInInspector] public float _ResoucesCount;
    public virtual float ResoucesCount { get; set; }
    [HideInInspector] public bool IsSelected;

    public override void LoadWorkSideButton(int WorkID) => LoadWorkSideButtons(WorkID, WorkDetails[WorkID].NecessaryItems.Items[0]);

    public override bool PrepareToWork(string WorkName, float WorkerPosition = 0)
    {
        bool Value = false;
        if (!FindWorkDetails(WorkName).IsStarted && FindWorkDetails(WorkName).WorkerID >= 0 && GameScript.Village.Villagers[FindWorkDetails(WorkName).WorkerID].HasItems(FindWorkDetails(WorkName).NecessaryItems.Items[0].NeededItems) || GameScript.Loading) Value = _PrepareToWork(WorkName, WorkerPosition);
        if (Value) GameScript._MainBookScript.Close();
        return Value;
    }

    public void LoadObstacle(string SaveString)
    {
        _SelectScript.OpenMainBookEvent += () => { IsSelected = true; UpdatePages(); };
        GameScript.Village.VillagersListChangedEvent += () => { if (IsSelected) UpdateWorkerButton(); };
        GameScript._MainBookScript.CloseMainBookEvent += () => { IsSelected = false; };

        if (!String.IsNullOrEmpty(SaveString))
        {
            ObstacleSaveClass _Save = JsonUtility.FromJson<ObstacleSaveClass>(SaveString);
            ResoucesCount = _Save.ResoucesCount;
            float SkippedTime = 0;
            while (SkippedTime < GameScript.SkipTime) { ResourcesRecovery(); SkippedTime += Time.deltaTime; }
            LoadWork(_Save.WorkSave);
        }
        else
        {
            ResoucesCount = 100;
            LoadWork(null);
        }
        for (int WorkID = 0; WorkID < WorkDetails.Count; WorkID++) WorkDetails[WorkID].WorkPage = GameScript._MainBookScript.FindPage(GameScript._MainBookScript.FindPageID(GetComponent<ObjectScript>().PrefabName + "|" + WorkDetails[WorkID].WorkName), true).GetComponent<InterractPageScript>();
    }

    public string SaveObstacle() { return JsonUtility.ToJson(new ObstacleSaveClass(ResoucesCount, SaveWork())); }

    [Serializable]
    public class ObstacleSaveClass
    {
        public float ResoucesCount;
        public string WorkSave;
        public ObstacleSaveClass(float ResoucesCount, string WorkSave)
        {
            this.ResoucesCount = ResoucesCount;
            this.WorkSave = WorkSave;
        }
    }

    public override void StopAllWork()
    {
        _SelectScript.SelectIsEnabled = true;
        if (WorkCoroutine != null) StopCoroutine(WorkCoroutine);
    }

    private void Update() { ResourcesRecovery(); }

    private void ResourcesRecovery() { if (ResoucesCount < 100 && Health > 0) if (AllWorkIsFinished || _WorkerScript.IsReturning) ResoucesCount += ResourcesRecoverySpeed * Time.deltaTime; }
}
