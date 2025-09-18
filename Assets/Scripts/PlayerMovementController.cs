using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    private PlayerControls platerControls;
	private CharacterController characterController;
	private Animator animator;
	[SerializeField]
	private float rotationFactorPerFrame = 15.0f;
	[SerializeField]
	private float runMultiplier = 3.0f;
	private float gravity = -9.8f;
	private float groundedGravity = -.05f;
	private float initialJumpVelocity;
	private float maxJumpHeight = 4f;
	private float maxJumpTime = .75f;
	private float fallMultiplier = 2f;

	private Vector2 currentMovementInput;
	private Vector3 currentMovement;
	private Vector3 currentRunMovement;
	private bool isMovementPressed;
	private bool isRunPressed;
	private bool isJumpPressed = false;
	private bool isJumping = false;
	private bool isJumpAnimating = false;

	private int isWalkingHash;
	private int isRunningHash;
	private int isJumpingHash;
	private int jumpCount = 0;

	private Dictionary<int, float> initialJumpVelocities = new Dictionary<int, float>();
	private Dictionary<int, float> jumpGravities = new Dictionary<int, float>();

	private void OnEnable()
	{
		platerControls.Player.Enable();
	}
	
	private void OnDisable()
	{
		platerControls.Player.Disable();
	}

	void Awake()
    {
		platerControls = new PlayerControls();
		characterController = GetComponent<CharacterController>();
		animator = GetComponent<Animator>();

		isWalkingHash = Animator.StringToHash("isWalking");
		isRunningHash = Animator.StringToHash("isRunning");
		isJumpingHash = Animator.StringToHash("isJumping");

		platerControls.Player.Move.started += OnMovementInput;
		platerControls.Player.Move.canceled += OnMovementInput;
		platerControls.Player.Move.performed += OnMovementInput;
		platerControls.Player.Run.started += OnRun;
		platerControls.Player.Run.canceled += OnRun;
		platerControls.Player.Jump.started += OnJump;
		platerControls.Player.Jump.canceled += OnJump;

		SetupJumpVariables();
	}

	private void SetupJumpVariables()
	{
		float timeToApex = maxJumpTime / 2;
		gravity = (-2 * maxJumpHeight)/Mathf.Pow(timeToApex, 2);
		initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
		float secondJumpGravity = (-2 * (maxJumpHeight + 2)) / Mathf.Pow((timeToApex * 1.25f), 2);
		float secondJumpInitialVelocity = (2 * (maxJumpHeight + 2)) / timeToApex * 1.25f;
		float thirdJumpGravity = (-2 * (maxJumpHeight + 4)) / Mathf.Pow((timeToApex * 1.5f), 2);
		float thirdJumpInitialVelocity = (2 * (maxJumpHeight + 4)) / timeToApex * 1.5f;

		jumpGravities.Add(0, gravity);
		jumpGravities.Add(1, gravity);
		jumpGravities.Add(2, secondJumpGravity);
		jumpGravities.Add(3, thirdJumpGravity);

		initialJumpVelocities.Add(1, initialJumpVelocity);
		initialJumpVelocities.Add(2, secondJumpInitialVelocity);
		initialJumpVelocities.Add(2, thirdJumpInitialVelocity);
	}

	private void OnMovementInput(InputAction.CallbackContext context)
	{
		currentMovementInput = context.ReadValue<Vector2>();
		currentMovement.x = currentMovementInput.x;
		currentMovement.z = currentMovementInput.y;
		currentRunMovement.x = currentMovementInput.x * runMultiplier;
		currentRunMovement.z = currentMovementInput.y * runMultiplier;
		isMovementPressed = currentMovement.x != 0 || currentMovement.z != 0;
	}

	private void OnRun(InputAction.CallbackContext context)
	{
		isRunPressed = context.ReadValueAsButton();
	}

	private void OnJump(InputAction.CallbackContext context)
	{
		isJumpPressed = context.ReadValueAsButton();
	}

	private void HandleAnimation()
	{
		bool isWalking = animator.GetBool(isWalkingHash);
		bool isRunning = animator.GetBool(isRunningHash);

		if (isMovementPressed && !isWalking)
		{
			animator.SetBool(isWalkingHash, true);
		}
		else if (!isMovementPressed && isWalking)
		{
			animator.SetBool(isWalkingHash, false);
		}
		if ((isMovementPressed && isRunPressed) && !isRunning)
		{
			animator.SetBool(isRunningHash, true);
		}
		else if ((!isMovementPressed || !isRunPressed) && isRunning)
		{
			animator.SetBool(isRunningHash, false);
		}
	}

	private void HandleRotation()
	{
		Vector3 positionToLookAt;
		positionToLookAt.x = currentMovement.x;
		positionToLookAt.y = 0.0f;
		positionToLookAt.z = currentMovement.z;

		Quaternion currentRotation = transform.rotation;

		if (isMovementPressed)
		{
			Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
			transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
		}
	}

	private void HandleGravity()
	{
		bool isFalling = currentMovement.y <= 0f || !isJumpPressed;
		if (characterController.isGrounded)
		{
			if (isJumpAnimating)
			{
				animator.SetBool(isJumpingHash, false);
				isJumpAnimating = false;
				StartCoroutine(JumpResetRoutine());
			}
			currentMovement.y = groundedGravity * Time.deltaTime;
			currentRunMovement.y = groundedGravity * Time.deltaTime;
		}
		else if (isFalling)
		{
			float previousVelocity = currentMovement.y;
			float newVelocity = currentMovement.y + (jumpGravities[jumpCount] * fallMultiplier * Time.deltaTime);
			float nextVelocity = Mathf.Max((previousVelocity + newVelocity) * .5f, -20f); 
			currentMovement.y = nextVelocity;
			currentRunMovement.y = nextVelocity;
		}
		else
		{
			float previousVelocity = currentMovement.y;
			float newVelocity = currentMovement.y + (jumpGravities[jumpCount] * Time.deltaTime); //gravity * Time.deltaTime = acceleration
			float nextVelocity = (previousVelocity + newVelocity) * .5f; //average of previous and new velocity so that it doesn't differ between frame rates
			currentMovement.y = nextVelocity;
			currentRunMovement.y = nextVelocity;
		}
	}

	private void HandleJump()
	{
		if (!isJumping && characterController.isGrounded && isJumpPressed)
		{
			animator.SetBool(isJumpingHash, true);
			isJumping = true;
			isJumpAnimating = true;
			jumpCount += 1;
			currentMovement.y = initialJumpVelocities[jumpCount] * .5f;
			currentRunMovement.y = initialJumpVelocities[jumpCount] * .5f;
		}
		else if (isJumping && characterController.isGrounded && !isJumpPressed)
		{
			isJumping = false;
		}
	}

	IEnumerator JumpResetRoutine()
	{
		yield return new WaitForSeconds(.5f);
		jumpCount = 0;
	}

	void Update()
    {
		HandleRotation();
		HandleAnimation();

		if (isRunPressed) 
		{
			characterController.Move(currentRunMovement * Time.deltaTime);
		}
		else
		{
			characterController.Move(currentMovement * Time.deltaTime);
		}

		HandleGravity();
		HandleJump();
	}
}
