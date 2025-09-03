using UnityEngine;
using System;

public class ObjectScript : MonoBehaviour
{
    [Header("Настройки")]
    public string PrefabName;
    public Sprite Icon;
    public Types Type;
    public enum Types { Building, Creature, Obstacle }
    [Space(5)]
    [Header("Характеристики")]
    public int MaxHealth; private int _Health;
    public int Health
    {
        get { return _Health; }
        set
        {
            _Health = value;
            if (_Health >= MaxHealth) _Health = MaxHealth;
            else if (_Health <= 0) _Health = 0;
            HealthChangesEvent?.Invoke();
        }
    }
    public int Damage; 
    public float MoveSpeed;
    public float AttackSpeed;
    public int Width;

    private int _ID;
    public int ID
    {
        get { return _ID; }
        set
        {
            _ID = value;
            Transform Temp = GetComponent<Transform>();
            Temp.position = new Vector3(Temp.position.x, Temp.position.y, -0.01f * _ID);
        }
    }

    public Action HealthChangesEvent;

    [HideInInspector] public Transform _Transform;
    [HideInInspector] public Animator _Animator;
    [HideInInspector] public BoxCollider2D _BoxCollider2D;
    [HideInInspector] public SelectScript _SelectScript;
    [HideInInspector] public PageScript InformationPage;

    public void LoadComponents()
    {
        _Transform = GetComponent<Transform>();
        if (TryGetComponent(out Animator Animator)) _Animator = Animator;
        else if (_Transform.childCount > 0 && _Transform.GetChild(0).TryGetComponent(out Animator ChildAnimator)) _Animator = ChildAnimator;
        if (TryGetComponent(out BoxCollider2D BoxCollider2D)) _BoxCollider2D = BoxCollider2D;
        if (TryGetComponent(out SelectScript SelectScript)) _SelectScript = SelectScript;
        GameObject _Page = GameScript._MainBookScript.FindPage(GameScript._MainBookScript.FindPageID(PrefabName), true);
        if (_Page) InformationPage = _Page.GetComponent<PageScript>();
    }

    public virtual void Load(string SaveString) { }
    public virtual string Save() { return null; }

    public enum DamageTypes { Starve, Hit, Sleeping, Fell }
    public virtual void Hit(int Damage, DamageTypes DamageType)
    {
        if (Health > 0)
        {
            switch (DamageType)
            {
                case DamageTypes.Sleeping: Health += Damage; break;
                default: Health -= Damage; break;
            }
            if (Health == 0) Die(DamageType);
        }
    }

    public virtual void Die(DamageTypes DamageType) { }
}


