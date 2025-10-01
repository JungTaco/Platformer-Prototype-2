using UnityEngine;

public class PlayerFallState : PlayerBaseState, IRootState
{
	public PlayerFallState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
	{ 
		IsRootState = true;
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
		InitializeSubState();
		Ctx.Animator.SetBool(Ctx.IsFallingHash, true);
		Debug.Log("FALLING");
	}

	public override void ExitState()
	{
		Ctx.Animator.SetBool(Ctx.IsFallingHash, false);
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

	public void HandleGravity()
	{
		float previousYVelocity = Ctx.CurrentMovementY;
		Ctx.CurrentMovementY = Ctx.CurrentMovementY + Ctx.Gravity * Time.deltaTime;
		Ctx.AppliedMovementY = Mathf.Max((previousYVelocity + Ctx.CurrentMovementY)*.5f, -20f);
	}
}
