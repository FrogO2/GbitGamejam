using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this component to a Character and it'll be able to walljump
	/// Animator parameters : WallJumping (bool)
	/// </summary>
	[MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Walljump")] 
	public class CharacterWalljump : CharacterAbility 
	{
		/// the possible ways to apply force when jumping
		public enum ForceModes { AddForce, SetForce }

		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "This component allows your character to perform an extra jump while wall clinging only. Here you can determine the force to apply to that jump."; }

		[Header("Walljump")]
		/// the force of a walljump
		[Tooltip("the force of a walljump")]
		public Vector2 WallJumpForce = new Vector2(10,4);
		/// returns true if a walljump happened this frame
		public bool WallJumpHappenedThisFrame { get; set; }
		/// the selected force mode 
		[Tooltip("the selected force mode ")]
		public ForceModes ForceMode = ForceModes.AddForce;
		/// if this is true, the character will be forced to flip towards the jump direction on the jump frame 
		[Tooltip("if this is true, the character will be forced to flip towards the jump direction on the jump frame")]
		public bool ForceFlipTowardsDirection = false;
		
		[Header("Limit")] 
		/// if this is true, walljumps count as regular (non wall) jump to decrease the number of jumps left
		[Tooltip("if this is true, walljumps count as regular (non wall) jump to decrease the number of jumps left")]
		public bool ShouldReduceNumberOfJumpsLeft = true;
		/// if this is true, number of consecutive walljumps will be limited to MaximumNumberOfWalljumps 
		[Tooltip("if this is true, number of consecutive walljumps will be limited to MaximumNumberOfWalljumps")]
		public bool LimitNumberOfWalljumps = false;
		/// the maximum number of walljumps allowed
		[Tooltip("the maximum number of walljumps allowed")]
		[MMCondition("LimitNumberOfWalljumps", true)]
		public int MaximumNumberOfWalljumps = 3;
		/// the amount of walljumps left at this time 
		[Tooltip("the amount of walljumps left at this time")]
		[MMCondition("LimitNumberOfWalljumps", true)]
		[MMReadOnly]
		public int NumberOfWalljumpsLeft;

		[Header("Coyote Time")] 
		/// whether or not to autorize wall jumps in a buffer duration after the character has exited the wall clinging state 
		[Tooltip("whether or not to autorize wall jumps in a buffer duration after the character has exited the wall clinging state")]
		public bool AllowCoyoteTime = false; 
		/// the duration (in seconds) during which a wall jump should still be allowed after having left the wall clinging state
		[Tooltip("the duration (in seconds) during which a wall jump should still be allowed after having left the wall clinging state")]
		[MMCondition("AllowCoyoteTime", true)]
		public float CoyoteTimeDuration = 0.2f;

		/// a delegate you can listen to to do something when a walljump happens
		public delegate void OnWallJumpDelegate();
		public OnWallJumpDelegate OnWallJump;

		protected CharacterJump _characterJump;
		protected CharacterWallClinging _characterWallClinging;
		// animation parameters
		protected const string _wallJumpingAnimationParameterName = "WallJumping";
		protected int _wallJumpingAnimationParameter;
		protected Vector2 _wallJumpVector;
		protected float _lastTimeWallClinging = -float.MaxValue;
		protected bool _hasWallJumped = false;
		protected bool _hasLeftGround = false;
		
		/// <summary>
		/// On start, we store our characterJump component
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_characterJump = _character?.FindAbility<CharacterJump>();
			_characterWallClinging = _character?.FindAbility<CharacterWallClinging>();
			ResetNumberOfWalljumpsLeft();
			_hasLeftGround = _controller.State.IsGrounded;
		}

		/// <summary>
		/// Resets the amount of walljumps left
		/// </summary>
		public virtual void ResetNumberOfWalljumpsLeft()
		{
			NumberOfWalljumpsLeft = MaximumNumberOfWalljumps;
		}

		/// <summary>
		/// Every frame, we chack if we're pressing the jump button
		/// </summary>
		protected override void HandleInput()
		{
			WallJumpHappenedThisFrame = false;

			if (_inputManager.JumpButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				WalljumpRequest();
			}
			
			
		}

		/// <summary>
		/// Performs a walljump if the conditions are met
		/// </summary>
		protected virtual void WalljumpRequest()
		{
			if (!AbilityAuthorized
			    || _condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				return;
			}
            
			// wall jump
			float wallJumpDirection;

			if (!EvaluateWallJumpConditions())
			{
				return;
			}

			_movement.ChangeState(CharacterStates.MovementStates.WallJumping);
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.WallJump);

			// we decrease the number of jumps left
			if ((_characterJump != null) && ShouldReduceNumberOfJumpsLeft)
			{
				_characterJump.SetNumberOfJumpsLeft(_characterJump.NumberOfJumpsLeft-1);
			}
			_characterJump.SetJumpFlags();

			_condition.ChangeState(CharacterStates.CharacterConditions.Normal);
			_controller.GravityActive(true);
			_controller.SlowFall (0f);	

			// If the character is colliding to the right with something (probably the wall)
			wallJumpDirection = _characterWallClinging.IsFacingRightWhileWallClinging ? -1f : 1f;
			_characterHorizontalMovement?.SetAirControlDirection(wallJumpDirection);
			_wallJumpVector.x = wallJumpDirection * WallJumpForce.x;
			_wallJumpVector.y = Mathf.Sqrt( 2f * WallJumpForce.y * Mathf.Abs(_controller.Parameters.Gravity));
			
			if (ForceMode == ForceModes.AddForce)
			{
				_controller.AddForce(_wallJumpVector);
			}
			else
			{
				_controller.SetForce(_wallJumpVector);
			}

			if (ForceFlipTowardsDirection)
			{
				if (_wallJumpVector.x > 0)
				{
					_character.Face(Character.FacingDirections.Right);    
				}
				else
				{
					_character.Face(Character.FacingDirections.Left);
				}
			}

			if (LimitNumberOfWalljumps)
			{
				NumberOfWalljumpsLeft--;
			}
			
			PlayAbilityStartFeedbacks();
			_hasWallJumped = true;
			WallJumpHappenedThisFrame = true;

			OnWallJump?.Invoke();
		}

		public virtual bool EvaluateWallJumpConditions()
		{
			if (LimitNumberOfWalljumps && NumberOfWalljumpsLeft <= 0)
			{
				return false;
			}

			if (_hasWallJumped)
			{
				return false;
			}

			if (_controller.State.IsGrounded)
			{
				return false;
			}
			
			if (!InWalljumpCoyoteTime())
			{
				return false;
			}

			return true;
		}

		public virtual bool InWalljumpCoyoteTime()
		{
			if (_characterWallClinging == null)
			{
				return false;
			}
			if (_characterWallClinging.HasTouchedGround)
			{
				return false;
			}
			
			if (_movement.CurrentState != CharacterStates.MovementStates.WallClinging)
			{
				if (AllowCoyoteTime)
				{
					if (Time.time - _lastTimeWallClinging > CoyoteTimeDuration)
					{
						return false;
					}
				}
				else
				{
					return false;	
				}
			}

			return true;
		}

		/// <summary>
		/// On ProcessAbility, we reset our number of wall jumps if needed
		/// </summary>
		public override void ProcessAbility()
		{
			if (_controller.State.IsGrounded)
			{
				ResetNumberOfWalljumpsLeft();
			}
		}

		protected void LateUpdate()
		{
			if (_character.MovementState.CurrentState == CharacterStates.MovementStates.WallClinging)
			{
				_hasWallJumped = false;
				_lastTimeWallClinging = Time.time;
			}

			if (_controller.State.IsGrounded)
			{
				_hasWallJumped = false;
			}
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_wallJumpingAnimationParameterName, AnimatorControllerParameterType.Bool, out _wallJumpingAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we send our character's animator the current walljumping status
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _wallJumpingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.WallJumping), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}

		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();
			if (_animator != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _wallJumpingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);	
			}
		}
	}
}