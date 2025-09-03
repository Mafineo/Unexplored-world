using UnityEngine;
using System;

public class SelectScript : MonoBehaviour
{
    public Action OpenMainBookEvent;
    [HideInInspector] public bool SelectIsEnabled = true;

    private ObjectScript _ObjectScript;

    private void Awake() { _ObjectScript = GetComponent<ObjectScript>(); }

    private void OnMouseDown() { if (SelectIsEnabled) ControllerScript.GameController.SelectingObject = true; }

    private void OnMouseUp()
    {
        if (SelectIsEnabled)
        {
            if (GameScript._MainBookScript.MainBookState == MainBookScript.MainBookStates.Closed && !GameScript._CameraScript.CameraIsFreezed && !GameScript._CameraScript.CameraIsDraged && GameScript._CameraScript.enabled)
            {
                if (ControllerScript.GameController.SelectedObject != gameObject) ControllerScript.GameController.SelectedObject = gameObject;
                else OpenObjectPage();
            }
        }
    }

    public void OpenObjectPage()
    {
        ControllerScript.GameController.SelectedObject = null;
        OpenMainBookEvent?.Invoke();
        GameScript._MainBookScript.LoadPagesList();
        if (_ObjectScript) { GameScript._MainBookScript.TargetPage = _ObjectScript.PrefabName; }
    }
}
