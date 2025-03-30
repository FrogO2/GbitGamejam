using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this component to a Character and it'll be able to cling to walls when being in the air, 
	// facing a wall, and moving in its direction
	/// Animator parameters : WallClinging (bool)
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Wallclinging")] 
	public class CharacterWallClinging : CharacterAbility 
	{
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "Add this component to your character and it'll be able to cling to walls, slowing down its fall. Here you can define the slow factor (close to 0 : super slow, 1 : normal fall) and the tolerance (to account for tiny holes in the wall."; }

		[Header("Wall Clinging")]
		/// the slow factor when wall clinging
		[Tooltip("the slow factor when wall clinging")]
		[Range(0.01f, 1)]
		public float WallClingingSlowFactor = 0.2f;
		/// the vertical offset to apply to raycasts for wall clinging
		[Tooltip("the vertical offset to apply to raycasts for wall clinging")]
		public float RaycastVerticalOffset = 0f;
		/// the tolerance applied to compensate for tiny irregularities in the wall (slightly misplaced tiles for example)
		[Tooltip("the tolerance applied to compensate for tiny irregularities in the wall (slightly misplaced tiles for example)")]
		public float WallClingingTolerance = 0.3f;
		/// if this is true, vertical forces will be reset on entry
		[Tooltip("if this is true, vertical forces will be reset on entry")]
		public bool ResetVerticalForceOnEntry = true;

		[Header("Automation")]
		/// if this is set to true, you won't need to press the opposite direction to wall cling, it'll be automatic anytime the character faces a wall
		[Tooltip("if this is set to true, you won't need to press the opposite direction to wall cling, it'll be automatic anytime the character faces a wall")]
		public bool InputIndependent = false;

		public bool IsFacingRightWhileWallClinging { get; set; }
		public bool HasTouchedGround { get; set; }

		protected CharacterStates.MovementStates _stateLastFrame;
		protected RaycastHit2D _raycast;
		protected WallClingingOverride _wallClingingOverride;
		protected bool _inputManagerNotNull;

		// animation parameters
		protected const string _wallClingingAnimationParameterName = "WallClinging";
		protected int _wallClingingAnimationParameter;

		protected override void Initialization()
		{
			base.Initialization();
			_inputManagerNotNull = _inputManager != null;
		}

		/// <summary>
		/// Every frame, checks if the wallclinging state should be exited
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();

			WallClinging();
			ExitWallClinging();
			WallClingingLastFrame ();
		}

		/// <summary>
		/// At the end of the frame, we store the current state for comparison use in the next frame
		/// </summary>
		public override void LateProcessAbility()
		{
			base.LateProcessAbility();
			_stateLastFrame = _movement.CurrentState;
		}
		
		/// <summary>
		/// Call this method to force the character to detach from the wall it's currently clinging to
		/// </summary>
		public virtual void DetachFromWall()
		{
			if (_movement.CurrentState != CharacterStates.MovementStates.WallClinging)
			{
				return;
			}
			ProcessExit();
		}

		/// <summary>
		/// Makes the player stick to a wall when jumping
		/// </summary>
		protected virtual void WallClinging()
		{
			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			    || (_controller.State.IsGrounded)
			    || (_controller.State.ColliderResized)
			    || (_controller.Speed.y >= 0) )
			{
				return;
			}
            
			if (InputIndependent)
			{
				if (TestForWall())
				{
					EnterWallClinging();
				}
			}
			else
			{
				if (_inputManagerNotNull)
				{
					if (_horizontalInput <= -_inputManager.Threshold.x)
					{
						if (TestForWall(-1))
						{
							EnterWallClinging();
						}
					}
					else if (_horizontalInput >= _inputManager.Threshold.x)
					{
						if (TestForWall(1))
						{
							EnterWallClinging();
						}
					}	
				}
			}            
		}

		/// <summary>
		/// Casts a ray to check if we're facing a wall
		/// </summary>
		/// <returns></returns>
		protected virtual bool TestForWall()
		{
			if (_character.IsFacingRight)
			{
				return TestForWall(1);
			}
			else
			{
				return TestForWall(-1);
			}
		}

		protected virtual bool TestForWall(int direction)
		{
			// we then cast a ray to the direction's the character is facing, in a down diagonal.
			// we could use the controller's IsCollidingLeft/Right for that, but this technique 
			// compensates for walls that have small holes or are not perfectly flat
			Vector3 raycastOrigin = _characterTransform.position;
			Vector3 raycastDirection;
			if (direction > 0)
			{
				raycastOrigin = raycastOrigin + _characterTransform.right * _controller.Width() / 2 + _characterTransform.up * RaycastVerticalOffset;
				raycastDirection = _characterTransform.right - _characterTransform.up;
			}
			else
			{
				raycastOrigin = raycastOrigin - _characterTransform.right * _controller.Width() / 2 + _characterTransform.up * RaycastVerticalOffset;
				raycastDirection = -_characterTransform.right - _characterTransform.up;
			}

			// we cast our ray 
			_raycast = MMDebug.RayCast(raycastOrigin, raycastDirection, WallClingingTolerance, _controller.PlatformMask & ~(_controller.OneWayPlatformMask | _controller.MovingOneWayPlatformMask), Color.black, _controller.Parameters.DrawRaycastsGizmos);

			// we check if the ray hit anything. If it didn't, or if we're not moving in the direction of the wall, we exit
			return _raycast;
		}

		/// <summary>
		/// Enters the wall clinging state
		/// </summary>
		protected virtual void EnterWallClinging()
		{
			// we check for an override
			if (_controller.CurrentWallCollider != null)
			{
				_wallClingingOverride = _controller.CurrentWallCollider.gameObject.MMGetComponentNoAlloc<WallClingingOverride>();
			}
			else if (_raycast.collider != null)
			{
				_wallClingingOverride = _raycast.collider.gameObject.MMGetComponentNoAlloc<WallClingingOverride>();
			}
            
			if (_wallClingingOverride != null)
			{
				// if we can't wallcling to this wall, we do nothing and exit
				if (!_wallClingingOverride.CanWallClingToThis)
				{
					return;
				}
				_controller.SlowFall(_wallClingingOverride.WallClingingSlowFactor);
			}
			else
			{
				// we slow the controller's fall speed
				_controller.SlowFall(WallClingingSlowFactor);
			}

			// if we weren't wallclinging before this frame, we start our sounds
			if ((_movement.CurrentState != CharacterStates.MovementStates.WallClinging) && !_startFeedbackIsPlaying)
			{
				if (ResetVerticalForceOnEntry)
				{
					_controller.SetVerticalForce(0f);	
				}
				// we start our feedbacks
				PlayAbilityStartFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.WallCling, MMCharacterEvent.Moments.Start);
			}

			_movement.ChangeState(CharacterStates.MovementStates.WallClinging);
			IsFacingRightWhileWallClinging = _character.IsFacingRight;
			HasTouchedGround = false;
		}

		/// <summary>
		/// If the character is currently wallclinging, checks if we should exit the state
		/// </summary>
		protected virtual void ExitWallClinging()
		{
			if (_controller.State.IsGrounded)
			{
				HasTouchedGround = true;
			}
			
			if (_movement.CurrentState == CharacterStates.MovementStates.WallClinging)
			{
				// we prepare a boolean to store our exit condition value
				bool shouldExit = false;
				if ((_controller.State.IsGrounded) // if the character is grounded
				    || (_controller.Speed.y > 0))  // or if it's moving up
				{
					// then we should exit
					shouldExit = true;
				}

				// we then cast a ray to the direction's the character is facing, in a down diagonal.
				// we could use the controller's IsCollidingLeft/Right for that, but this technique 
				// compensates for walls that have small holes or are not perfectly flat
				Vector3 raycastOrigin = _characterTransform.position;
				Vector3 raycastDirection;
				if (_character.IsFacingRight) 
				{ 
					raycastOrigin = raycastOrigin + _characterTransform.right * _controller.Width()/ 2 + _characterTransform.up * RaycastVerticalOffset;
					raycastDirection = _characterTransform.right - _characterTransform.up; 
				}
				else
				{
					raycastOrigin = raycastOrigin - _characterTransform.right * _controller.Width()/ 2 + _characterTransform.up * RaycastVerticalOffset;
					raycastDirection = - _characterTransform.right - _characterTransform.up;
				}
                				
				// we check if the ray hit anything. If it didn't, or if we're not moving in the direction of the wall, we exit
				if (!InputIndependent)
				{
					// we cast our ray 
					RaycastHit2D hit = MMDebug.RayCast(raycastOrigin, raycastDirection, WallClingingTolerance, _controller.PlatformMask & ~(_controller.OneWayPlatformMask | _controller.MovingOneWayPlatformMask), Color.black, _controller.Parameters.DrawRaycastsGizmos);
                    
					if (_character.IsFacingRight)
					{
						if ((!hit) || (_horizontalInput <= _inputManager.Threshold.x))
						{
							shouldExit = true;
						}
					}
					else
					{
						if ((!hit) || (_horizontalInput >= -_inputManager.Threshold.x))
						{
							shouldExit = true;
						}
					}
				}
				else
				{
					if (_raycast.collider == null)
					{
						shouldExit = true;
					}
				}
				
				if (shouldExit)
				{
					ProcessExit();
				}
			}

			if ((_stateLastFrame == CharacterStates.MovementStates.WallClinging) 
			    && (_movement.CurrentState != CharacterStates.MovementStates.WallClinging)
			    && _startFeedbackIsPlaying)
			{
				// we play our exit feedbacks
				StopStartFeedbacks();
				PlayAbilityStopFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.WallCling, MMCharacterEvent.Moments.End);
			}
		}

		protected virtual void ProcessExit()
		{
			// if we're not wallclinging anymore, we reset the slowFall factor, and reset our state.
			_controller.SlowFall(0f);
			// we reset the state
			_movement.ChangeState(CharacterStates.MovementStates.Idle);
		}

		/// <summary>
		/// This methods tests if we were wallcling previously, and if so, resets the slowfall factor and stops the wallclinging sound
		/// </summary>
		protected virtual void WallClingingLastFrame()
		{
			if ((_movement.PreviousState == CharacterStates.MovementStates.WallClinging) 
			    && (_movement.CurrentState != CharacterStates.MovementStates.WallClinging)
			    && _startFeedbackIsPlaying)
			{
				_controller.SlowFall (0f);	
				StopStartFeedbacks();
			}
		}
        
		protected override void OnDeath()
		{
			base.OnDeath();
			ProcessExit();
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_wallClingingAnimationParameterName, AnimatorControllerParameterType.Bool, out _wallClingingAnimationParameter);
		}

		/// <summary>
		/// Updates the animator with the current wallclinging state
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _wallClingingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.WallClinging), _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}
		
		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();
			if (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
			{
				ProcessExit();	
			}

			if (_animator != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _wallClingingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);	
			}
		}
	}
}