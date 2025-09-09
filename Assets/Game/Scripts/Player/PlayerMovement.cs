using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _walkSpeed;
    [SerializeField]
    private InputManager _input;
    private float _rotationSmoothTime = 0.1f;
    private float _rotationSmoothVelocity;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _input.OnMoveInput += Move;
    }

    private void OnDestroy()
    {
        _input.OnMoveInput -= Move;
    }

    private void Move(Vector2 axisDirection)
    {
        
        if (axisDirection.magnitude >= 0.1)
        {
            float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0f, rotationAngle, 0f);
            Vector3 movemenDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
            // Debug.Log(movemenDirection);
            _rigidbody.AddForce(movemenDirection * _walkSpeed * Time.deltaTime); 
        }
    }
}
