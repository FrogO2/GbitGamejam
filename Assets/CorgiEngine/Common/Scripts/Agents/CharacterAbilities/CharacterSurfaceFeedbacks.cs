using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MoreMountains.CorgiEngine
{
	
	/// <summary>
	/// Add this component to a character and it'll let you define a number of surfaces and associate walk and run feedbacks to them
	/// It will also let you trigger events when entering or exiting these surfaces
	/// Important : Surfaces are evaluated from top to bottom. The first surface definition that matches the current detected
	/// ground will be considered the current surface. So make sure your order them accordingly.
	/// </summary>
	[MMHiddenProperties("AbilityStopFeedbacks", "AbilityStartFeedbacks")]
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Surface Feedbacks")] 
	public class CharacterSurfaceFeedbacks : CharacterAbility
	{	
		[Serializable]
		public class CharacterSurfaceFeedbacksItems
		{
			/// an ID to identify this surface in the list. Not used by anything but makes the list more readable
			[Tooltip("an ID to identify this surface in the list. Not used by anything but makes the list more readable")]
			public string ID;
			/// the list of layers that identify this surface
			[Tooltip("the list of layers that identify this surface")]
			public LayerMask Layers;
			/// whether or not to use a tag to identify this surface or just rely only on the layer
			[Tooltip("whether or not to use a tag to identify this surface or just rely only on the layer")]
			public bool UseTag;
			/// if using tags, the Tag that should be on this surface to identify it (on top of the layer)
			[Tooltip("if using tags, the Tag that should be on this surface to identify it (on top of the layer)")]
			[MMCondition("UseTag", true)]
			public string Tag;
			/// the feedback to bind to the Movement ability's AbilityStartFeedbacks slot
			[Tooltip("the feedback to bind to the Movement ability's AbilityStartFeedbacks slot")]
			public MMFeedbacks WalkStartFeedback;
			/// the feedback to bind to the Movement ability's AbilityStopFeedbacks slot
			[Tooltip("the feedback to bind to the Movement ability's AbilityStopFeedbacks slot")]
			public MMFeedbacks WalkStopFeedback;
			/// the feedback to bind to the Run ability's AbilityStartFeedbacks slot
			[Tooltip("the feedback to bind to the Run ability's AbilityStartFeedbacks slot")]
			public MMFeedbacks RunStartFeedback;
			/// the feedback to bind to the Run ability's AbilityStopFeedbacks slot
			[Tooltip("the feedback to bind to the Run ability's AbilityStopFeedbacks slot")]
			public MMFeedbacks RunStopFeedback;
			/// a UnityEvent that will trigger when entering this surface
			[Tooltip("a UnityEvent that will trigger when entering this surface")]
			public UnityEvent OnEnterSurface;
			/// a UnityEvent that will trigger when exiting this surface
			[Tooltip("a UnityEvent that will trigger when exiting this surface")]
			public UnityEvent OnExitSurface;
		}
		
		/// whether detection should rely on periodical controller checks or be driven by an external script (via the SetCurrentSurfaceIndex(int index) method)
		public enum SurfaceDetectionModes { Controller, Script }
		/// This method is only used to display a helpbox text at the beginning of the ability's inspector
		public override string HelpBoxText() { return "This component allows a character and it'll let you define a number of surfaces and associate walk and run feedbacks to them. " +
		                                              "It will also let you trigger events when entering or exiting these surfaces." +
		                                              "Important : Surfaces are evaluated from top to bottom. The first surface definition that matches the current detected ground will " +
		                                              "be considered the current surface. So make sure your order them accordingly."; }

		[Header("List of Surfaces")] 
		/// a list of surface definitions, defined by a layer, an optional tag, and a walk and run sound. These will be evaluated from top to bottom, first match found becomes the current surface.
		[Tooltip("a list of surface definitions, defined by a layer, an optional tag, and a walk and run sound. These will be evaluated from top to bottom, first match found becomes the current surface.")]
		public List<CharacterSurfaceFeedbacksItems> Surfaces;
		
		[Header("Detection")]
		/// whether detection should rely on periodical controller checks or be driven by an external script (via the SetCurrentSurfaceIndex(int index) method)
		[Tooltip("whether detection should rely on periodical controller checks or be driven by an external script (via the SetCurrentSurfaceIndex(int index) method)")]
		public SurfaceDetectionModes SurfaceDetectionMode = SurfaceDetectionModes.Controller;
		/// the frequency (in seconds) at which to cast the raycast to detect surfaces, usually you'll want to space them a bit to save on performance
		[Tooltip("the frequency (in seconds) at which to cast the raycast to detect surfaces, usually you'll want to space them a bit to save on performance")]
		[MMEnumCondition("SurfaceDetectionMode", (int)SurfaceDetectionModes.Controller)]
		public float ControllerCheckFrequency = 0.3f;
		
		[Header("Debug")]
		/// The current index of the surface we're on in the Surfaces list
		[Tooltip("The current index of the surface we're on in the Surfaces list")]
		[MMReadOnly]
		public int CurrentSurfaceIndex = -1;
		
		protected float _timeSinceLastCheck = -float.PositiveInfinity;
		protected int _surfaceIndexLastFrame;
		protected CharacterRun _characterRun;
		
		/// <summary>
		/// A method you can use to force the surface index, when in ScriptDriven mode
		/// </summary>
		/// <param name="index"></param>
		public virtual void SetCurrentSurfaceIndex(int index)
		{
			CurrentSurfaceIndex = index;
		}

		/// <summary>
		/// On init we grab our run ability and init our index
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			_characterRun = _character.FindAbility<CharacterRun>();
			_surfaceIndexLastFrame = -1;
		}
		
		/// <summary>
		/// Every frame we detect surfaces if needed, and handle a potential surface change
		/// </summary>
		public override void ProcessAbility()
		{
			base.ProcessAbility();
			DetectSurface();
			HandleSurfaceChange();
		}

		/// <summary>
		/// If we're on a new surface, we swap feedbacks and invoke our events
		/// </summary>
		protected virtual void HandleSurfaceChange()
		{
			if (_surfaceIndexLastFrame != CurrentSurfaceIndex)
			{
				if (_surfaceIndexLastFrame >= 0 && _surfaceIndexLastFrame < Surfaces.Count)
				{
					Surfaces[_surfaceIndexLastFrame].OnExitSurface?.Invoke();
				}
				Surfaces[CurrentSurfaceIndex].OnEnterSurface?.Invoke();
				_characterHorizontalMovement.StopStartFeedbacks();
				_characterRun.StopStartFeedbacks();
				_characterHorizontalMovement.AbilityStartFeedbacks = Surfaces[CurrentSurfaceIndex].WalkStartFeedback;
				_characterHorizontalMovement.AbilityStopFeedbacks = Surfaces[CurrentSurfaceIndex].WalkStopFeedback;
				_characterRun.AbilityStartFeedbacks = Surfaces[CurrentSurfaceIndex].RunStartFeedback;
				_characterRun.AbilityStopFeedbacks = Surfaces[CurrentSurfaceIndex].RunStopFeedback;
				if (_movement.CurrentState == CharacterStates.MovementStates.Walking)
				{
					_characterHorizontalMovement.PlayAbilityStartFeedbacks();
				}
				if (_movement.CurrentState == CharacterStates.MovementStates.Running)
				{
					_characterRun.PlayAbilityStartFeedbacks();
				}
			}
			_surfaceIndexLastFrame = CurrentSurfaceIndex;
		}

		/// <summary>
		/// Returns true if the tags match or if we're not using tags
		/// </summary>
		/// <param name="useTag"></param>
		/// <param name="contactTag"></param>
		/// <param name="surfaceTag"></param>
		/// <returns></returns>
		protected virtual bool TagsMatch(bool useTag, string contactTag, string surfaceTag)
		{
			if (!useTag)
			{
				return true;
			}
			return contactTag == surfaceTag;
		}

		/// <summary>
		/// Checks if a surface detection is needed and performs it
		/// </summary>
		protected virtual void DetectSurface()
		{
			if (SurfaceDetectionMode == SurfaceDetectionModes.Script)
			{
				return;
			}
			
			if (Time.time - _timeSinceLastCheck < ControllerCheckFrequency)
			{
				return;
			}
			_timeSinceLastCheck = Time.time;

			if (!_controller.State.IsGrounded)
			{
				return;
			}

			if (_controller.StandingOn == null)
			{
				return;
			}
			
			foreach (CharacterSurfaceFeedbacksItems item in Surfaces)
			{
				if (item.Layers.MMContains(_controller.StandingOn.layer) && TagsMatch(item.UseTag, item.Tag, _controller.StandingOn.tag))
				{
					CurrentSurfaceIndex = Surfaces.IndexOf(item);
					return;
				}
			}
		}
	}
}