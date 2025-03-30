using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This action will cause your AI character to stop running
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Run Stop")]
	public class AIActionRunStop : AIAction
	{
		protected CharacterRun _characterRun;

		/// <summary>
		/// On init we grab our CharacterRun component
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			_characterRun = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterRun>();
		}

		/// <summary>
		/// On PerformAction we stop running
		/// </summary>
		public override void PerformAction()
		{
			_characterRun.RunStop();
		}
	}
}