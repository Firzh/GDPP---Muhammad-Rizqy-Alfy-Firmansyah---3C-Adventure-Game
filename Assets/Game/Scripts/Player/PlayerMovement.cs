using System;
using UnityEditor.ShaderGraph.Internal;
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
    private float _crouchSpeed;
    [SerializeField]
    private float _walkSprintTransition;
    [SerializeField]
    private InputManager _input;

    // ====================== jump
    [SerializeField]
    private float _jumpForce;
    [SerializeField]
    private Transform _groundDetector;
    [SerializeField]
    private float _detectorRadius;
    [SerializeField]
    private LayerMask _groundLayer;

    // ====================== stairs
    [SerializeField]
    private Vector3 _upperStepOffset;
    [SerializeField]
    private float _stepCheckerDistance;
    [SerializeField]
    private float _stepForce;

    // ====================== climb
    [SerializeField]
    private Vector3 _climbOffset;
    [SerializeField]
    private Transform _climbDetector;
    [SerializeField]
    private float _climbCheckDistance;
    [SerializeField]
    private LayerMask _climbableLayer;
    [SerializeField]
    private float _climbSpeed;

    // ====================== camera
    [SerializeField]
    private Transform _cameraTransformTPS;
    [SerializeField]
    private Transform _cameraTransformFPS;
    [SerializeField]
    private CameraManager _cameraManager;
    // public Action OnChangePerspective;

    // ====================== crouch
    [SerializeField]
    private Transform _playerFpsPos; // drag: PlayerFPSPos
    [SerializeField]
    private float _standCamY = 1.7f;
    [SerializeField]
    private float _crouchCamY = 1.2f;
    [SerializeField] 
    private float _camY_LerpSpeed = 12f;
    private float _targetCamY;

    // ====================== movements
    [SerializeField]
    private float _rotationSmoothTime = 0.1f;
    private float _rotationSmoothVelocity;
    private bool _isGrounded;
    private PlayerStance _playerStance;
    private Animator _animator;
    private CapsuleCollider _collider;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<CapsuleCollider>();
        _speed = _walkSpeed;
        _playerStance = PlayerStance.Stand;

        _targetCamY = _standCamY;
        if (_playerFpsPos != null)
        {
            var p = _playerFpsPos.localPosition;
            p.y = _targetCamY;
            _playerFpsPos.localPosition = p;
        }
        HideAndLockCursor();
    }

    private void Start()
    {
        _input.OnMoveInput += Move;
        _input.OnSprintInput += Sprint;
        _input.OnJumpInput += Jump;
        _input.OnClimbInput += StartClimb;
        _input.OnCancelClimb += CancelClimb;
        _input.OnCrouchInput += Crouch;
        _cameraManager.OnChangePerspective += ChangePerspective;
    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
        UpdateFpsAnchorHeight();
    }

    private void UpdateFpsAnchorHeight()
    {
        if (_playerFpsPos == null) return;
        Vector3 p = _playerFpsPos.localPosition;
        p.y = Mathf.Lerp(p.y, _targetCamY, _camY_LerpSpeed * Time.deltaTime);
        _playerFpsPos.localPosition = p;

        // opsional: jika kamera bukan child anchor, sinkronkan pos kamera ke anchor
        // _cameraTransformFPS.localPosition = _playerFpsPos.localPosition;
    }

    private void HideAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void DisplayAndFreeCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        _input.OnMoveInput -= Move;
        _input.OnSprintInput -= Sprint;
        _input.OnJumpInput -= Jump;
        _input.OnClimbInput -= StartClimb;
        _input.OnCancelClimb -= CancelClimb;
        _input.OnCrouchInput -= Crouch;
        _cameraManager.OnChangePerspective -= ChangePerspective;
    }

    private void Move(Vector2 axisDirection)
    {
        Vector3 movementDirection = Vector3.zero;
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;
        bool isPlayerCrouch = _playerStance == PlayerStance.Crouch;

        if (isPlayerStanding || isPlayerCrouch)
        {
            switch (_cameraManager.CameraState)
            {
                case CameraState.ThirdPerson:
                    if (axisDirection.magnitude >= 0.1)
                    {
                        // Debug.Log("This is Third Person Camera");
                        float rotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + _cameraTransformTPS.eulerAngles.y;
                        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, rotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);

                        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
                        movementDirection = Quaternion.Euler(0f, rotationAngle, 0f) * Vector3.forward;
                        _rigidbody.AddForce(movementDirection * Time.deltaTime * _speed);
                    }
                    break;
                case CameraState.FirstPerson:
                    // Debug.Log("This is First Person Camera");
                    float FPSRotationAngle = Mathf.Atan2(axisDirection.x, axisDirection.y) * Mathf.Rad2Deg + _cameraTransformFPS.eulerAngles.y;
                    float FPSSmoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, FPSRotationAngle, ref _rotationSmoothVelocity, _rotationSmoothTime);
                    // Bug saat backwards karena rotasi kamera FPS mengikuti rotasi karakter. 
                    // Teruji bisa backwards normal jika kamera menghadap belakang (kepala karakter) yang mana ga mungkin di FPS wkwk

                    // Saat mundur, jangan update rotasi (lock rotasi ke yang terakhir)
                    if (axisDirection.y >= 0)
                    {
                        transform.rotation = Quaternion.Euler(0f, FPSSmoothAngle, 0f);
                    }

                    // Hitung arah gerakan relatif karakter
                    Vector3 verticalDirection = axisDirection.y * transform.forward;
                    Vector3 horizontalDirection = axisDirection.x * transform.right;
                    movementDirection = verticalDirection + horizontalDirection;

                    _rigidbody.AddForce(movementDirection * Time.deltaTime * _speed);
                    break;
                default:
                    break;
            }
            Vector3 velocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            _animator.SetFloat("Velocity", velocity.magnitude * axisDirection.magnitude);

            _animator.SetFloat("VelocityZ", velocity.magnitude * axisDirection.y);
            _animator.SetFloat("VelocityX", velocity.magnitude * axisDirection.x);
        }
        else if (isPlayerClimbing)
        {
            Vector3 horizontal = axisDirection.x * transform.right;
            Vector3 vertical = axisDirection.y * transform.up;
            movementDirection = horizontal + vertical;
            _rigidbody.AddForce(movementDirection * Time.deltaTime * _climbSpeed);
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
            if (_speed > _walkSpeed)
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
            _animator.SetTrigger("Jump");
        }
    }

    private void CheckIsGrounded()
    {
        _isGrounded = Physics.CheckSphere(_groundDetector.position, _detectorRadius, _groundLayer);
        _animator.SetBool("IsGrounded", _isGrounded);
        // Debug.Log("Player hit the ground");
    }
    private void CheckStep()
    {
        bool isHitLowerStep = Physics.Raycast(_groundDetector.position, transform.forward, _stepCheckerDistance);
        bool isHitUpperStep = Physics.Raycast(_groundDetector.position + _upperStepOffset, transform.forward, _stepCheckerDistance);
        if (isHitLowerStep && !isHitUpperStep)
        {
            _rigidbody.AddForce(0, _stepForce * Time.deltaTime, 0);
            // Debug.Log("Upper force applied");
        }
    }

    private void StartClimb()
    {
        bool isInFrontOfClimbingWall = Physics.Raycast(_climbDetector.position, transform.forward,
                                                        out RaycastHit hit, _climbCheckDistance, _climbableLayer);
        if (isInFrontOfClimbingWall)
        {
            // Debug.Log("In front of a Wall");
        }

        bool isNotClimbing = _playerStance != PlayerStance.Climb;
        if (isNotClimbing)
        {
            // Debug.Log("Player is not climbing");
        }

        if (isInFrontOfClimbingWall && _isGrounded && isNotClimbing)
        {
            Vector3 offset = (transform.forward * _climbOffset.z) + (Vector3.up * _climbOffset.y);
            transform.position = hit.point - offset;
            _playerStance = PlayerStance.Climb;
            _rigidbody.useGravity = false;
            _speed = _climbSpeed;
            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(70);
        }
    }

    private void CancelClimb()
    {
        if (_playerStance == PlayerStance.Climb)
        {
            _playerStance = PlayerStance.Stand;
            _rigidbody.useGravity = true;
            transform.position -= transform.forward;
            _speed = _walkSpeed;
            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(40);
        }
    }

    private void ChangePerspective()
    {
        // OnChangePerspective();
        _animator.SetTrigger("ChangePerspective");
    }

    private void Crouch()
    {
        if (_playerStance == PlayerStance.Stand)
        {
            _playerStance = PlayerStance.Crouch;
            _animator.SetBool("IsCrouch", true);
            _speed = _crouchSpeed;
            _collider.height = 1.3f;
            _collider.center = Vector3.up * 0.66f;

            _targetCamY = _crouchCamY;
        }
        else if (_playerStance == PlayerStance.Crouch)
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("IsCrouch", false);
            _speed = _walkSpeed;
            _collider.height = 1.8f;
            _collider.center = Vector3.up * 0.9f;

            _targetCamY = _standCamY;
        }
    }
}
