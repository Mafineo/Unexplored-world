using UnityEngine;
using System;

public class PageScript : MonoBehaviour
{
    [Header("Найстройки")]
    public bool PageIsImportant;
    public bool PageIsHidden;
    public bool HidePageAfterClose;
    [HideInInspector] public bool PageIsStatic = true;
    [Space(5f)]
    [Header("Локации")]
    public bool HideInCave;

    public Action PageEnabledEvent;

    private void OnEnable() => PageEnabledEvent?.Invoke();
}
