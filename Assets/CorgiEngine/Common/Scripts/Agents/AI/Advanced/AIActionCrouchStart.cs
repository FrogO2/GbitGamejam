using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This action will cause your AI character to start crouching
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Crouch Start")]
	public class AIActionCrouchStart : AIAction
	{
		protected CharacterCrouch _characterCrouch;

		/// <summary>
		/// On init we grab our CharacterRun component
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			_characterCrouch = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterCrouch>();
		}

		/// <summary>
		/// On PerformAction we start running
		/// </summary>
		public override void PerformAction()
		{
			_characterCrouch.Crouch();
		}
	}
}