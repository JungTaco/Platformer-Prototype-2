using System.Collections;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
	public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
	{
		IsRootState = true;
		InitializeSubState();
	}

	public override void CheckSwitchStates()
	{
		if (Ctx.CharacterController.isGrounded)
		{
			SwitchState(Factory.Grounded());
		}
	}

	public override void EnterState()
	{
		HandleJump();
	}

	public override void ExitState()
	{
		Ctx.Animator.SetBool(Ctx.IsJumpingHash, false);
		if (Ctx.IsJumpPressed)
		{
			Ctx.RequiresNewJumpPress = true;
		}
		Ctx.CurrentJumpResetRoutine= Ctx.StartCoroutine(IJumpResetRoutine());
		if (Ctx.JumpCount >= 3)
		{
			Ctx .JumpCount= 0;
			Ctx.Animator.SetInteger(Ctx.JumpCountHash, Ctx.JumpCount);
		}
	}

	public override void InitializeSubState()
	{
		if (!Ctx.IsMovementPressed && !Ctx.isRunPressed)
		{
			SetSubState(Factory.Idle());
		}
		else if (Ctx.IsMovementPressed && !Ctx.isRunPressed)
		{
			SetSubState(Factory.Walk());
		}
		else
		{
			SetSubState(Factory.Run());
		}
	}

	public override void UpdateState()
	{
		HandleGravity();
		CheckSwitchStates();
	}

	void HandleJump()
	{
		if (Ctx.JumpCount < 3 && Ctx.CurrentJumpResetRoutine != null)
		{
			Ctx.StopCoroutine(Ctx.CurrentJumpResetRoutine);
			Ctx.CurrentJumpResetRoutine = null;
		}
		Ctx.Animator.SetBool(Ctx.IsJumpingHash, true);
		Ctx.IsJumping = true;
		Ctx.JumpCount += 1;
		Ctx.Animator.SetInteger(Ctx.JumpCountHash, Ctx.JumpCount);
		Ctx.CurrentMovementY = Ctx.InitialJumpVelocities[Ctx.JumpCount];
		Ctx.AppliedMovementY = Ctx.InitialJumpVelocities[Ctx.JumpCount];
	}

	private void HandleGravity()
	{
		bool isFalling = Ctx.CurrentMovementY <= 0f || !Ctx.IsJumpPressed;

		if (isFalling)
		{
			float previousVelocity = Ctx.CurrentMovementY;
			Ctx.CurrentMovementY = Ctx.CurrentMovementY + (Ctx.JumpGravities[Ctx.JumpCount] * Ctx.FallMultiplier * Time.deltaTime);
			Ctx.AppliedMovementY = Mathf.Max((previousVelocity + Ctx.CurrentMovementY) * .5f, -20f);
		}
		else
		{
			float previousVelocity = Ctx.CurrentMovementY;
			Ctx.CurrentMovementY = Ctx.CurrentMovementY + (Ctx.JumpGravities[Ctx.JumpCount] * Time.deltaTime); //gravity * Time.deltaTime = acceleration
			Ctx.AppliedMovementY = (previousVelocity + Ctx.CurrentMovementY) * .5f; //average of previous and new velocity so that it doesn't differ between frame rates
		}
	}

	IEnumerator IJumpResetRoutine()
	{
		yield return new WaitForSeconds(1.5f);
		Ctx.JumpCount = 0;
	}
}
