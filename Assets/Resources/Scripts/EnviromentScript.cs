using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public class EnviromentScript : MonoBehaviour
{
    [Header("Настройки")]
    public Locations Location;
    public enum Locations { Valley, Cave }
    [Space(5f)]
    [Header("Детали")]
    public GameObject Background;
    public GameObject Earth;
    public GameObject Sun;
    public GameObject RainEffect;
    public GameObject SnowfallEffect;
    public GameObject[] CutScenes;

    private Coroutine _StartWindSpeedChange;
    private float _WindSpeed;
    public float WindSpeed
    {
        get { return _WindSpeed; }
        set
        {
            if (_StartWindSpeedChange != null) StopCoroutine(_StartWindSpeedChange);
            _StartWindSpeedChange = StartCoroutine(StartWindSpeedChange(value));
            IEnumerator StartWindSpeedChange(float TargetSpeed) 
            {
                while (Math.Abs(_WindSpeed - TargetSpeed) > 0.001f)
                {
                    if (_WindSpeed > TargetSpeed) _WindSpeed -= 0.01f;
                    else _WindSpeed += 0.01f;
                    yield return new WaitForSeconds(0.06f);
                }
            }
        }
    }

    private float TimeOfDay;
    public enum DayPhases { Morning, Afternoon, Evening, Night }
    [HideInInspector] public DayPhases DayPhase;
    private int Days = 1;
    public enum Seasons { Summer, Winter }
    private Seasons Season;
    private bool Precipitation;

    public Action ChangeDayCycleEvent;

    private Animator _BackgroundAnimator;
    private Animator _EarthAnimator;
    private Animator _SunAnimator;
    private ParticleSystem _RainEffect;
    private ParticleSystem.MainModule _RainEffectMainModule;
    private ParticleSystem _SnowfallEffect;
    private ParticleSystem.MainModule _SnowfallEffectMainModule;
    [HideInInspector] public AsyncOperation _AsyncOperation;
    public enum SunStates { Day, Night, MainlyCloud }

    private void Awake()
    {
        _BackgroundAnimator = Background.GetComponent<Animator>();
        _EarthAnimator = Earth.GetComponent<Animator>();
        _SunAnimator = Sun.GetComponent<Animator>();
        _RainEffect = RainEffect.GetComponent<ParticleSystem>();
        _RainEffectMainModule = _RainEffect.main;
        _SnowfallEffect = SnowfallEffect.GetComponent<ParticleSystem>();
        _SnowfallEffectMainModule = _SnowfallEffect.main;
        _AsyncOperation = null;
    }

    private void Update()
    {
        TimeOfDay += Time.deltaTime;
        if (TimeOfDay >= 75)
        {
            TimeOfDay -= 75;
            if (DayPhase == DayPhases.Night) DayPhase = 0;
            else DayPhase++;
            if (DayPhase == DayPhases.Morning || DayPhase == DayPhases.Night)
            {
                _BackgroundAnimator.SetBool("Night", DayPhase == DayPhases.Night);
                ChangeDayCycleEvent?.Invoke();
                if (DayPhase == DayPhases.Morning)
                {
                    Days++;
                    if (Days % 10 == 0)
                    {
                        switch (Season)
                        {
                            case Seasons.Summer: Season = Seasons.Winter; break;
                            case Seasons.Winter: Season = Seasons.Summer; break;
                        }
                    }
                }
            }
            SetEnviroment(DayPhase, Season, GameScript.TryChanse(20));
        }
    }

    private void SetEnviroment(DayPhases DayPhase, Seasons Season, bool Precipitation, bool Loading = false)
    {
        _BackgroundAnimator.SetBool("Night", DayPhase == DayPhases.Night);
        if (Location == Locations.Valley) _EarthAnimator.SetBool("Winter", Season == Seasons.Winter);
        SetPrecipitation(Precipitation);
        if (!Loading && this.Season != Season)
        {
            SetPrecipitation(false);
            SetPrecipitation(true);
        }
        this.Season = Season;
        SetWindSpeed(Loading);
        SetSunLight(Loading);

        void SetWindSpeed(bool Loading)
        {
            float Speed = 0;
            if (Location == Locations.Cave) Speed = 0.2f;
            else
            {
                switch (Season)
                {
                    case Seasons.Summer:
                        if (Precipitation) Speed = 2.5f;
                        else Speed = 1f;
                        break;
                    case Seasons.Winter:
                        if (Precipitation) Speed = 2f;
                        else Speed = 1f;
                        break;
                }
            }
            if (Loading) _WindSpeed = Speed;
            else WindSpeed = Speed;
        }
        void SetSunLight(bool Loading)
        {
            if (Location == Locations.Cave) _SunAnimator.SetInteger("LightID", (int)SunStates.Night);
            else
            {
                if (DayPhase == DayPhases.Night) _SunAnimator.SetInteger("LightID", (int)SunStates.Night);
                else
                {
                    switch (Season)
                    {
                        case Seasons.Summer:
                            if (Precipitation) _SunAnimator.SetInteger("LightID", (int)SunStates.MainlyCloud);
                            else _SunAnimator.SetInteger("LightID", (int)SunStates.Day);
                            break;
                        case Seasons.Winter:
                            _SunAnimator.SetInteger("LightID", (int)SunStates.Day);
                            break;
                    }
                }
            }
            if (Loading) _SunAnimator.Play("Night");
        }
    }

    private void SetPrecipitation(bool Start, bool Loading = false)
    {
        if (Location != Locations.Cave)
        {
            if (Start)
            {
                switch (Season)
                {
                    case Seasons.Summer:
                        _BackgroundAnimator.SetBool("MainlyCloudy", true);
                        if (Loading) _RainEffectMainModule.prewarm = true;
                        _RainEffectMainModule.loop = true;
                        RainEffect.SetActive(true);
                        break;
                    case Seasons.Winter:
                        _BackgroundAnimator.SetBool("MainlyCloudy", false);
                        if (Loading) _SnowfallEffectMainModule.prewarm = true;
                        _SnowfallEffectMainModule.loop = true;
                        SnowfallEffect.SetActive(true);
                        break;
                }
            }
            else
            {
                _BackgroundAnimator.SetBool("MainlyCloudy", false);
                _RainEffectMainModule.prewarm = false;
                _RainEffectMainModule.loop = false;
                _SnowfallEffectMainModule.prewarm = false;
                _SnowfallEffectMainModule.loop = false;
            }
        }
        Precipitation = Start;
    }

    private void GenerateObstacles()
    {
        GameObject Obstacle;
        Dictionary<string, int> Obstacles = new Dictionary<string, int>();
        float SetPosition = 0;
        bool AdditionTree = GameScript.TryChanse(50);
        for (int Side = -1; Side <= 1; Side += 2)
        {
            SetPosition = ((GameScript._EnviromentScript.Location == EnviromentScript.Locations.Cave) ? 3.2f : 6.4f) * Side;
            for (int Stage = 0; Stage < 2; Stage++)
            {
                if (GameScript._EnviromentScript.Location == EnviromentScript.Locations.Cave) Obstacles = new Dictionary<string, int>() { { "Space", 2 }, { "Stone", 2 }, { "Coal", 1 }, { "IronOre", 1 }, { "GoldOre", 1 } };
                else
                {
                    Obstacles = new Dictionary<string, int>() { { "Space", 1 }, { "Stone", 1 }, { "AppleTree", 1 }, { "Bush", 2 }, { "Lake", 1 } };
                    if (Side == -1)
                    {
                        if (Stage == 0)
                        {
                            if (AdditionTree) Obstacles["AppleTree"]++;
                            else Obstacles["Stone"]++;
                        }
                        else
                        {
                            if (AdditionTree) Obstacles["Stone"]++;
                            else Obstacles["AppleTree"]++;
                            Obstacles["Lake"]--;
                        }
                    }
                    else
                    {
                        if (Stage == 0)
                        {
                            if (AdditionTree) Obstacles["Stone"]++;
                            else Obstacles["AppleTree"]++;
                        }
                        else
                        {
                            if (AdditionTree) Obstacles["AppleTree"]++;
                            else Obstacles["Stone"]++;
                            Obstacles["Lake"]--;
                        }
                    }
                }
                while (Obstacles.Values.Max() > 0)
                {
                    int RandomInteger = UnityEngine.Random.Range(0, Obstacles.Count);
                    if (Obstacles.ElementAt(RandomInteger).Value > 0)
                    {
                        string Name = Obstacles.ElementAt(RandomInteger).Key;
                        if (Name == "Space")
                        {
                            SetPosition += 1.6f * Side;
                            Obstacles[Name]--;
                        }
                        else if (Name != "Lake" || Name == "Lake" && Obstacles.Values.Sum() == 1)
                        {
                            Obstacle = GameScript.Village.CreateNewObject(Name);
                            ObjectScript Script = Obstacle.GetComponent<ObjectScript>();
                            SetPosition += 0.8f * Script.Width * Side;
                            Obstacle.transform.position = new Vector3(SetPosition, 0, Obstacle.transform.position.z);
                            SetPosition += 0.8f * Script.Width * Side;
                            Obstacles[Name]--;
                        }
                    }
                }
            }
        }
    }

    public void Load(string SaveString)
    {
        if (!String.IsNullOrEmpty(SaveString))
        {
            SaveClass _Save = JsonUtility.FromJson<SaveClass>(SaveString);
            TimeOfDay = _Save.TimeOfDay;
            DayPhase += _Save.DayPhase;
            Days = _Save.Days;
            Season += _Save.Season;
            Precipitation = _Save.Precipitation;
        }
        else
        {
            Days = 1;
            GenerateObstacles();
        }
        SetEnviroment(DayPhase, Season, Precipitation, true);
    }

    public string Save()
    {
        SaveClass _Save = new SaveClass(TimeOfDay, ((int)DayPhase), Days, ((int)Season), Precipitation);
        return JsonUtility.ToJson(_Save);
    }

    public class SaveClass
    {
        public float TimeOfDay;
        public int DayPhase;
        public int Days;
        public int Season;
        public bool Precipitation;
        public SaveClass(float TimeOfDay, int DayPhase, int Days, int Season, bool Precipitation)
        {
            this.TimeOfDay = TimeOfDay;
            this.DayPhase = DayPhase;
            this.Days = Days;
            this.Season = Season;
            this.Precipitation = Precipitation;
        }
    }

    public void FindCutScene(string Name)
    {
        foreach (GameObject CutScene in CutScenes)
        {
            if (CutScene.name == Name)
            {
                CutScene.SetActive(false);
                CutScene.SetActive(true);
            }
        }
    }

    public void StartLoadLocation(string SceneName)
    {
        if (GameScript._MainBookScript.MainBookIsStatic && _AsyncOperation == null) StartCoroutine(StartLoad());
        IEnumerator StartLoad()
        {
            GameScript._GameScript.Save();
            GameScript._MainBookScript.Close();
            GameScript._CameraScript.enabled = false;
            _AsyncOperation = SceneManager.LoadSceneAsync(SceneName);
            _AsyncOperation.allowSceneActivation = false;
            Vector2 TargetPosition = Vector2.zero;
            switch (SceneName)
            {
                case "ValleyScene": TargetPosition = GameScript.Village.FindObject("CaveDescent").transform.position; break;
                case "CaveScene": TargetPosition = GameScript.Village.FindObject("Mine").transform.position; break;
            }
            while (!GameScript.DistanceIsDone(GameScript._CameraScript._Transform.position, new Vector2(TargetPosition.x, 3f)) || GameScript._Camera.orthographicSize < 4f)
            {
                GameScript._CameraScript._Transform.position = Vector3.MoveTowards(GameScript._CameraScript._Transform.position, new Vector3(TargetPosition.x, 3f, -10f), 5f * Time.deltaTime);
                if (GameScript._Camera.orthographicSize < 4f) GameScript._Camera.orthographicSize += 2f * Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            switch (SceneName)
            {
                case "ValleyScene":
                    GameScript._EnviromentScript.FindCutScene("MoveToValley");
                    break;
                case "CaveScene":
                    GameScript._EnviromentScript.FindCutScene("MoveToCave");
                    break;
            }
            _SunAnimator.SetInteger("LightID", (int)SunStates.Night);
        }
    }

    public void EndSceneLoad()
    {
        StartCoroutine(EndLoad());
        IEnumerator EndLoad()
        {
            while (_AsyncOperation.progress < 0.9f || !_SunAnimator.GetCurrentAnimatorStateInfo(0).IsName("Night")) yield return new WaitForEndOfFrame();
            _AsyncOperation.allowSceneActivation = true;
        }
    }
}
