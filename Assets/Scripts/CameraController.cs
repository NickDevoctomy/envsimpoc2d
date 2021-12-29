using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _camera;
    private float _moveSpeed = 10f;
    private float _zoomSpeed = 0.5f;
    private float _dragSpeed = 0.5f;
    private Vector3 _dragOrigin;

    private void Start()
    {
        _camera = GetComponent<Camera>();
        if(Map.Instance != null)
        {
            transform.position = new Vector3(Map.Instance.Width / 2, Map.Instance.Height / 2, transform.position.z);
        }
    }

    void Update()
    {
        if(ApplyKeyboardMovement())
        {
            return;
        }

        ApplyMouseScrollWheelZoom();

        //Left click drag camera
        if (Input.GetMouseButtonDown(0))
        {
            _dragOrigin = Input.mousePosition;
            return;
        }

        if (!Input.GetMouseButton(0)) return;

        Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - _dragOrigin);
        Vector3 move = new Vector3((pos.x * _dragSpeed) * -1, (pos.y * _dragSpeed) * -1, 0);

        transform.Translate(move, Space.World);
    }

    private bool ApplyKeyboardMovement()
    {
        float horizontal = Input.GetAxis("Horizontal") * _moveSpeed;
        float vertical = Input.GetAxis("Vertical") * _moveSpeed;

        if(horizontal == 0 && vertical == 0)
        {
            return false;
        }

        var newPos = new Vector3(
            transform.position.x + Time.deltaTime * horizontal,
            transform.position.y + Time.deltaTime * vertical,
            transform.position.z);
        transform.position = newPos;
        return true;
    }

    private void ApplyMouseScrollWheelZoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            _camera.orthographicSize += _zoomSpeed;
            if (_camera.orthographicSize > 30f)
            {
                _camera.orthographicSize = 30f;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            _camera.orthographicSize -= _zoomSpeed;
            if(_camera.orthographicSize < 0.5)
            {
                _camera.orthographicSize = 0.5f;
            }
        }

        Debug.Log($"_camera.orthographicSize = {_camera.orthographicSize}");
    }
}
