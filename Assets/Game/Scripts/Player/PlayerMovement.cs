using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    private float _speed;
    [SerializeField]
    private float _walkSpeed;
    [SerializeField]
    private float _sprintSpeed;
    [SerializeField]
    private float _walkSprintTransition;
    [SerializeField]
    private InputManager _input;
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private Transform _groundDetector;
    [SerializeField]
    private float _detectorRadius;
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private float _rotationSmoothTime = 0.1f;
    private float _rotationSmoothVelocity;
    private bool _isGrounded;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _speed = _walkSpeed;
    }

    private void Start()
    {
        _input.OnMoveInput += Move;
        _input.OnSprintInput += Sprint;
        _input.OnJumpInput += Jump;
    }

    private void Update()
    {
        CheckIsGrounded();
    }

    private void OnDestroy()
    {
        _input.OnMoveInput -= Move;
        _input.OnSprintInput -= Sprint;
        _input.OnJumpInput -= Jump;
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
            _rigidbody.AddForce(movemenDirection * _speed * Time.deltaTime);
        }
    }

    private void Sprint(bool isSprint)
    {
        if (isSprint)
        {
            if (_speed < _sprintSpeed)
            {
                _speed = _speed + _walkSprintTransition * Time.deltaTime;
            }
            // Debug.Log("Sprinting");
        }
        else
        {
            if (_speed < _sprintSpeed)
            {
                _speed = _speed - _walkSprintTransition * Time.deltaTime;
            }
            // Debug.Log("Walking");
        }
    }

    private void Jump()
    {
        if (_isGrounded)
        {
            Vector3 jumppDirection = Vector3.up;
            _rigidbody.AddForce(jumppDirection * _jumpForce * Time.deltaTime);
        }
    }

    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);
        // Debug.Log("Player hit the ground");
    }
}
