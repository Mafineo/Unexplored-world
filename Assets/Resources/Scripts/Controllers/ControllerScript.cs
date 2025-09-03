using UnityEngine;

public class ControllerScript : MonoBehaviour
{
    [HideInInspector] public static ControllerScript GameController;
    private GameObject _SelectedObject;
    public GameObject SelectedObject
    {
        get { return _SelectedObject; }
        set
        {
            if (_SelectedObject != value && _SelectedObject && value) _CursorAnimator.Play("Hiden");
            _SelectedObject = value;
            _CursorAnimator.SetBool("Active", _SelectedObject);
        }
    }
    [HideInInspector] public bool SelectingObject;

    private GameObject _Cursor;
    private Animator _CursorAnimator;

    private void Awake()
    {
        GameController = GetComponent<ControllerScript>();
        _Cursor = transform.GetChild(0).gameObject;
        _CursorAnimator = _Cursor.GetComponent<Animator>();
    }

    private void Update()
    {
        if (SelectedObject) GetComponent<RectTransform>().localPosition = new Vector3(SelectedObject.transform.position.x, 0, 0);
        if (Input.GetMouseButtonUp(0) && !GameScript._CameraScript.CameraIsDraged)
        { 
            if (!SelectingObject) SelectedObject = null;
            SelectingObject = false;
        }
    }
}
