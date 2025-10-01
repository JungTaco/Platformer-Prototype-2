using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStateMachine : MonoBehaviour
{
	public PlayerBaseState CurrentState{ get { return _currentState; } set { _currentState = value; } }
	public Animator Animator { get { return _animator; } }
	public CharacterController CharacterController { get { return _characterController; } }
	public Coroutine CurrentJumpResetRoutine { get { return _currentJumpResetRoutine; } set { _currentJumpResetRoutine = value; } }
	public Dictionary<int, float> InitialJumpVelocities { get { return _initialJumpVelocities; } }
	public Dictionary<int, float> JumpGravities { get { return _jumpGravities; } }
	public int JumpCount{ get { return _jumpCount; } set { _jumpCount = value; } }
	public int IsJumpingHash { get { return _isJumpingHash; } }
	public int IsWalkingHash { get { return _isWalkingHash; } }
	public int IsRunningHash { get { return _isRunningHash; } }
	public int JumpCountHash { get { return _jumpCountHash; } }
	public int IsFallingHash { get { return _isFallingHash; } }
	public bool RequiresNewJumpPress { get { return _requiresNewJumpPress; } set{ _requiresNewJumpPress = value; } }
	public bool IsJumping { set{ _isJumping = value; } }
	public bool IsJumpPressed { get { return _isJumpPressed; } }
	public bool IsMovementPressed { get { return _isMovementPressed; } }
	public bool isRunPressed {  get { return _isRunPressed; } }
	public float GroundedGravity { get { return _groundedGravity; } }
	public float Gravity { get { return _gravity; } }
	public float FallMultiplier { get { return _fallMultiplier; } }
	public float RunMultiplier { get { return _runMultiplier; } }
	public float CurrentMovementY { get { return _currentMovement.y; } set { _currentMovement.y = value; } }
	public float AppliedMovementY { get { return _appliedMovement.y; } set { _appliedMovement.y = value; } }
	public float AppliedMovementX { get { return _appliedMovement.x; } set { _appliedMovement.x = value; } }
	public float AppliedMovementZ { get { return _appliedMovement.z; } set { _appliedMovement.z = value; } }
	public Vector2 CurrentMovementInput{ get { return _currentMovementInput; } }

	private PlayerControls _platerControls;
	private CharacterController _characterController;
	private Animator _animator;
	[SerializeField]
	private float _rotationFactorPerFrame = 15.0f;
	[SerializeField]
	private float _runMultiplier = 3.0f;
	private float _gravity = -9.8f;
	private float _groundedGravity = -50f;
	private float _initialJumpVelocity;
	private float _maxJumpHeight = 2f;
	private float _maxJumpTime = .75f;
	private float _fallMultiplier = 2f;

	private Vector2 _currentMovementInput;
	private Vector3 _currentMovement;
	private Vector3 _currentRunMovement;
	private Vector3 _appliedMovement;
	private bool _isMovementPressed;
	private bool _isRunPressed;
	private bool _isJumpPressed = false;
	private bool _isJumping = false;
	private bool _requiresNewJumpPress = false;

	private int _isWalkingHash;
	private int _isRunningHash;
	private int _isJumpingHash;
	private int _jumpCountHash;
	private int _isFallingHash;
	private int _jumpCount = 0;

	//private List<float> initialJumpVelocities = new List<float>();
	//private List<float> jumpGravities = new List<float>();
	private Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
	private Dictionary<int, float> _jumpGravities = new Dictionary<int, float>();

	private Coroutine _currentJumpResetRoutine = null;

	private PlayerBaseState _currentState;
	private PlayerStateFactory _stateFactory;

	private void OnEnable()
	{
		_platerControls.Player.Enable();
	}

	private void OnDisable()
	{
		_platerControls.Player.Disable();
	}

	void Awake()
	{
		_platerControls = new PlayerControls();
		_characterController = GetComponent<CharacterController>();
		_animator = GetComponent<Animator>();

		_stateFactory = new PlayerStateFactory(this);
		_currentState = _stateFactory.Grounded();
		_currentState.EnterState();

		_isWalkingHash = Animator.StringToHash("isWalking");
		_isRunningHash = Animator.StringToHash("isRunning");
		_isJumpingHash = Animator.StringToHash("isJumping");
		_jumpCountHash = Animator.StringToHash("jumpCount");
		_isFallingHash = Animator.StringToHash("isFalling");

		_platerControls.Player.Move.started += OnMovementInput;
		_platerControls.Player.Move.canceled += OnMovementInput;
		_platerControls.Player.Move.performed += OnMovementInput;
		_platerControls.Player.Run.started += OnRun;
		_platerControls.Player.Run.canceled += OnRun;
		_platerControls.Player.Jump.started += OnJump;
		_platerControls.Player.Jump.canceled += OnJump;

		SetupJumpVariables();
	}

	private void SetupJumpVariables()
	{
		float timeToApex = _maxJumpTime / 2;
		_gravity = (-2 * _maxJumpHeight) / Mathf.Pow(timeToApex, 2);
		_initialJumpVelocity = (2 * _maxJumpHeight) / timeToApex;
		float secondJumpGravity = (-2 * (_maxJumpHeight * 1.05f)) / Mathf.Pow((timeToApex * 1.05f), 2);
		float secondJumpInitialVelocity = (2 * (_maxJumpHeight * 1.05f)) / timeToApex * 1.05f;
		float thirdJumpGravity = (-2 * (_maxJumpHeight * 1.1f)) / Mathf.Pow((timeToApex * 1.25f), 2);
		float thirdJumpInitialVelocity = (2 * (_maxJumpHeight * 1.1f)) / timeToApex * 1.25f;

		_jumpGravities.Add(0, _gravity);
		_jumpGravities.Add(1, _gravity);
		_jumpGravities.Add(2, secondJumpGravity);
		_jumpGravities.Add(3, thirdJumpGravity);

		_initialJumpVelocities.Add(1, _initialJumpVelocity);
		_initialJumpVelocities.Add(2, secondJumpInitialVelocity);
		_initialJumpVelocities.Add(3, thirdJumpInitialVelocity);
	}

	private void OnMovementInput(InputAction.CallbackContext context)
	{
		_currentMovementInput = context.ReadValue<Vector2>();
		_currentMovement.x = _currentMovementInput.x;
		_currentMovement.z = _currentMovementInput.y;
		_currentRunMovement.x = _currentMovementInput.x * _runMultiplier;
		_currentRunMovement.z = _currentMovementInput.y * _runMultiplier;
		_isMovementPressed = _currentMovement.x != 0 || _currentMovement.z != 0;
	}

	private void OnRun(InputAction.CallbackContext context)
	{
		_isRunPressed = context.ReadValueAsButton();
	}

	private void OnJump(InputAction.CallbackContext context)
	{
		_isJumpPressed = context.ReadValueAsButton();
		_requiresNewJumpPress = false;
	}

	private void HandleRotation()
	{
		Vector3 positionToLookAt;
		positionToLookAt.x = _currentMovement.x;
		positionToLookAt.y = 0.0f;
		positionToLookAt.z = _currentMovement.z;

		Quaternion currentRotation = transform.rotation;

		if (_isMovementPressed)
		{
			Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
			transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, _rotationFactorPerFrame * Time.deltaTime);
		}
	}

	private void Update()
	{
		HandleRotation();	
		_characterController.Move(_appliedMovement * Time.deltaTime);
		_currentState.UpdateStates();
	}
}
