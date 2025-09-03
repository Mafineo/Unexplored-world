using UnityEngine;
using System;
using System.Collections;

public class StoneScript : ObstacleScript
{
    [Header("Камень")]
    public GameObject Stone;
    public GameObject[] BreakStages;
    public GameObject HitEffect;
    public GameObject SparkEffect;
    public GameObject BreakEffect;

    private void ShowBreakStage()
    {
        int TempHealth = Mathf.CeilToInt((float)(Health) / MaxHealth * 100);
        if (TempHealth > 60) ChangeSprite(0);
        else if (TempHealth <= 60 && TempHealth > 30) ChangeSprite(1);
        else if (TempHealth <= 30) ChangeSprite(2);
        void ChangeSprite(int Stage) { for (int StageID = 0; StageID < BreakStages.Length; StageID++) BreakStages[StageID].SetActive(StageID == Stage); }
    }

    public override void UpdatePages()
    {
        InformationPage.PageIsHidden = false;
        for (int SlotID = 0; SlotID < FindWorkDetails("Break").Reward.Items.Count; SlotID++)
        {
            ItemScript Temp = FindWorkDetails("Break").Reward.Items[SlotID].Item;
            FindWorkDetails("Break").WorkPage._Pictures[SlotID].Load(ItemPictureScript.PictureTypes.Button, Temp.Icon, FindWorkDetails("Break").Reward.FindItem(Temp).Count.ToString(), delegate { GameScript._MainBookScript.TargetPage = Temp.name + "|Item"; });
        }
        FindWorkDetails("Break").WorkPage._BigPictures[0].Load(ItemPictureScript.PictureTypes.Button, Text: "Ломать", Action: delegate { StartWork("Break"); });
        FindWorkDetails("Break").WorkPage.PageIsHidden = false;

        GameScript._MainBookScript._Paper.WorkPages = new GameObject[0];

        UpdateWorkerButton();
    }

    IEnumerator WaitEndOfBreak()
    {
        _BoxCollider2D.enabled = false;
        while (!_Animator.GetCurrentAnimatorStateInfo(0).IsName("Broken") || _WorkerScript) yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }

    public override void Load(string SaveString)
    {
        if (!String.IsNullOrEmpty(SaveString))
        {
            SaveClass _Save = JsonUtility.FromJson<SaveClass>(SaveString);
            _Transform.position = new Vector3(_Save.Position, 0, _Transform.position.z);
            Health = _Save.Health;
            ShowBreakStage();
            LoadObstacle(_Save.ObstacleSave);
            if (Health == 0)
            {
                _Animator.SetBool("Broken", true);
                _Animator.Play("Broken");
                StartCoroutine(WaitEndOfBreak());
            }
        }
        else
        {
            Health = MaxHealth;
            LoadObstacle(null);
        }
    }

    public override string Save()
    {
        SaveClass _Save = new SaveClass(_Transform.position.x, Health, SaveObstacle());
        return PrefabName + "|" + JsonUtility.ToJson(_Save);
    }    

    [Serializable]
    public class SaveClass
    {
        public float Position;
        public int Health;
        public string ObstacleSave;
        public SaveClass(float Position, int Health, string ObstacleSave)
        {
            this.Position = Position;
            this.Health = Health;
            this.ObstacleSave = ObstacleSave;
        }
    }

    public override void StartWork(string WorkName, float WorkerPosition = 0)
    {
        if (WorkName == "Break" && Health > 0 || GameScript.Loading) if (PrepareToWork(WorkName, WorkerPosition)) WorkCoroutine = StartCoroutine(WorkerControll());
        IEnumerator WorkerControll()
        { 
            switch (WorkName)
            {
                case "Break":
                    _SelectScript.SelectIsEnabled = false;
                    GameScript._MainBookScript.Close();
                    FindWorkDetails("Break").IsStarted = true;
                    VillagersControllScript._VillagersControllScript.SkipTimeCurrentVillagerID = _WorkerScript.VillagerID;
                    if (WorkerActID == 0)
                    {
                        while (!_WorkerScript.Run(new Vector2(_Transform.position.x - 1f, _Transform.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                        WorkerActID++;
                    }
                    if (WorkerActID == 1)
                    {
                        if (_WorkerScript._Transform.localScale.x > 0) _WorkerScript.Flip();
                        if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[0] == GameScript.FindGameItem("Pickaxe"))
                        {
                            _WorkerScript.SetItem("Pickaxe");
                            _WorkerScript._Animator.speed = 1.1f;
                            _WorkerScript._Animator.SetBool("Attack", true);
                        }
                        else
                        {
                            _WorkerScript.SetItem("Stone");
                            _WorkerScript._Animator.speed = 0.9f;
                            _WorkerScript._Animator.SetBool("Attack_Stone", true);
                        }
                        for (; Health > 0;)
                        {
                            if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime / _WorkerScript._Animator.speed)) for (_WorkerScript.ActIsEnded = false; !_WorkerScript.ActIsEnded;) yield return new WaitForEndOfFrame();
                            if (GameScript.Village.Villagers[_WorkerScript.VillagerID].Items[0] == GameScript.FindGameItem("Pickaxe"))
                            {
                                Break(130);
                                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) Instantiate(SparkEffect, new Vector3(_Transform.position.x - 0.4f, _Transform.position.y + 0.4f, -0.3f), Quaternion.identity);
                                GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 8;
                            }
                            else
                            {
                                Break(80);
                                if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) Instantiate(HitEffect, new Vector3(_Transform.position.x - 0.4f, _Transform.position.y + 0.4f), Quaternion.identity);
                                GameScript.Village.Villagers[_WorkerScript.VillagerID].Satiety -= 12;
                            }
                        }
                        WorkerActID++;
                        _WorkerScript._Animator.SetBool("Attack", false);
                        _WorkerScript._Animator.SetBool("Attack_Stone", false);
                        if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) while (_WorkerScript.HumanState != HumanScript.HumanStates.Idle) yield return new WaitForEndOfFrame();
                        _WorkerScript.SetItem(null); _WorkerScript._Animator.speed = 1;
                    }
                    if (WorkerActID == 2) _WorkerScript.StartReturn();
                    break;
            }
            _SelectScript.SelectIsEnabled = true;
        }
        void Break(int Damage)
        {
            Health -= Damage;
            int HealthBeforeHit = Mathf.CeilToInt((float)(Health + Damage) / MaxHealth * 100);
            int HealthAfterHit = Mathf.CeilToInt((float)Health / MaxHealth * 100);
            ShowBreakStage();
            if (HealthBeforeHit > 60 && HealthAfterHit <= 60 || HealthBeforeHit > 30 && HealthAfterHit <= 30 || HealthAfterHit == 0) Instantiate(BreakEffect, new Vector3(_Transform.position.x - 0.4f, _Transform.position.y + 1.3f), Quaternion.identity);
            if (HealthAfterHit == 0)
            {
                for (int ItemID = 0; ItemID < FindWorkDetails("Break").Reward.Items.Count; ItemID++) GameScript.Village.GiveItem(FindWorkDetails("Break").Reward.Items[ItemID].Item, FindWorkDetails("Break").Reward.Items[ItemID].Count);
                _Animator.SetBool("Broken", true);
                StartCoroutine(WaitEndOfBreak());
            }
        }
    }
}
