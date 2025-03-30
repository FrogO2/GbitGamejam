using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this component to an object with a 2D collider and it'll be grippable by any Character equipped with a CharacterGrip
	/// </summary>
	[RequireComponent(typeof(Collider2D))]
	[AddComponentMenu("Corgi Engine/Environment/Grip")]
	public class Grip : CorgiMonoBehaviour 
	{
		[MMInformation("Add this component to an object with a Collider2D on it, and any character with a CharacterGrip ability will be able to hang on to it. You can also specify an offset for the character when gripping.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

		/// the offset to apply to the gripped character's position when gripping
		[Tooltip("the offset to apply to the gripped character's position when gripping")]
		public Vector3 GripOffset = Vector3.zero;

		[Header("Interpolation")]
		/// if this is true, the position of the gripping character will be interpolated towards the grip's position when gripping starts
		[Tooltip("if this is true, the position of the gripping character will be interpolated towards the grip's position when gripping starts")]
		public bool TweenToGripPosition = false;
		/// the tween definition to use when interpolating the character's position
		[Tooltip("the tween definition to use when interpolating the character's position")]
		public MMTweenType TweenType = new MMTweenType(MMTween.MMTweenCurve.EaseInQuartic);
		/// the duration (in seconds) over which to tween the character into gripping position when grip starts
		[Tooltip("the duration (in seconds) over which to tween the character into gripping position when grip starts")]
		public float TweenDuration = 0.5f;

		protected MMPathMovement _mmPathMovement;

		/// <summary>
		/// On Start we grab our MMPathMovement component
		/// </summary>
		protected virtual void Start()
		{
			_mmPathMovement = this.gameObject.GetComponent<MMPathMovement>();
		}

		/// <summary>
		/// When an object collides with the grip, we check to see if it's a compatible character, and if yes, we change its state to Gripping
		/// </summary>
		/// <param name="collider">Collider.</param>
		protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			CharacterGrip characterGrip = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterGrip>();
			if (characterGrip == null)	{	return;	}

			characterGrip.StartGripping (this);

			if (_mmPathMovement != null)
			{
				_mmPathMovement.CanMove = true;
			}
		}
	}
}