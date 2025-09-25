using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
	public PlayerGroundedState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
	{
		IsRootState = true;
		InitializeSubState();
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
		Ctx.CurrentMovementY = Ctx.GroundedGravity * Time.deltaTime;
		Ctx.AppliedMovementY = Ctx.GroundedGravity * Time.deltaTime;
	}

	public override void ExitState()
	{
		
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
		//Debug.Log("grounded state: " + Ctx.CharacterController.isGrounded);
	}
}
