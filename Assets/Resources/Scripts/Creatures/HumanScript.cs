using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;

public class HumanScript : ObjectScript
{
    [Header("Одежда")]
    public List<BodyPartClass> BodyParts = new List<BodyPartClass>();
    public List<ClothesClass> ClothesList = new List<ClothesClass>();
    public List<EffectClass> EffectsList = new List<EffectClass>();
    [Space(5f)]
    [Header("Предметы")]
    public GameObject[] ItemsList;
    [Header("Интерфейс")]
    public Transform InterfaceTransform;
    public Text NameText;

    [HideInInspector] public string CurrentHumanClothes = "PeasantClothes";
    [HideInInspector] public string CurrentHumanItem = null;
    [HideInInspector] public bool ActIsEnded;
    private bool IsBusy;
    [HideInInspector] public bool IsReturning;

    private int _VillagerID = -1;
    public int VillagerID
    {
        get { return _VillagerID; }
        set
        {
            _VillagerID = value;
            HealthChangesEvent += () => GameScript.Village.VillagersListChangedEvent?.Invoke();
        }
    }
    private GameObject _TargetObject;
    public GameObject TargetObject
    {
        get { return _TargetObject; }
        set
        {
            StopAllCoroutines();
            IsReturning = false;
            if (_TargetObject)
            {
                if (_TargetObject.TryGetComponent(out WorkScript WorkScript))
                {
                    WorkScript.WorkerActID = -1;
                    if (Health == 0) WorkScript._WorkerScript = null;
                }
                if (_TargetObject.TryGetComponent(out ProduceScript ProduceScript))
                {
                    if (ProduceScript.CollectingIsStarted || Health == 0) ProduceScript.FinishAllWork();
                    ProduceScript.CollectingIsStarted = false;
                }
                else if (_TargetObject.TryGetComponent(out ObstacleScript ObstacleScript)) ObstacleScript.FinishAllWork();
            }
            _TargetObject = value;
        }
    }

    private void Awake()
    {
        foreach (EffectClass Effect in EffectsList) Effect.MainModule = Effect.Effect.GetComponent<ParticleSystem>().main;
        IsBusy = true;
    }

    public bool Run(Vector2 TargetPosition)
    {
        if (IsBusy)
        {
            _Animator.SetBool("Run", false);
            return false;
        }
        else if (Mathf.Abs(TargetPosition.x - _Transform.position.x) > 0.0001f || Mathf.Abs(TargetPosition.y - _Transform.position.y) > 0.0001f)
        {
            if (HumanState != HumanStates.Other)
            {
                if (_Transform.position.x > TargetPosition.x && _Transform.localScale.x < 0 || _Transform.position.x < TargetPosition.x && _Transform.localScale.x > 0) Flip();
                _Transform.position = Vector3.MoveTowards(_Transform.position, new Vector3(TargetPosition.x, TargetPosition.y, _Transform.position.z), MoveSpeed * Time.deltaTime);
                _Animator.speed = MoveSpeed;
                _Animator.SetBool("Run", true);
            } 
            return false;
        }
        else
        {
            _Animator.speed = 1;
            _Animator.SetBool("Run", false);
            return true;
        }
    }

    public void Flip()
    {
        _Transform.localScale = new Vector3(-_Transform.localScale.x, _Transform.localScale.y, _Transform.localScale.z);
        InterfaceTransform.localScale = new Vector3(-InterfaceTransform.localScale.x, InterfaceTransform.localScale.y, InterfaceTransform.localScale.z);
    }

    public void StartEnter(bool LoadedEnter)
    {
        if (!LoadedEnter) StartCoroutine(HumanControll());
        else IsBusy = false;
        IEnumerator HumanControll()
        {
            if (GameScript._EnviromentScript.Location == EnviromentScript.Locations.Cave)
            {
                Transform _CaveDescentTransform = GameScript.Village.FindObject("CaveDescent").GetComponent<Transform>();
                CaveDescentScript _CaveDescentScript = _CaveDescentTransform.gameObject.GetComponent<CaveDescentScript>();
                _Transform.position = new Vector3(_CaveDescentTransform.position.x + 0.5f, _CaveDescentTransform.position.y + 7.2f, _Transform.position.z);
                NameText.gameObject.SetActive(false);
                if (_Transform.localScale.x < 0) Flip();
                while (!_CaveDescentScript.SetRopeUser(gameObject)) yield return new WaitForEndOfFrame();
                SetItem(null); _Animator.speed = 0.3f;
                _Animator.SetBool("UseCaveRope_Down", true);
                while (_Transform.position.y > 0)
                {
                    if (_CaveDescentScript._RopeUser == gameObject && _Transform.position.y < 5) _CaveDescentScript.SetRopeUser(null);
                    _Transform.position = Vector3.MoveTowards(_Transform.position, new Vector3(_Transform.position.x, -1f, _Transform.position.z), 2f * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }
                _Transform.position = new Vector3(_CaveDescentTransform.position.x + 0.5f, _CaveDescentTransform.position.y, _Transform.position.z);
                _Animator.speed = 1f;
                _Animator.SetBool("UseCaveRope_Down", false);
                for (ActIsEnded = false; !ActIsEnded;) yield return new WaitForEndOfFrame();
                while (HumanState != HumanStates.Idle) yield return new WaitForEndOfFrame();
                NameText.gameObject.SetActive(true);
            }
            else
            {
                Transform _TownHallTransform = GameScript.Village.FindObject("TownHall").GetComponent<Transform>();
                _Transform.position = new Vector3(_TownHallTransform.position.x - 0.4f, _TownHallTransform.position.y, _Transform.position.z);
            }
            IsBusy = false;
        }
    }

    public void StartReturn()
    {
        StartCoroutine(HumanControll());
        IEnumerator HumanControll()
        {
            IsReturning = true;
            if (VillagersControllScript._VillagersControllScript.IsTimeSkipped) yield return new WaitForEndOfFrame();
            if (GameScript._EnviromentScript.Location == EnviromentScript.Locations.Cave)
            {
                Transform Target = GameScript.Village.FindObject("CaveDescent").GetComponent<Transform>();
                CaveDescentScript _CaveDescentScript = Target.gameObject.GetComponent<CaveDescentScript>();
                while (!Run(new Vector2(Target.position.x + 0.5f + _CaveDescentScript._RopeUsers.Count * 0.8f, Target.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                VillagersControllScript._VillagersControllScript.SkipAllTime();
                _CaveDescentScript._RopeUsers.Add(gameObject);
                if (_Transform.localScale.x < 0) Flip();
                while (!_CaveDescentScript.SetRopeUser(gameObject))
                {
                    Run(new Vector2(Target.position.x + 0.5f + _CaveDescentScript.FindRopeUserID(gameObject) * 0.8f, Target.position.y));
                    yield return new WaitForEndOfFrame();
                }
                while (!Run(new Vector2(Target.position.x + 0.5f, Target.position.y))) yield return new WaitForEndOfFrame();
                if (_Transform.localScale.x < 0) Flip();
                IsReturning = false;
                NameText.gameObject.SetActive(false);
                SetItem(null); _Animator.speed = 1;
                _Animator.SetBool("UseCaveRope_Up", true);
                for (ActIsEnded = false; !ActIsEnded;) yield return new WaitForEndOfFrame();
                while (_Transform.position.y < 7.2f)
                {
                    if (_CaveDescentScript._RopeUser == gameObject && _Transform.position.y > 3)
                    {
                        _CaveDescentScript.SetRopeUser(null);
                        _CaveDescentScript._RopeUsers.RemoveAt(0);
                    }
                    _Transform.position = Vector3.MoveTowards(_Transform.position, new Vector3(_Transform.position.x, 8f, _Transform.position.z), Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }
                GameScript.Village.DestroyObject(ID);
                TargetObject = null;
                if (VillagerID != -1) GameScript.Village.Villagers[VillagerID].GoInside();
            }
            else
            {
                Transform Target = GameScript.Village.FindObject("TownHall").GetComponent<Transform>();
                while (!Run(new Vector2(Target.position.x - 0.4f, Target.position.y))) if (VillagersControllScript._VillagersControllScript.SkipTime(Time.deltaTime)) yield return new WaitForEndOfFrame();
                VillagersControllScript._VillagersControllScript.SkipAllTime();
                TargetObject = null;
                if (VillagerID != -1) GameScript.Village.Villagers[VillagerID].GoInside();
            }
        }
    }

    public override void Die(DamageTypes DamageType)
    {
        TargetObject = null;
        SetItem(null);
        IsBusy = true;
        _Animator.speed = 1;
        NameText.gameObject.SetActive(false);
        DamageTypes _DamageType = DamageType;
        if (_Animator.GetCurrentAnimatorStateInfo(0).IsName("UseCaveRope_0") || _Animator.GetCurrentAnimatorStateInfo(0).IsName("UseCaveRope_1"))
        {
            _DamageType = DamageTypes.Fell;
            StartCoroutine(StartFall());
        }
        else _Animator.SetInteger("Die", (int)DamageType);
        _Animator.SetBool("AnimationBan", true);
        if (VillagerID != -1) GameScript.Village.Villagers[VillagerID].Death = JsonUtility.ToJson(new GameScript.VillageClass.VillagerClass.SaveClass.DeathClass(_Transform.position.x, _Transform.localScale.x > 0, _DamageType.ToString()));
        if (GameScript.Loading)
        {
            if ((int)DamageType <= 3) _Animator.Play("Dead_" + (int)DamageType);
            else _Animator.Play("Dead_0");
        }
        IEnumerator StartFall()
        {
            if (GameScript._EnviromentScript.Location == EnviromentScript.Locations.Cave)
            {
                Transform _CaveDescentTransform = GameScript.Village.FindObject("CaveDescent").GetComponent<Transform>();
                CaveDescentScript _CaveDescentScript = _CaveDescentTransform.gameObject.GetComponent<CaveDescentScript>();
                _CaveDescentScript.SetRopeUser(null);
                _Animator.SetInteger("Die", (int)DamageType);
                while (_Transform.position.y > 0)
                {
                    _Transform.position = Vector3.MoveTowards(_Transform.position, new Vector3(_Transform.position.x, -1f, _Transform.position.z), 3f * Time.deltaTime);
                    yield return new WaitForEndOfFrame();
                }
                _Transform.position = new Vector3(_CaveDescentTransform.position.x + 0.5f, _CaveDescentTransform.position.y, _Transform.position.z);
                _Animator.speed = 1f;
                _Animator.SetBool("UseCaveRope_Up", false);
            }
        }
    }

    public void ActEnd() => ActIsEnded = true;

    public enum HumanStates { Idle, Run, Other }
    public HumanStates HumanState
    {
        get
        {
            if (_Animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) return HumanStates.Idle;
            else if (_Animator.GetCurrentAnimatorStateInfo(0).IsName("Run")) return HumanStates.Run;
            else return HumanStates.Other;
        }
    }

    [Serializable]
    public class BodyPartClass
    {
        public string Name;
        public GameObject Part;
    }

    public BodyPartClass FindBodyPart(string Name)
    {
        foreach (BodyPartClass BodyPart in BodyParts) if (BodyPart.Name == Name) return BodyPart;
        return null;
    }

    [Serializable]
    public class ClothesClass
    {
        public string Name;
        public List<BodyPartClass> BodyParts = new List<BodyPartClass>();
        [Serializable]
        public class BodyPartClass
        {
            public string Name;
            public Sprite Part;
        }
        public Sprite FindClothesPart(string Name)
        {
            foreach (BodyPartClass BodyPart in BodyParts) if (BodyPart.Name == Name) return BodyPart.Part;
            return null;
        }
    }

    public ClothesClass FindClothes(string Name)
    {
        foreach (ClothesClass Clothes in ClothesList) if (Clothes.Name == Name) return Clothes;
        return null;
    }

    public void SetClothes(string Name)
    {
        foreach (BodyPartClass BodyPart in BodyParts) BodyPart.Part.GetComponent<SpriteRenderer>().sprite = null;
        CurrentHumanClothes = Name;
        ClothesClass Clothes = FindClothes(CurrentHumanClothes);
        foreach (ClothesClass.BodyPartClass BodyPart in Clothes.BodyParts) FindBodyPart(BodyPart.Name).Part.GetComponent<SpriteRenderer>().sprite = BodyPart.Part;
    }

    public void SetItem(string Name, bool HideOldItems = true)
    {
        foreach (GameObject Item in ItemsList)
        {
            if (Item.name == Name)
            {
                CurrentHumanItem = Item.name;
                Item.SetActive(true);
            }
            else if (HideOldItems) Item.SetActive(false);
        }
    }

    [Serializable]
    public class EffectClass
    {
        public string Name;
        public GameObject Effect;
        [HideInInspector] public ParticleSystem.MainModule MainModule;
    }

    public void SetEffect(string Name, bool State)
    {
        foreach (EffectClass Effect in EffectsList)
        {
            if (Effect.Name == Name)
            {
                if (State) { Effect.MainModule.loop = true; Effect.Effect.SetActive(true); }
                else Effect.MainModule.loop = false;
            }
        }
    }

    public void Dive_Death_0() => SetEffect("AirBubbles", false);
    public void Dive_Death_1() => Instantiate(GameScript.FindGamePrefab("WaterSplash"), new Vector3(_Transform.position.x - 2.4f, _Transform.position.y - 0.25f), Quaternion.identity);
}
