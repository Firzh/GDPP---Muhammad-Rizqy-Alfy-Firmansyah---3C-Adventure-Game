using System;
using System.Collections;
using UnityEngine;

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

    // ====================== glide
    [SerializeField]
    private float _glideSpeed;
    [SerializeField]
    private float _airDrag;
    [SerializeField]
    private Vector3 _glideRotationSpeed;
    [SerializeField]
    private float _minGlideRotationX;
    [SerializeField]
    private float _maxGlideRotationX;

    // ====================== movements
    [SerializeField]
    private float _rotationSmoothTime = 0.1f;
    private float _rotationSmoothVelocity;
    private bool _isGrounded;
    private PlayerStance _playerStance;
    private Animator _animator;
    private CapsuleCollider _collider;

    // ====================== punch
    [SerializeField]
    private float _resetComboInterval;
    private Coroutine _resetCombo;
    private bool _isPunching;
    private int _combo = 0;

    // ====================== hit & destroy
    [SerializeField]
    private Transform _hitDetector;

    [SerializeField]
    private float _hitDetectorRadius;

    [SerializeField]
    private LayerMask _hitLayer;


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
        _input.OnGlideInput += StartGlide;
        _input.OnCancelGlide += CancelGlide;
        _input.OnPunchInput += Punch;
        _cameraManager.OnChangePerspective += ChangePerspective;
    }

    private void Update()
    {
        CheckIsGrounded();
        CheckStep();
        UpdateFpsAnchorHeight();
        Glide();
    }

    private void UpdateFpsAnchorHeight()
    {
        if (_playerFpsPos == null) return;

        Vector3 p = _playerFpsPos.localPosition;
        // Update Y position based on target cam height
        p.y = Mathf.Lerp(p.y, _targetCamY, _camY_LerpSpeed * Time.deltaTime);

        // Modify Z based on the crouch state
        if (_playerStance == PlayerStance.Crouch)
        {
            p.z = 0.5f; // Crouch Z position
        }
        else
        {
            p.z = 0.3f; // Non-crouch Z position
        }

        _playerFpsPos.localPosition = p;
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
        _input.OnGlideInput -= StartGlide;
        _input.OnCancelGlide -= CancelGlide;
        _input.OnPunchInput -= Punch;
        _cameraManager.OnChangePerspective -= ChangePerspective;
    }

    private void Move(Vector2 axisDirection)
    {
        Vector3 movementDirection = Vector3.zero;
        bool isPlayerStanding = _playerStance == PlayerStance.Stand;
        bool isPlayerClimbing = _playerStance == PlayerStance.Climb;
        bool isPlayerCrouch = _playerStance == PlayerStance.Crouch;
        bool isPlayerGliding = _playerStance == PlayerStance.Glide;

        if ((isPlayerStanding || isPlayerCrouch) != _isPunching)
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
            _rigidbody.AddForce(movementDirection * Time.deltaTime * _climbSpeed);
            Vector3 velocity = new Vector3(_rigidbody.linearVelocity.x, _rigidbody.linearVelocity.y, 0);
            _animator.SetFloat("ClimbVelocityY", velocity.magnitude * axisDirection.y);
            _animator.SetFloat("ClimbVelocityX", velocity.magnitude * axisDirection.x);
        }
        else if (isPlayerGliding)
        {
            // Get the current rotation in Euler angles
            Vector3 rotationDegree = transform.rotation.eulerAngles;

            // Apply the changes based on axisDirection and rotation speed
            rotationDegree.x += _glideRotationSpeed.x * axisDirection.y * Time.deltaTime;

            // Clamp the x rotation within a specific range
            rotationDegree.x = Mathf.Clamp(rotationDegree.x, _minGlideRotationX, _maxGlideRotationX);

            // Apply the rotation for z and y axes
            rotationDegree.z += _glideRotationSpeed.z * axisDirection.x * Time.deltaTime;
            rotationDegree.y += _glideRotationSpeed.y * axisDirection.x * Time.deltaTime;

            // Apply the new rotation to the transform
            transform.rotation = Quaternion.Euler(rotationDegree);
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

        if (_isGrounded)
        {
            CancelGlide();
        }
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
            _collider.center = Vector3.up * 1.4f;
            Vector3 offset = (transform.forward * _climbOffset.z) + (Vector3.up * _climbOffset.y);
            transform.position = hit.point - offset;
            _playerStance = PlayerStance.Climb;
            _rigidbody.useGravity = false;
            _speed = _climbSpeed;
            _cameraManager.SetFPSClampedCamera(true, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(70);
            _animator.SetBool("IsClimbing", true);
        }
    }

    private void CancelClimb()
    {
        if (_playerStance == PlayerStance.Climb)
        {
            _collider.center = Vector3.up * 0.9f;
            _playerStance = PlayerStance.Stand;
            _rigidbody.useGravity = true;
            transform.position -= transform.forward;
            _speed = _walkSpeed;
            _cameraManager.SetFPSClampedCamera(false, transform.rotation.eulerAngles);
            _cameraManager.SetTPSFieldOfView(40);
            _animator.SetBool("IsClimbing", false);
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

    private void StartGlide()
    {
        if (_playerStance != PlayerStance.Glide && !_isGrounded)
        {
            _playerStance = PlayerStance.Glide;
            _animator.SetBool("IsGliding", true);
        }
    }

    private void Glide()
    {
        if (_playerStance == PlayerStance.Glide)
        {
            Vector3 playerRotation = transform.rotation.eulerAngles;
            float lift = playerRotation.x;
            Vector3 upForce = transform.up * (lift + _airDrag);
            Vector3 forwardForce = transform.forward * _glideSpeed;
            Vector3 totalForce = upForce + forwardForce;
            _rigidbody.AddForce(totalForce * Time.deltaTime);
        }
    }

    private void CancelGlide()
    {
        if (_playerStance == PlayerStance.Glide)
        {
            _playerStance = PlayerStance.Stand;
            _animator.SetBool("IsGliding", false);
        }
    }

    private void Punch()
    {
        if (!_isPunching && _playerStance == PlayerStance.Stand)
        {
            _isPunching = true;
            if (_isPunching == true)
            {
                Debug.Log("is punching");
            }

            if (_combo < 3)
            {
                _combo = _combo + 1;
            }
            else
            {
                _combo = 1;
            }
            _animator.SetInteger("Combo", _combo);
            _animator.SetTrigger("Punch");
        }
    }

    private void EndPunch()
    {
        _isPunching = false;
        if (_resetCombo != null)
        {
            StopCoroutine(_resetCombo);
        }
        _resetCombo = StartCoroutine(ResetCombo());
    }

    private IEnumerator ResetCombo()
    {
        yield return new WaitForSeconds(_resetComboInterval);
        _combo = 0;
    }

    private void Hit()
    {
        Collider[] hitObjects = Physics.OverlapSphere(_hitDetector.position, _hitDetectorRadius, _hitLayer);
        for (int i = 0; i < hitObjects.Length; i++)
        {
            if (hitObjects[i].gameObject != null)
            {
                Destroy(hitObjects[i].gameObject);
            }
        }
    }
}
