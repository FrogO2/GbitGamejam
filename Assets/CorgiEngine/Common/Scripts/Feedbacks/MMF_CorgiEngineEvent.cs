using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This feedback lets you trigger Corgi Engine Events, that can then be caught by other classes
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackPath("Events/Corgi Engine Events")]
	[FeedbackHelp("This feedback lets you trigger Corgi Engine Events, that can then be caught by other classes")]
	public class MMF_CorgiEngineEvent : MMF_Feedback
	{
		/// a static bool used to disable all feedbacks of this type at once
		public static bool FeedbackTypeAuthorized = true;
		/// sets the inspector color for this feedback
		#if UNITY_EDITOR
			public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.EventsColor; } }
		#endif
		
		[MMFInspectorGroup("Corgi Engine Events", true, 17)]

		/// the type of event to trigger
		[Tooltip("the type of event to trigger")]
		public CorgiEngineEventTypes EventType = CorgiEngineEventTypes.PauseNoMenu;
		/// an optional Character to pass to the event
		[Tooltip("an optional Character to pass to the event")]
		public Character TargetCharacter;

		/// <summary>
		/// On play, we ask for a floating text to be spawned
		/// </summary>
		/// <param name="position"></param>
		/// <param name="attenuation"></param>
		protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
		{
			if (Active)
			{
				CorgiEngineEvent.Trigger(EventType, TargetCharacter);
			}
		}
	}
}