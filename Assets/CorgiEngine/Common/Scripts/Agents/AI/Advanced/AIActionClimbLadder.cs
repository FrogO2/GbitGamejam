using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This Action will let the AI agent you put it on climb ladders, up, down, or in any direction
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/AI/Actions/AI Action Climb Ladder")]
	public class AIActionClimbLadder : AIAction
	{
		public enum Modes { ClimbUp, ClimbDown, ClimbAny, None}
		
		[Header("Ladder interaction")]
		/// whether the AI should climb up, down, or any direction on ladders it encounters
		[Tooltip("whether the AI should climb up, down, or any direction on ladders it encounters")]
		public Modes Mode = Modes.ClimbAny;
		/// a duration, in seconds, during which the agent can't change input duration after having climbed a ladder
		[Tooltip("a duration, in seconds, during which the agent can't change input duration after having climbed a ladder")]
		public float CooldownBetweenClimbsDuration = 1f;
		
		// private stuff
		protected CorgiController _controller;
		protected Character _character;
		protected Health _health;
		protected CharacterHorizontalMovement _characterHorizontalMovement;
		protected CharacterLadder _characterLadder;
		protected Vector2 _ladderInput;
		protected bool _climbingLastFrame;
		protected Ladder _ladderLastFrame;
		protected float _yInputLastFrame;
		protected float _timeSinceLastClimb;

		protected const float _threshold = 0.5f;

		/// <summary>
		/// On init we grab all the components we'll need
		/// </summary>
		public override void Initialization()
		{
			if(!ShouldInitialize) return;
			// we get the CorgiController2D component
			_controller = GetComponentInParent<CorgiController>();
			_character = GetComponentInParent<Character>();
			_characterHorizontalMovement = _character?.FindAbility<CharacterHorizontalMovement>();
			_characterLadder = _character?.FindAbility<CharacterLadder>();
			_health = _character.CharacterHealth;
		}

		/// <summary>
		/// On PerformAction we try climbing a ladder if possible
		/// </summary>
		public override void PerformAction()
		{
			ClimbLadder();
			StoreLastFrame();
		}

		/// <summary>
		/// This method changes ladder input based on our settings and ladder position
		/// </summary>
		protected virtual void ClimbLadder()
		{
			if ((_character == null) || (_characterLadder == null))
			{
				return;
			}
			if ((_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
			    || (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen))
			{
				return;
			}

			if (!_characterLadder.LadderColliding)
			{
				return;
			}

			_ladderInput.x = 0f;
			_ladderInput.y = 0f;
			switch (Mode)
			{
				case Modes.ClimbUp:
					_ladderInput.y = 1f;
					break;
				case Modes.ClimbDown:
					_ladderInput.y = -1f;
					break;
				case Modes.ClimbAny:
					if (_ladderLastFrame == _characterLadder.CurrentLadder)
					{
						_ladderInput.y = _yInputLastFrame;
					}
					else
					{
						if (_timeSinceLastClimb > CooldownBetweenClimbsDuration)
						{
							_ladderInput.y = _characterLadder.AboveLadderPlatform() ? -1f : 1f;	
						}
					}
					break;
				case Modes.None:
					_ladderInput.y = 0f;
					break;
			}

			_characterLadder.SetInput(_ladderInput, _threshold);
		}

		/// <summary>
		/// We store our ladder and climbing state
		/// </summary>
		protected virtual void StoreLastFrame()
		{
			_ladderLastFrame = _characterLadder.CurrentLadder;
			_climbingLastFrame = _character.MovementState.CurrentState == CharacterStates.MovementStates.LadderClimbing;
			_yInputLastFrame = _ladderInput.y;
			if (_climbingLastFrame)
			{
				_timeSinceLastClimb = 0f;
			}
			else
			{
				_timeSinceLastClimb += Time.deltaTime;
			}
		}

		/// <summary>
		/// When exiting the state we reset our ladder input
		/// </summary>
		public override void OnExitState()
		{
			base.OnExitState();
			ResetLadderInput();
		}

		/// <summary>
		/// When reviving we reset our ladder input
		/// </summary>
		protected virtual void OnRevive()
		{
			ResetLadderInput();
		}

		protected virtual void ResetLadderInput()
		{
			if (_characterLadder == null)
			{
				_characterLadder.SetInput(Vector2.zero, _threshold);	
			}
		}

		/// <summary>
		/// On enable we start listening for OnRevive events
		/// </summary>
		protected virtual void OnEnable()
		{
			if (_health == null)
			{
				_health = this.gameObject.GetComponentInParent<Health>();
			}

			if (_health != null)
			{
				_health.OnRevive += OnRevive;
			}
		}

		/// <summary>
		/// On disable we stop listening for OnRevive events
		/// </summary>
		protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnRevive -= OnRevive;
			}
		}
	}
}