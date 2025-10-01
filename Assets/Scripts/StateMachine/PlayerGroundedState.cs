using UnityEngine;

public class PlayerGroundedState : PlayerBaseState, IRootState
{
	public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
	{
		IsRootState = true;	
	}

	public override void CheckSwitchStates()
	{
		if (Ctx.IsJumpPressed && !Ctx.RequiresNewJumpPress)
		{
			SwitchState(Factory.Jump());
		}
		else if (!Ctx.CharacterController.isGrounded) 
		{
			SwitchState(Factory.Fall());
		}
	}

	public override void EnterState()
	{
		InitializeSubState();
		HandleGravity();
	}

	public override void ExitState()
	{
		
	}

	public void HandleGravity()
	{
		Ctx.CurrentMovementY = Ctx.GroundedGravity * Time.deltaTime;
		Ctx.AppliedMovementY = Ctx.GroundedGravity * Time.deltaTime;
	}

	public override void InitializeSubState()
	{
		if(!Ctx.IsMovementPressed && !Ctx.isRunPressed)
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
		CheckSwitchStates();
	}
}
