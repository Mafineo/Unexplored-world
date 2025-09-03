using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using System.IO;

public class GameScript : MonoBehaviour
{
    [Header("Параметры")]
    public GameObject MainCamera;
    public GameObject MainBook;

    [HideInInspector] public static GameScript _GameScript;
    [HideInInspector] public static EnviromentScript _EnviromentScript;
    [HideInInspector] public static MainBookScript _MainBookScript;
    [HideInInspector] public static Camera _Camera;
    [HideInInspector] public static CameraScript _CameraScript;
    [HideInInspector] public static VillageClass Village;
    private string SavePath;

    [HideInInspector] public static HumanScript _HumanScript;
    private const int CurrentBandleVersionCode = 7;

    [HideInInspector] public static bool Loading = false;
    [HideInInspector] public static float SkipTime = 0;
    private DateTime StartSessionTime;

    private void Awake()
    {
        _GameScript = GetComponent<GameScript>();
        _EnviromentScript = GetComponent<EnviromentScript>();
        _MainBookScript = MainBook.GetComponent<MainBookScript>();
        _Camera = MainCamera.GetComponent<Camera>();
        _CameraScript = MainCamera.GetComponent<CameraScript>();
        _HumanScript = FindGamePrefab("Human").GetComponent<HumanScript>();
    }

    private void Start() => Load();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7)) Village.Villagers[Village.GetFisrtVillager()].Hit(100000, ObjectScript.DamageTypes.Starve);
    }

    private void Load()
    {
        Village = new VillageClass();
        StartSessionTime = DateTime.Now;
        _MainBookScript.Load();
        if (Application.platform == RuntimePlatform.Android) SavePath = Path.Combine(Application.persistentDataPath, "Village.json");
        else SavePath = Path.Combine(Application.dataPath, "Files/Village.json");
        if (File.Exists(SavePath))
        {
            VillageClass.SaveClass _Save = JsonUtility.FromJson<VillageClass.SaveClass>(File.ReadAllText(SavePath));
            if (_Save.BandleVersionCode >= 7)
            {
                Loading = true;
                foreach (string Item in _Save.Items)
                {
                    VillageClass.SaveClass.ItemClass _Item = JsonUtility.FromJson<VillageClass.SaveClass.ItemClass>(Item);
                    ItemScript Script = FindGameItem(_Item.ItemName);
                    if (Script) Village.GiveItem(Script, _Item.ItemCount);
                }
                for (int VillagerID = 0; VillagerID < _Save.Villagers.Length; VillagerID++)
                {
                    VillageClass.VillagerClass.SaveClass _Villager = JsonUtility.FromJson<VillageClass.VillagerClass.SaveClass>(_Save.Villagers[VillagerID]);
                    ItemScript[] Items = new ItemScript[_Villager.Items.Length];
                    for (int ItemID = 0; ItemID < _Villager.Items.Length; ItemID++) Items[ItemID] = (String.IsNullOrEmpty(_Villager.Items[ItemID])) ? null : FindGameItem(_Villager.Items[ItemID]);
                    Village.AddVillager(_Villager.ID, _Villager.Name, _Villager.Health, _Villager.Satiety, JsonUtility.FromJson<VillageClass.VillagerClass.CharacteristicsClass>(_Villager.Characteristics), Items, _Villager.IsOutside, Enum.IsDefined(typeof(EnviromentScript.Locations), _Villager.Location) ? (EnviromentScript.Locations)Enum.Parse(typeof(EnviromentScript.Locations), _Villager.Location) : _EnviromentScript.Location, _Villager.Death);
                }
                VillageClass.SaveClass.LocationClass Location = _Save.FindLocation(_EnviromentScript.Location.ToString());
                if (Location != null && !String.IsNullOrEmpty(Location.FreeTime))
                {
                    TimeSpan FreeTime = JsonUtility.FromJson<VillageClass.SaveClass.LocationClass.TimeClass>(Location.FreeTime).Date - new DateTime(1, 1, 1, 1, 1, 1);
                    SkipTime = (float)FreeTime.TotalSeconds;
                }
                switch (_EnviromentScript.Location)
                {
                    case EnviromentScript.Locations.Valley:
                        LoadObjects(Location.Objects);
                        _EnviromentScript.Load(_Save.Enviroment);
                        break;
                    case EnviromentScript.Locations.Cave:
                        if (Location == null)
                        {
                            Village.CreateNewObject("CaveDescent");
                            _EnviromentScript.Load(null);
                        }
                        else
                        {
                            LoadObjects(Location.Objects);
                            _EnviromentScript.Load(_Save.Enviroment);
                        }
                        break;
                }
            }
            else CreateVillage();
        }
        else CreateVillage();
        VillagersControllScript._VillagersControllScript.SkipTimeForAllVillagers();
        Loading = false;
        SkipTime = 0;
        void CreateVillage()
        {
            for (int VillagerID = 0; VillagerID < 3; VillagerID++) Village.AddVillager();
            GameObject Building = Village.CreateNewObject("TownHall");
            Building = Village.CreateNewObject("Mine");
            Building.transform.position = new Vector3(3.2f, 0, Building.transform.position.z);
            Village.GiveItem(FindGameItem("Stone"), 100);
            Village.GiveItem(FindGameItem("Log"), 100);
            Village.GiveItem(FindGameItem("Clay"), 100);
            Village.GiveItem(FindGameItem("Wheat"), 100);
            Village.GiveItem(FindGameItem("Fiber"), 100);
            Village.GiveItem(FindGameItem("Shovel"), 3);
            Village.GiveItem(FindGameItem("Axe"), 3);
            Village.GiveItem(FindGameItem("Pickaxe"), 3);
            Village.GiveItem(FindGameItem("Hoe"), 2);
            Village.GiveItem(FindGameItem("Flour"), 3);
            Village.GiveItem(FindGameItem("Handbag"), 1);
            Village.GiveItem(FindGameItem("Coal"), 5);
            Village.GiveItem(FindGameItem("IronOre"), 100);
            Village.GiveItem(FindGameItem("GoldOre"), 100);
            Village.GiveItem(FindGameItem("FarmClothes"), 4);
            Village.GiveItem(FindGameItem("WorkClothes"), 5);
            _EnviromentScript.Load(null);
        }
        void LoadObjects(string [] Objects)
        {
            foreach (string Object in Objects)
            {
                if (!String.IsNullOrEmpty(Object))
                {
                    string[] Words = Object.Split('|');
                    Village.CreateNewObject(Words[0], Words[1]);
                }
            }
        }
    }

    public void Save()
    {
        DateTime FinishSessionTime = DateTime.Now;
        VillageClass.SaveClass _OldSave = (File.Exists(SavePath)) ? JsonUtility.FromJson<VillageClass.SaveClass>(File.ReadAllText(SavePath)) : new VillageClass.SaveClass();
        VillageClass.SaveClass _Save = new VillageClass.SaveClass();
        _Save.BandleVersionCode = CurrentBandleVersionCode;
        _Save.Items = new string[Village.Items.Count];
        int ItemID = 0;
        foreach(KeyValuePair<string, int> Item in Village.Items)
        {
            VillageClass.SaveClass.ItemClass _Item = new VillageClass.SaveClass.ItemClass(Item.Key, Item.Value);
            _Save.Items[ItemID] = JsonUtility.ToJson(_Item);
            ItemID++;
        }

        _Save.VillagersCount = Village.VillagersCount;
        _Save.Villagers = new string[Village.Villagers.Count];
        int VillagerID = 0;
        foreach(KeyValuePair<int, VillageClass.VillagerClass> Villager in Village.Villagers)
        {
            string[] Items = new string[Villager.Value.Items.Length];
            for (ItemID = 0; ItemID < Villager.Value.Items.Length; ItemID++) Items[ItemID] = (Villager.Value.Items[ItemID]) ? Villager.Value.Items[ItemID].name : null;
            VillageClass.VillagerClass.SaveClass _Villager = new VillageClass.VillagerClass.SaveClass(Villager.Key, Villager.Value.Name, Villager.Value.Health, Villager.Value.Satiety, JsonUtility.ToJson(Villager.Value.Characteristics), Items, Villager.Value.IsOutside, Villager.Value.Location.ToString(), Villager.Value.Death);
            _Save.Villagers[VillagerID] = JsonUtility.ToJson(_Villager);
            VillagerID++;
        }

        bool LocationSaved = false;
        _Save.Locations = _OldSave.Locations;
        for (int LocationID = 0; LocationID < _Save.Locations.Length; LocationID++)
        {
            VillageClass.SaveClass.LocationClass Location = JsonUtility.FromJson<VillageClass.SaveClass.LocationClass>(_OldSave.Locations[LocationID]);
            if (Location.LocationName == _EnviromentScript.Location.ToString())
            {
                Location.Objects = LocationObjects();
                Location.FreeTime = null;
                _Save.Locations[LocationID] = JsonUtility.ToJson(Location);
                LocationSaved = true;
            }
            else
            {
                if (!String.IsNullOrEmpty(Location.FreeTime))
                {
                    VillageClass.SaveClass.LocationClass.TimeClass Time = JsonUtility.FromJson<VillageClass.SaveClass.LocationClass.TimeClass>(Location.FreeTime);
                    DateTime ResultDate = Time.Date.Add(FinishSessionTime - StartSessionTime);
                    Location.FreeTime = JsonUtility.ToJson(new VillageClass.SaveClass.LocationClass.TimeClass(ResultDate));
                }
                else Location.FreeTime = JsonUtility.ToJson(new VillageClass.SaveClass.LocationClass.TimeClass(new DateTime(1, 1, 1, 1, 1, 1).Add(FinishSessionTime - StartSessionTime)));
            }
            _Save.Locations[LocationID] = JsonUtility.ToJson(Location);
        }
        if (!LocationSaved)
        {
            _Save.Locations = ExpandArray(_Save.Locations);
            _Save.Locations[_OldSave.Locations.Length] = JsonUtility.ToJson(new VillageClass.SaveClass.LocationClass(_EnviromentScript.Location.ToString(), LocationObjects()));
        }
        _Save.Enviroment = _EnviromentScript.Save();
        File.WriteAllText(SavePath, JsonUtility.ToJson(_Save));
        string[] LocationObjects()
        {
            string[] Objects = new string[Village.Objects.Count];
            for (int ObjectID = 0; ObjectID < Village.Objects.Count; ObjectID++)
            {
                if (Village.Objects[ObjectID])
                {
                    ObjectScript Script = Village.Objects[ObjectID].GetComponent<ObjectScript>();
                    Objects[ObjectID] = Script.Save();
                    if (Village.Objects[ObjectID].TryGetComponent(out BuildScript BuildScript) && BuildScript.BuildPrepareMode) Objects[ObjectID] = null;
                }
                else Objects[ObjectID] = null;
            }
            return Objects;
        }
    }

    private void OnApplicationPause(bool pause) { if (pause && !String.IsNullOrEmpty(SavePath)) Save(); }
    private void OnApplicationQuit() { if (!String.IsNullOrEmpty(SavePath)) Save(); }

    [Serializable]
    public class VillageClass
    {
        public int VillagersCount = 0;
        public Dictionary<string, int> Items = new Dictionary<string, int>();
        public Dictionary<int, VillagerClass> Villagers = new Dictionary<int, VillagerClass>();
        public List<GameObject> Objects = new List<GameObject>();
        public Action ItemsChangedEvent;
        public Action VillagersListChangedEvent;
        public Action<int> VillagerDeletedEvent;
        public bool CheckItem(ItemScript Item)
        {
            if (!Item || !Items.ContainsKey(Item.name)) { Items.Add(Item.name, 0); return false; }
            else return true;
        }
        public int GetItemsCount(ItemScript Item)
        {
            if (!CheckItem(Item)) return 0;
            else return Items[Item.name];
        }
        public bool GiveItem(ItemScript Item, int Count)
        {
            if (!Item) return false;
            CheckItem(Item);
            if (Count < 0) if (Items[Item.name] < Math.Abs(Count)) return false;
            Items[Item.name] += Count;
            ItemsChangedEvent?.Invoke();
            return true;
        }
        public bool GetItems(RecipeScript Recipe)
        {
            foreach (ItemsPackScript.ItemClass Ingredient in Recipe.Items.Items) { CheckItem(Ingredient.Item); if (Items[Ingredient.Item.name] < Ingredient.Count) return false; }
            foreach (ItemsPackScript.ItemClass Ingredient in Recipe.Items.Items) Items[Ingredient.Item.name] -= Ingredient.Count;
            ItemsChangedEvent?.Invoke();
            return true;
        }
        public GameObject CreateNewObject(string Name, string SaveString = null, bool UseSaveString = true)
        {
            if (FindGamePrefab(Name))
            {
                GameObject Object = Instantiate(FindGamePrefab(Name), new Vector3(), Quaternion.identity);
                ObjectScript Script = Object.GetComponent<ObjectScript>();
                Script.ID = Objects.Count;
                Script.LoadComponents();
                Objects.Add(Object);
                if (UseSaveString) Script.Load(SaveString);
                return Object;
            }
            else return null;
        }
        public void DestroyObject(int ID)
        {
            if (ID < Objects.Count)
            {
                GameObject Object = Objects[ID];
                Objects.Remove(Objects[ID]);
                Destroy(Object);
                for (int ObjectID = ID; ObjectID < Objects.Count; ObjectID++) Objects[ObjectID].GetComponent<ObjectScript>().ID--;
            }
        }
        public void PlaceNewBuilding(string Name)
        {
            if (FindGamePrefab(Name) && _MainBookScript.MainBookIsStatic && IsVillageAlive)
            {
                GameObject Building = CreateNewObject(Name, UseSaveString: false);
                Building.GetComponent<BuildScript>().BuildPrepareMode = true;
                _MainBookScript.TakeForConfirm();
                ConfirmPageScript _ConfirmPageScript = _MainBookScript.FindPage(_MainBookScript.FindPageID("Confirm"), true).GetComponent<ConfirmPageScript>();
                _ConfirmPageScript._Description.text = "Перетягивай здание\nи подтверди постройку\n";
                _ConfirmPageScript._BigPictures[0].Load(ItemPictureScript.PictureTypes.Button, Action: delegate { if (IsVillageAlive) { Building.GetComponent<BuildScript>().BuildPrepareMode = false; } });
                _ConfirmPageScript._BigPictures[1].Load(ItemPictureScript.PictureTypes.Button, Action: delegate { Building.GetComponent<BuildScript>().CancelBuild(); });
            }
        }
        public GameObject FindObject(string Name)
        {
            foreach (GameObject Object in Objects) if (Object && Object.GetComponent<ObjectScript>().PrefabName == Name) return Object;
            return null;
        }
        public void AddVillager()
        {
            Villagers.Add(VillagersCount, new VillagerClass(Villagers.Count));
            VillagersCount++;
            VillagersListChangedEvent?.Invoke();
        }
        public void AddVillager(int ID, string Name, int Health, int Satiety, VillagerClass.CharacteristicsClass Characteristics, ItemScript[] Items, bool IsOutside, EnviromentScript.Locations Location, string Death)
        {
            Villagers.Add(ID, new VillagerClass(ID, Name, Health, Satiety, Characteristics, Items, IsOutside, Location, Death));
            VillagersListChangedEvent?.Invoke();
        }
        public int GetFisrtVillager()
        {
            foreach (KeyValuePair<int, VillagerClass> Villager in Village.Villagers) return Villager.Key;
            return 0;
        }
        public bool IsVillageAlive
        {
            get
            {
                bool HaveAliveVillager = false;
                foreach (KeyValuePair<int, VillagerClass> Villager in Village.Villagers)
                {
                    if (Villager.Value.Health > 0)
                    {
                        HaveAliveVillager = true;
                        break;
                    }
                }
                return (Villagers.Count > 0 && HaveAliveVillager);
            }
        }
        public class VillagerClass
        {
            private int _ID;
            public int ID
            {
                get { return _ID; }
                set
                {
                    _ID = value;
                    if (Human) _Script.VillagerID = _ID;
                }
            }
            public string Name;
            private int _Health;
            public int Health
            {
                get   { return (Human) ? _Script.Health : _Health; }
                set
                {
                    _Health = value;
                    if (_Health <= 0) _Health = 0;
                    else if (_Health > _HumanScript.MaxHealth) _Health = _HumanScript.MaxHealth;
                    if (Human) _Script.Health = _Health;
                    Village.VillagersListChangedEvent?.Invoke();
                }
            }
            public void Hit(int Damage, ObjectScript.DamageTypes DamageType)
            {
                if (!Human)
                {
                    GoOutside(false);
                    _Script.Hit(Damage, DamageType);
                    GoInside();
                }
                else _Script.Hit(Damage, DamageType);
            }
            private int _Satiety;
            public int Satiety
            {
                get { return _Satiety; }
                set
                {
                    _Satiety = value;
                    if (_Satiety < 0) _Satiety = 0;
                    else if (_Satiety > 1000) _Satiety = 1000;
                    Village.VillagersListChangedEvent?.Invoke();
                }
            }
            public CharacteristicsClass Characteristics;
            public ItemScript[] Items;
            public bool IsOutside;
            public EnviromentScript.Locations Location;
            public string Death;
            public HumanScript _Script;
            private GameObject _Human;
            public GameObject Human
            {
                get { return _Human; }
                set
                {
                    _Human = value;
                    _Script = _Human.GetComponent<HumanScript>();
                }
            }
            public VillagerClass(int ID)
            {
                this.ID = ID;
                Name = GameVillagersNames[ID];
                Health = _HumanScript.MaxHealth;
                Satiety = 1000;
                Characteristics = new CharacteristicsClass();
                Items = new ItemScript[6];
                IsOutside = false;
                Location = _EnviromentScript.Location;
                Death = null;
            }
            public VillagerClass(int ID, string Name, int Health, int Satiety, CharacteristicsClass Characteristics, ItemScript[] Items, bool IsOutside, EnviromentScript.Locations Location, string Death)
            {
                this.ID = ID;
                this.Name = Name;
                this.Health = Health;
                this.Satiety = Satiety;
                this.Characteristics = Characteristics;
                this.Items = Items;
                this.IsOutside = IsOutside;
                this.Location = Location;
                this.Death = Death;
            }
            public GameObject GoOutside(bool GetLocation = true)
            {
                IsOutside = true;
                if (!Human)
                {
                    Human = Village.CreateNewObject("Human");
                    if (GetLocation) Location = _EnviromentScript.Location;
                    _Script.Health = _Health;
                    if (Items[1]) _Script.SetClothes(Items[1].name);
                    _Script.VillagerID = ID;
                    _Script.NameText.text = Name;
                    Village.VillagersListChangedEvent?.Invoke();
                }
                return Human;
            }
            public void GoInside()
            {
                IsOutside = false;
                Health = _Script.Health;
                Village.DestroyObject(_Script.ID);
                Village.VillagersListChangedEvent?.Invoke();
            }
            public bool UseItem(ItemScript Item)
            {
                if (Health > 0)
                {
                    if (Item.HealCount > 0 || Item.FeedCount > 0)
                    {
                        if (!IsOutside)
                        {
                            if (Village.GiveItem(Item, -1))
                            {
                                Health += Item.HealCount;
                                Satiety += Item.FeedCount;
                                Village.VillagersListChangedEvent?.Invoke();
                                Village.ItemsChangedEvent?.Invoke();
                                return true;
                            }
                        }
                        int MaxItems = 0;
                        if (Items[2])
                        {
                            switch (Items[2].name)
                            {
                                case "Handbag": MaxItems = 5; break;
                                case "Bag": MaxItems = 6; break;
                                default: MaxItems = 3; break;
                            }
                        }
                        for (int ItemID = 0; ItemID < MaxItems; ItemID++)
                        {
                            if (Items[ItemID] == Item)
                            {
                                Health += Item.HealCount;
                                Satiety += Item.FeedCount;
                                Items[ItemID] = null;
                                Village.VillagersListChangedEvent?.Invoke();
                                Village.ItemsChangedEvent?.Invoke();
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            public bool SetItem(int SlotID, ItemScript Item, bool Swap = false)
            {
                if (!Swap && !IsOutside)
                {
                    if (!Item || Village.Items[Item.name] > 0)
                    {
                        Village.GiveItem(Item, -1);
                        Village.GiveItem(Items[SlotID], 1);
                        if (SlotID == 2)
                        {
                            if (Items[2])
                            {
                                if (Items[2].name == "Bag")
                                {
                                    if (!Item)
                                    {
                                        for (int ItemID = 3; ItemID < 6; ItemID++)
                                        {
                                            Village.GiveItem(Items[ItemID], 1);
                                            Items[ItemID] = null;
                                        }
                                    }
                                    else if (Item.name == "Handbag")
                                    {
                                        Village.GiveItem(Items[5], 1);
                                        Items[5] = null;
                                    }
                                }
                                else if (Items[2].name == "Handbag")
                                {
                                    if (!Item || Item && Item.name != "Bag")
                                    {
                                        for (int ItemID = 3; ItemID < 6; ItemID++)
                                        {
                                            Village.GiveItem(Items[ItemID], 1);
                                            Items[ItemID] = null;
                                        }
                                    }
                                }
                            }
                        }
                        Items[SlotID] = Item;
                        Village.VillagersListChangedEvent?.Invoke();
                        Village.ItemsChangedEvent?.Invoke();
                        return true;
                    }
                }
                int MaxItems = 0;
                if (Items[2])
                {
                    switch (Items[2].name)
                    {
                        case "Handbag": MaxItems = 5; break;
                        case "Bag": MaxItems = 6; break;
                        default: MaxItems = 3; break;
                    }
                }
                for (int ItemID = 0; ItemID < MaxItems; ItemID++)
                {
                    if (Items[ItemID] == Item && ItemID != SlotID)
                    {
                        if (ItemID > 1 || ItemID == 0 && Items[SlotID].IsInstrument || ItemID == 1 && Items[SlotID].IsClothes)
                        {
                            Items[ItemID] = Items[SlotID];
                            Items[SlotID] = Item;
                            Village.VillagersListChangedEvent?.Invoke();
                            Village.ItemsChangedEvent?.Invoke();
                            return true;
                        }
                    }
                }
                return false;
            }
            public bool HasItem(ItemScript Item)
            {
                foreach (ItemScript FindItem in Items) if (FindItem == Item) return true;
                return false;
            }
            public int InventoryItemsCount(ItemScript Item)
            {
                int ItemCount = 0;
                if (!IsOutside) ItemCount += Village.Items[Item.name];
                foreach (ItemScript FindItem in Items) if (FindItem == Item) ItemCount++;
                return ItemCount;
            }
            public bool HasItems(ItemScript[][] Items)
            {
                if (!IsOutside || Human && Human.GetComponent<HumanScript>().IsReturning)
                {
                    for (int GroupID = 0; GroupID < Items.GetLength(0); GroupID++)
                    {
                        for (int ItemID = 0; ItemID < Items[GroupID].Length; ItemID++)
                        {
                            if (HasItem(Items[GroupID][ItemID])) break;
                            else if (ItemID == Items[GroupID].Length - 1) return false;
                        }
                    }
                    return true;
                }
                return false;
            }
            public class CharacteristicsClass
            {
                public int Strength;
                public int Agility;
                public int Intelligence;
                public CharacteristicsClass()
                {
                    Strength = 2;
                    Agility = 2;
                    Intelligence = 2;
                }
                public CharacteristicsClass(int Strength, int Agility, int Intelligence)
                {
                    this.Strength = Strength;
                    this.Agility = Agility;
                    this.Intelligence = Intelligence;
                }
            }
            public class SaveClass
            {
                public int ID;
                public string Name;
                public int Health;
                public int Satiety;
                public string Characteristics;
                public string[] Items;
                public bool IsOutside;
                public string Location;
                public string Death;
                public SaveClass(int ID, string Name, int Health, int Satiety, string Characteristics, string[] Items, bool IsOutside, string Location, string Death)
                {
                    this.ID = ID;
                    this.Name = Name;
                    this.Health = Health;
                    this.Satiety = Satiety;
                    this.Characteristics = Characteristics;
                    this.Items = Items;
                    this.IsOutside = IsOutside;
                    this.Location = Location;
                    this.Death = Death;
                }
                public class DeathClass
                {
                    public float Position;
                    public bool Rotation;
                    public string DamageType;
                    public DeathClass(float Position, bool Rotation, string DamageType)
                    {
                        this.Position = Position;
                        this.Rotation = Rotation;
                        this.DamageType = DamageType;
                    }
                }

            }
        }
        public class SaveClass
        {
            public int BandleVersionCode;
            public int VillagersCount;
            public string[] Items;
            public string[] Villagers;
            public string[] Locations = new string[0];
            public string Enviroment;
            public LocationClass FindLocation(string Name)
            {
                foreach(string Location in Locations)
                {
                    LocationClass _LocationClass = JsonUtility.FromJson<LocationClass>(Location);
                    if (_LocationClass.LocationName == Name) return _LocationClass; 
                }
                return null;
            }
            public class ItemClass
            {
                public string ItemName;
                public int ItemCount;
                public ItemClass(string ItemName, int ItemCount)
                {
                    this.ItemName = ItemName;
                    this.ItemCount = ItemCount;
                }
            }
            public class LocationClass
            {
                public string LocationName;
                public string[] Objects = new string[0];
                public string FreeTime;
                public LocationClass(string LocationName, string[] Objects)
                {
                    this.LocationName = LocationName;
                    this.Objects = Objects;
                }
                public class TimeClass
                {
                    public int Year;
                    public int Month;
                    public int Day;
                    public int Hour;
                    public int Minute;
                    public int Second;
                    public TimeClass(DateTime Date)
                    {
                        Year = Date.Year;
                        Month = Date.Month;
                        Day = Date.Day;
                        Hour = Date.Hour;
                        Minute = Date.Minute;
                        Second = Date.Second;
                    }
                    public DateTime Date { get { return new DateTime(Year, Month, Day, Hour, Minute, Second); } }
                }
            }
        }
    }

    public static GameObject FindGamePrefab(string Name)
    {
        switch (Name)
        {
            case "TownHall": return Resources.Load<GameObject>("Prefabs/Buildings/TownHall");
            case "Mine": return Resources.Load<GameObject>("Prefabs/Buildings/Mine");
            case "GardenBed": return Resources.Load<GameObject>("Prefabs/Buildings/GardenBed");
            case "Mill": return Resources.Load<GameObject>("Prefabs/Buildings/Mill");
            case "Bakery": return Resources.Load<GameObject>("Prefabs/Buildings/Bakery");
            case "Foundry": return Resources.Load<GameObject>("Prefabs/Buildings/Foundry");
            case "Human": return Resources.Load<GameObject>("Prefabs/Creatures/Human");
            case "EarthBlock": return Resources.Load<GameObject>("Prefabs/World/EarthBlock");
            case "AppleTree": return Resources.Load<GameObject>("Prefabs/World/AppleTree");
            case "Stone": return Resources.Load<GameObject>("Prefabs/World/Stone");
            case "Bush": return Resources.Load<GameObject>("Prefabs/World/Bush");
            case "CaveDescent": return Resources.Load<GameObject>("Prefabs/Buildings/CaveDescent");
            case "Coal": return Resources.Load<GameObject>("Prefabs/World/Coal");
            case "IronOre": return Resources.Load<GameObject>("Prefabs/World/IronOre");
            case "GoldOre": return Resources.Load<GameObject>("Prefabs/World/GoldOre");
            case "Lake": return Resources.Load<GameObject>("Prefabs/World/Lake");
            case "BuildSmoke": return Resources.Load<GameObject>("Prefabs/Effects/BuildSmoke");
            case "WaterSplash": return Resources.Load<GameObject>("Prefabs/Effects/WaterSplash");
            default: return null;
        }
    }

    public static ItemScript FindGameItem(string Name) { return Resources.Load<ItemScript>("Scripts/DataScripts/Items/" + Name); }

    public static bool TryChanse(float Probability)
    {
        if (UnityEngine.Random.Range(0, 100) < Probability) return true;
        else return false;
    }

    public static bool DistanceIsDone(Vector2 StartPoint, Vector2 FinishPoint, float Distance = 0.001f)
    {
        if (Mathf.Abs(StartPoint.x - FinishPoint.x) < Distance && Mathf.Abs(StartPoint.y - FinishPoint.y) < Distance) return true;
        else return false;
    }

    public static T[] ExpandArray<T>(T[] Array)
    {
        T[] Answer = new T[Array.Length + 1];
        for (int Index = 0; Index < Array.Length; Index++) Answer[Index] = Array[Index];
        return Answer;
    }

    public static string[] GameVillagersNames = { "Алекс", "Авель", "Андре", "Адольф", "Август", "Армин" };
}
