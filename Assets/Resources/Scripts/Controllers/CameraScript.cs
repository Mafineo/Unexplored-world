using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public bool CameraIsFreezed;
    [HideInInspector] public bool CameraIsDraged;

    [HideInInspector] public Transform _Transform;
    private Camera _Camera;
    private Vector3 _StartPosition;

    private void Awake()
    {
        _Transform = GetComponent<Transform>();
        _Camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            Touch[] _Touches = Input.touches;
            if (Input.touchCount > 0)
            {
                switch (_Touches[0].phase)
                {
                    case TouchPhase.Began:
                        _StartPosition = GameScript._Camera.ScreenToWorldPoint(_Touches[0].position);
                        CameraIsDraged = false;
                        break;
                    case TouchPhase.Moved:
                        if (!CameraIsFreezed)
                        {
                            Vector2 DragDistance = GameScript._Camera.ScreenToWorldPoint(_Touches[0].position) - _StartPosition;
                            _Transform.position = new Vector3(Mathf.Clamp(_Transform.position.x - DragDistance.x, -35, 35), Mathf.Clamp(_Transform.position.y - DragDistance.y, -3f + 3f * (1f / 4 * _Camera.orthographicSize), 6f - 3f * (1f / 4 * _Camera.orthographicSize)), -10);
                            if (DragDistance.x != 0 && DragDistance.y != 0) CameraIsDraged = true;
                        }
                        break;
                    case TouchPhase.Ended:
                        if (_Touches.Length > 1)
                        {
                            _StartPosition = GameScript._Camera.ScreenToWorldPoint(_Touches[1].position);
                            CameraIsDraged = false;
                        }
                        break;
                }
            }
            if (Input.touchCount > 1)
            {
                Vector2[] _TouchesDirections = new Vector2[2];
                for (int TouchID = 0; TouchID < _Touches.Length; TouchID++) _TouchesDirections[TouchID] = _Touches[TouchID].position - _Touches[TouchID].deltaPosition;
                var CurrentZoom = _Camera.orthographicSize - (Vector2.Distance(_Touches[0].position, _Touches[1].position) - Vector2.Distance(_TouchesDirections[0], _TouchesDirections[1])) * 0.002f;
                _Camera.orthographicSize = Mathf.Clamp(CurrentZoom, 1, 4);
                _Transform.position = new Vector3(Mathf.Clamp(_Transform.position.x, -35, 35), Mathf.Clamp(_Transform.position.y, -3f + 3f * (1f / 4 * _Camera.orthographicSize), 6f - 3f * (1f / 4 * _Camera.orthographicSize)), -10);
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                _StartPosition = GameScript._Camera.ScreenToWorldPoint(Input.mousePosition);
                CameraIsDraged = false;
            }
            else if (Input.GetMouseButton(0) && !CameraIsFreezed)
            {
                Vector2 DragDistance = GameScript._Camera.ScreenToWorldPoint(Input.mousePosition) - _StartPosition;
                _Transform.position = new Vector3(Mathf.Clamp(_Transform.position.x - DragDistance.x, -35, 35), Mathf.Clamp(_Transform.position.y - DragDistance.y, -3f + 3f * (1f / 4 * _Camera.orthographicSize), 6f - 3f * (1f / 4 * _Camera.orthographicSize)), -10);
                if (DragDistance.x != 0 && DragDistance.y != 0) CameraIsDraged = true;
            }
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                _Camera.orthographicSize = Mathf.Clamp(_Camera.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * 2, 1, 4);
                _Transform.position = new Vector3(Mathf.Clamp(_Transform.position.x, -35, 35), Mathf.Clamp(_Transform.position.y, -3f + 3f * (1f / 4 * _Camera.orthographicSize), 6f - 3f * (1f / 4 * _Camera.orthographicSize)), -10);
            }
        }
    }
}
