using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This Action will make the Character patrol while jumping until it (optionally) hits a wall or a hole.
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Patrol And Jump")]
	public class AIActionPatrolAndJump : AIActionPatrol
	{
		protected CharacterJump _characterJump;

		/// <summary>
		/// On init we grab all the components we'll need
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();
			_characterJump = _character?.FindAbility<CharacterJump>();
		}

		/// <summary>
		/// This method initiates all the required checks and moves the character
		/// </summary>
		protected override void Patrol()
		{
			base.Patrol();
			if (_character == null)
			{
				return;
			}
			if ((_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
			    || (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen))
			{
				return;
			}_characterJump.JumpStart();
		}
	}
}