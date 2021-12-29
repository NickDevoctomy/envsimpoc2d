using UnityEngine;

public class CameraController : MonoBehaviour
{
    private float _moveSpeed = 10f;
    private Vector3? _lastPosition = null;

    private void Start()
    {
        if(Map.Instance != null)
        {
            transform.position = new Vector3(Map.Instance.Width / 2, Map.Instance.Height / 2, transform.position.z);
        }
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal") * _moveSpeed;
        float vertical = Input.GetAxis("Vertical") * _moveSpeed;

        var newPos = new Vector3(
            transform.position.x + Time.deltaTime * horizontal,
            transform.position.y + Time.deltaTime * vertical,
            transform.position.z);
        transform.position = newPos;

        if(_lastPosition != transform.position)
        {
            _lastPosition = transform.position;
        }
    }
}
