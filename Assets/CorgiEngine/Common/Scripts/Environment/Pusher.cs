using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// If you plan on having moving platforms push your playable character, you'll want to use a Pusher in addition to it
	/// Typically you'll want to parent a pusher to the platform, and bind it to the Pusher's inspector.
	/// You can see an example of it in use in the MinimalMovingPlatforms demo scene
	/// </summary>
	[RequireComponent(typeof(BoxCollider2D))]
	public class Pusher : MonoBehaviour
	{
		/// the moving platform to use as a speed reference
		[Tooltip("the moving platform to use as a speed reference")]
		public MMPathMovement BoundPathMovement;
		/// how far from the collider's bounds the character needs to move to 'detach' from its influence
		[Tooltip("how far from the collider's bounds the character needs to move to 'detach' from its influence")]
		public float DistanceThreshold = 1f;
		
		protected BoxCollider2D _boxCollider2D;
		protected Bounds _bounds;
		protected List<CorgiController> _controllers;
		protected CorgiController _wipController;

		/// <summary>
		/// On Awake we initialize our bounds
		/// </summary>
		protected virtual void Awake()
		{
			_boxCollider2D = this.gameObject.GetComponent<BoxCollider2D>();
			_bounds = _boxCollider2D.bounds;
			_controllers = new List<CorgiController>();
		}

		/// <summary>
		/// On Update we detach our controllers if needed
		/// </summary>
		protected virtual void Update()
		{
			if (_controllers.Count <= 0)
			{
				return;
			}
			
			for (int i = _controllers.Count - 1 ; i >= 0; i--)
			{
				if (_controllers[i] != null)
				{
					float distanceToBoundsCenter = Vector2.Distance(_bounds.center, _controllers[i].BoundsCenter);
			
					if (distanceToBoundsCenter > DistanceThreshold + _bounds.size.x/2)
					{
						DetachController(_controllers[i]);
						continue;
					}

					if ((_controllers[i].ColliderTopPosition.y < _bounds.min.y) ||
					    (_controllers[i].ColliderBottomPosition.y > _bounds.max.y))
					{
						DetachController(_controllers[i]);
						continue;
					}

					if (BoundPathMovement.CurrentSpeed.x > 0 &&
					    _controllers[i].ColliderRightPosition.x < this.transform.position.x)
					{
						DetachController(_controllers[i]);
						continue;
					}
					
					if (BoundPathMovement.CurrentSpeed.x < 0 &&
					    _controllers[i].ColliderLeftPosition.x > this.transform.position.x)
					{
						DetachController(_controllers[i]);
						continue;
					}
					
				}
			}
		}

		/// <summary>
		/// Detaches a specific controller from this pusher
		/// </summary>
		/// <param name="controller"></param>
		protected virtual void DetachController(CorgiController controller)
		{
			controller.SetPusherPlatform(null);
			_controllers.Remove(controller);
		}

		/// <summary>
		/// On trigger enter, we add a colliding controller to our management list if needed
		/// </summary>
		/// <param name="other"></param>
		protected void OnTriggerEnter2D(Collider2D other)
		{
			_wipController = other.GetComponent<CorgiController>();
			if (_wipController == null)
			{
				return;
			}
			_wipController.SetPusherPlatform(BoundPathMovement);
			_controllers.Add(_wipController);
		}
	}
}
