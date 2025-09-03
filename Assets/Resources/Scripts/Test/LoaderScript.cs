using System.Collections;
using UnityEngine;
using System;

public class LoaderScript : MonoBehaviour
{
    /*public CreatureClass Creature = new CreatureClass();

    private void Start() => Load();

    private void Update()
    {
        if (Creature.Target)
        {
            if (GetComponent<CreatureScript>().CreatureRun(new Vector2(Creature.Target.transform.position.x, 0)))
            {
                switch (Creature.CurrentAct)
                {
                    case CreatureActs.Return:
                        GameScript.Village.GameSave -= Save;
                        Destroy(gameObject);
                        break;
                    case CreatureActs.Work:
                        CreatureEnterBuilding();
                        Creature.SetTarget(GameScript._GameScript.FindObject("Town hall"), CreatureActs.Return);
                        break;
                }
            }
        }
        else GetComponent<CreatureScript>().CreatureRun(transform.position);
    }
    //Acts
    public void CreatureEnterBuilding()
    {
        StartCoroutine(CreatureEnterBuildingCoroutine());
        IEnumerator CreatureEnterBuildingCoroutine()
        {
            transform.localScale = new Vector3(0, 0, 0);
            MoveSpeed = 0;
            yield return new WaitForSeconds(1f);
            transform.localScale = new Vector3(1, 1, 1);
            MoveSpeed = 1;
        }
    }
    //Load and Save
    public void Load()
    {
        gameObject.name = Name;
        if (!String.IsNullOrEmpty(SaveString))
        {
            CreatureClass.CreatureSaveClass CreatureSave = JsonUtility.FromJson<CreatureClass.CreatureSaveClass>(SaveString);
            transform.position = CreatureSave.Position;
            Creature.SetTarget((GameScript.Village.Objects[CreatureSave.TargetID]) ? GameScript.Village.Objects[CreatureSave.TargetID].gameObject : GameScript._GameScript.FindObject("Town hall"), (Enum.IsDefined(typeof(CreatureActs), CreatureSave.CurrentAct) || GameScript.Village.Objects.Count >= CreatureSave.TargetID) ? (CreatureActs)Enum.Parse(typeof(CreatureActs), CreatureSave.CurrentAct) : CreatureActs.Return);
        }
        GameScript.Village.GameSave += Save;
    }

    public void Save(ref string[] SavesSpace)
    {
        CreatureClass.CreatureSaveClass CreatureSave = new CreatureClass.CreatureSaveClass();
        CreatureSave.SetValues(transform.position, Creature.Target.GetComponent<ObjectScript>().ID, Creature.CurrentAct.ToString());
        SavesSpace[ID] = gameObject.name + "|" + JsonUtility.ToJson(CreatureSave);
    }
    // storage
    public enum CreatureActs { Return, Work }
    [Serializable]
    public class CreatureClass
    {
        public GameObject Target;
        public CreatureActs CurrentAct;
        public class CreatureSaveClass
        {
            public Vector2 Position;
            public int TargetID;
            public string CurrentAct;
            public void SetValues(Vector2 Position, int TargetID, string CurrentAct)
            {
                this.Position = Position;
                this.TargetID = TargetID;
                this.CurrentAct = CurrentAct;
            }
        }
        public void SetTarget(GameObject Target, CreatureActs CreatureAct)
        {
            this.Target = Target;
            CurrentAct = CreatureAct;
        }
    }*/
}