using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
	public PlayerRunState(PlayerStateMachine currentContext, PlayerStateFactory factory) : base(currentContext, factory)
	{ }

	public override void CheckSwitchStates()
	{
		if (!Ctx.IsMovementPressed)
		{
			SwitchState(Factory.Idle());
		}
		else if (Ctx.IsMovementPressed && !Ctx.isRunPressed)
		{
			SwitchState(Factory.Walk());
		}
	}

	public override void EnterState()
	{
		Ctx.Animator.SetBool(Ctx.IsWalkingHash, true);
		Ctx.Animator.SetBool(Ctx.IsRunningHash, true);
	}

	public override void ExitState()
	{
	
	}

	public override void InitializeSubState()
	{
	
	}

	public override void UpdateState()
	{
		CheckSwitchStates();
		Ctx.AppliedMovementX = Ctx.CurrentMovementInput.x * Ctx.RunMultiplier;
		Ctx.AppliedMovementZ = Ctx.CurrentMovementInput.y * Ctx.RunMultiplier;
	}
}
