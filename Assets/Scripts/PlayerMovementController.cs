using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    private PlayerControls _platerControls;
	private CharacterController _characterController;
	private Animator _animator;
	[SerializeField]
	private float _rotationFactorPerFrame = 15.0f;
	[SerializeField]
	private float _runMultiplier = 3.0f;
	private float _gravity = -9.8f;
	private float _groundedGravity = -.05f;
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
	private bool _isJumpAnimating = false;

	private int _isWalkingHash;
	private int _isRunningHash;
	private int _isJumpingHash;
	private int _jumpCountHash;
	private int _jumpCount = 0;

	//private List<float> initialJumpVelocities = new List<float>();
	//private List<float> jumpGravities = new List<float>();
	private Dictionary<int, float> _initialJumpVelocities = new Dictionary<int, float>();
	private Dictionary<int, float> _jumpGravities = new Dictionary<int, float>();

	private Coroutine _currentJumpResetRoutine = null;

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

		_isWalkingHash = Animator.StringToHash("isWalking");
		_isRunningHash = Animator.StringToHash("isRunning");
		_isJumpingHash = Animator.StringToHash("isJumping");
		_jumpCountHash = Animator.StringToHash("jumpCount");

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
		_gravity = (-2 * _maxJumpHeight)/Mathf.Pow(timeToApex, 2);
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
	}

	private void HandleAnimation()
	{
		bool isWalking = _animator.GetBool(_isWalkingHash);
		bool isRunning = _animator.GetBool(_isRunningHash);

		if (_isMovementPressed && !isWalking)
		{
			_animator.SetBool(_isWalkingHash, true);
		}
		else if (!_isMovementPressed && isWalking)
		{
			_animator.SetBool(_isWalkingHash, false);
		}
		if ((_isMovementPressed && _isRunPressed) && !isRunning)
		{
			_animator.SetBool(_isRunningHash, true);
		}
		else if ((!_isMovementPressed || !_isRunPressed) && isRunning)
		{
			_animator.SetBool(_isRunningHash, false);
		}
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

	private void HandleGravity()
	{
		bool isFalling = _currentMovement.y <= 0f || !_isJumpPressed;
		if (_characterController.isGrounded)
		{
			if (_isJumpAnimating)
			{
				_animator.SetBool(_isJumpingHash, false);
				_isJumpAnimating = false;
				_currentJumpResetRoutine = StartCoroutine(JumpResetRoutine());
				if (_jumpCount == 3)
				{
					_jumpCount = 0;
					_animator.SetInteger(_jumpCountHash, _jumpCount); 
				}
			}
			_currentMovement.y = _groundedGravity * Time.deltaTime;
			_appliedMovement.y = _groundedGravity * Time.deltaTime;
		}
		else if (isFalling)
		{
			float previousVelocity = _currentMovement.y;
			_currentMovement.y = _currentMovement.y + (_jumpGravities[_jumpCount] * _fallMultiplier * Time.deltaTime);
			_appliedMovement.y = Mathf.Max((previousVelocity + _currentMovement.y) * .5f, -20f); 
		}
		else
		{
			float previousVelocity = _currentMovement.y;
			_currentMovement.y = _currentMovement.y + (_jumpGravities[_jumpCount] * Time.deltaTime); //gravity * Time.deltaTime = acceleration
			_appliedMovement.y = (previousVelocity + _currentMovement.y) * .5f; //average of previous and new velocity so that it doesn't differ between frame rates
		}
	}

	private void HandleJump()
	{
		if (!_isJumping && _characterController.isGrounded && _isJumpPressed)
		{
			if (_jumpCount < 3 && _currentJumpResetRoutine != null)
			{
				StopCoroutine(JumpResetRoutine());
				_currentJumpResetRoutine = null;
			}
			_animator.SetBool(_isJumpingHash, true);
			_isJumping = true;
			_isJumpAnimating = true;
			_jumpCount += 1;
			_animator.SetInteger(_jumpCountHash, _jumpCount);
			_currentMovement.y = _initialJumpVelocities[_jumpCount];
			_appliedMovement.y = _initialJumpVelocities[_jumpCount];			
		}
		else if (_isJumping && _characterController.isGrounded && !_isJumpPressed)
		{
			_isJumping = false;
		}
	}

	IEnumerator JumpResetRoutine()
	{
		yield return new WaitForSeconds(1.5f);
		_jumpCount = 0;
		_animator.SetInteger(_jumpCountHash, _jumpCount);
	}

	void Update()
    {
		HandleRotation();
		HandleAnimation();

		if (_isRunPressed) 
		{
			_appliedMovement.x = _currentRunMovement.x;
			_appliedMovement.z = _currentRunMovement.z;
		}
		else
		{
			_appliedMovement.x = _currentMovement.x;
			_appliedMovement.z = _currentMovement.z;
		}

		_characterController.Move(_appliedMovement * Time.deltaTime);

		HandleGravity();
		HandleJump();
	}
}
