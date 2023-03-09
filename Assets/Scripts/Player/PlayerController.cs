using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]

public class PlayerController : MonoBehaviour
{
    [Header("Player")]
    public float MoveSpeed = 10.0f;
    public float SprintSpeed = 15.0f;
    public float DodgeSpeed = 20.0f;
    public float RotationSpeed = 5.0f;
    public float SpeedChangeRate = 10.0f;
    public Vector3 inputDirection;
    private bool isDodge;
    private Vector3 dodgeVec;

    [Space(10)]
    public float JumpHeight = 1.2f;
    public float Gravity = -9.0f;

    [Space(10)]
    public float JumpTimeout = 0.1f;
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.5f;
    public LayerMask GroundLayers;

    [Header("Camera")]
    public Camera PlayerCamera;
    public float _CameraVerticalAngle = 0f;
    [Range(0.1f, 1f)]
    public float AimingRotationMultiplier = 0.4f;

    private float _speed;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private PlayerWeaponsManager _WeaponsManager;
    private PlayerInput _playerInput;
    private CharacterController _controller;
    private InputSystem _input;
    private GameObject _mainCamera;
    private Animator _animator;

    public float RotationMultiplier
    {
        get
        {
            if (_WeaponsManager.IsAiming)
            {
                return AimingRotationMultiplier;
            }
                return 1f;
        }
    }

    private bool IsCurrentDeviceMouse
    {
        get
        {
            return _playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<InputSystem>();
        _playerInput = GetComponent<PlayerInput>();
        _animator = GetComponentInChildren<Animator>();
        _WeaponsManager = GetComponent<PlayerWeaponsManager>();
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
        isDodge = false;
    }

    private void Update()
    {
        JumpAndGravity();
        GroundedCheck();
        Move();
        Dodge();
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void CameraRotation()
    {
        transform.Rotate( new Vector3(0f,(_input.look.x * RotationSpeed * RotationMultiplier),0f), Space.Self);
        
        _CameraVerticalAngle += _input.look.y * RotationSpeed * RotationMultiplier;
        _CameraVerticalAngle = Mathf.Clamp(_CameraVerticalAngle, -89f, 89f);
        PlayerCamera.transform.localEulerAngles = new Vector3(_CameraVerticalAngle, 0, 0);
    }

    private void Move()
    {
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        if (_input.move == Vector2.zero) targetSpeed = 0.0f;
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        if (_input.move != Vector2.zero)
        {
            inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
        }

        if (isDodge) inputDirection = dodgeVec;

        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);


        _animator.SetBool("isRun",inputDirection != Vector3.zero);
        _animator.SetBool("isRunFast", _input.sprint);
    }

    private void JumpAndGravity()
    {
        if (Grounded) // 착지상태 
        {
            _fallTimeoutDelta = FallTimeout;

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
                _animator.SetBool("isJump", false);
            }

            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                _animator.SetBool("isJump", true);
                _animator.SetTrigger("doJump");
                Debug.Log("JUMP");
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else // 점프 중
        {
            _jumpTimeoutDelta = JumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }

            _input.jump = false;
        }

        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }
    void Dodge()
    {
        if (_input.dodge && isDodge == false)
        {
            dodgeVec = inputDirection;
            MoveSpeed *= 2;
            _animator.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 1f);
        }
    }

    void DodgeOut()
    {
        MoveSpeed *= 0.5f;
        isDodge = false;
    }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax) // 카메라 회전시 360도 안으로 유지
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected() // 착지를 위한 구체 시각화 
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }
}
