using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This class is only meant to be used in the MinimalStairs demo scene
	/// In it, a blue RectangleAI character will move (semi) randomly on the stairs there
	/// As you can see below, it's a very basic AI, that will randomly decide to go up or down every once in a while
	/// </summary>
	public class AIStairsRandomizer : MonoBehaviour
	{
		/// the frequency, in seconds, at which this AI will change its direction
		[MMVector("Min", "Max")] 
		public Vector2 DecisionChangeDelay = new Vector2(3f, 6f);

		[Header("Debug")] 
		/// whether or not this AI is currently trying to go up stairs
		[MMReadOnly] 
		public bool GoingUp;
		/// whether or not this AI is currently trying to go down stairs
		[MMReadOnly] 
		public bool GoingDown;
		/// the next time this AI will change its direction
		[MMReadOnly] 
		public float NextDecisionAt;
		
		protected CharacterStairs _characterStairs;

		/// <summary>
		/// On Start we grab our CharacterStairs component and randomize our direction
		/// </summary>
		protected virtual void Start()
		{
			_characterStairs = this.gameObject.GetComponent<CharacterStairs>();
			RandomizeDirection();
		}

		/// <summary>
		/// On Update we pick a new direction if needed
		/// </summary>
		protected virtual void Update()
		{
			if (Time.time > NextDecisionAt)
			{
				RandomizeDirection();
			}
		}

		/// <summary>
		/// Randomizes the direction this AI is going to take next time it meets stairs
		/// </summary>
		protected virtual void RandomizeDirection()
		{
			if (MMMaths.RollADice(6) > 3)
			{
				GoingUp = true;
				GoingDown = false;
			}
			else
			{
				GoingUp = false;
				GoingDown = true;
			}
			_characterStairs.SetInput(GoingUp, GoingDown);
			NextDecisionAt = Time.time + Random.Range(DecisionChangeDelay.x, DecisionChangeDelay.y);
		}
	}
}
