using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This class lets you grab, carry and throw objects with a GrabCarryAndThrowObject component.
	/// 
	/// Animation parameters :
	/// - Grabbing, boolean, triggered when an object is grabbed
	/// - Carrying : boolean, true if an object is being carried, false otherwise
	/// - CarryingID : int, set to whatever value is set on the carried object 
	/// - Throwing, boolean, triggered when an object gets thrown
	/// </summary>
	[AddComponentMenu("Corgi Engine/Character/Abilities/Character Grab, Carry and Throw")]
	public class CharacterGrabCarryAndThrow : CharacterAbility
	{
		public override string HelpBoxText() { return "This class lets you grab, carry and throw objects with a GrabCarryAndThrowObject component." +
		                                              " In the Grab section you can define how you want the raycast that detects grabbable objects to work, " +
		                                              "in the Carry section you can set an optional child transform to attach carried objects to, and in the Throw section you can define how strong you want " +
		                                              "this Character's throw to be, and how much recoil it should get."; }

		[Header("Grab")]

		/// the direction the raycast used to detect grabbable objects will be cast in (if the Character is facing right). Use Vector3.down for Mario2-like grabs from the top, or Vector3.right 
		/// for side grabs for example.
		[Tooltip("the direction the raycast used to detect grabbable objects will be cast in (if the Character is facing right). Use Vector3.down for Mario2-like grabs from the top, or Vector3.right for side grabs for example")]
		public Vector3 RaycastDirection = Vector3.down;
		/// the distance the grab raycast should cover (you'll want it bigger than half your Character's dimensions
		[Tooltip("the distance the grab raycast should cover (you'll want it bigger than half your Character's dimensions")]
		public float RaycastDistance = 1f;
		/// the layer this grab raycast should look for objects on. This should match the layer you put your GrabCarryAndThrowObjects on
		[Tooltip("the layer this grab raycast should look for objects on. This should match the layer you put your GrabCarryAndThrowObjects on")]
		public LayerMask DetectionLayerMask = LayerManager.PlatformsLayerMask | LayerManager.EnemiesLayerMask;
		/// whether or not this Character is grabbing something right now
		[MMReadOnly]
		[Tooltip("whether or not this Character is grabbing something right now")]
		public bool Grabbing = false;

		[Header("Carry")]

		/// a Transform used to attach carried objects to
		[Tooltip("a Transform used to attach carried objects to")]
		public Transform CarryParent;
		/// whether or not this Character is carrying an object this frame
		[MMReadOnly]
		[Tooltip("whether or not this Character is carrying an object this frame")]
		public bool Carrying = false;
		/// the ID of the object being carried
		[MMReadOnly]
		[Tooltip("the ID of the object being carried")]
		public int CarryingID = -1;
		/// a reference to the object being carried
		[MMReadOnly]
		[Tooltip("a reference to the object being carried")]
		public GrabCarryAndThrowObject CarriedObject = null;

		[Header("Throw")]

		/// the force to apply when throwing
		[Tooltip("the force to apply when throwing")]
		public float ThrowForce = 1f;
		/// a modifier to apply to the recoil set on the object
		[Tooltip("a modifier to apply to the recoil set on the object")]
		public float RecoilModifier = 1f;
		/// whether or not this Character is throwing something this frame
		[MMReadOnly]
		[Tooltip("whether or not this Character is throwing something this frame")]
		public bool Throwing = false;
		/// whether or not to allow the character to throw if next to a grabbable object
		[Tooltip("whether or not to allow the character to throw if next to a grabbable object")]
		public bool PreventThrowIfCarryingOnGrab = false; 

		protected Vector2 _raycastOrigin;
		protected Vector2 _recoilVector;

		[Header("ThrowAbility Settings")]
        [Tooltip("轨迹渲染器")]
        public LineRenderer TrajectoryLine;
        [Tooltip("轨迹点数")]
        public int TrajectoryPoints = 20;
        [Tooltip("模拟步长")]
        public float SimulationStep = 0.1f;
        [Tooltip("障碍物层级")]
        public LayerMask ObstacleLayers = LayerManager.ObstaclesLayerMask;
        
        // 新增私有字段
        protected bool _isAiming;
        protected Vector2 _throwDirection;
        protected Camera _mainCamera;
        protected GameObject _throwPreview;
		// animation parameters
		protected const string _grabbingAnimationParameterName = "Grabbing";
		protected int _grabbingAnimationParameter;
		protected const string _carryingAnimationParameterName = "Carrying";
		protected int _carryingAnimationParameter;
		protected const string _carryingIDAnimationParameterName = "CarryingID";
		protected int _carryingIDAnimationParameter;
		protected const string _throwingAnimationParameterName = "Throwing";
		protected int _throwingAnimationParameter;
		protected Vector3 _actualRaycastDirection;
		
		/// <summary>
		/// On init we set our CarryParent to the character transform if null
		/// </summary>
		protected override void Initialization()
		{
			base.Initialization();
			if (CarryParent == null)
			{
				CarryParent = this.transform;
			}
			_mainCamera = Camera.main;
            InitializeTrajectory();
		}
        protected virtual void InitializeTrajectory()
        {
            if (TrajectoryLine != null)
            {
                TrajectoryLine.positionCount = TrajectoryPoints;
                TrajectoryLine.enabled = false;
				TrajectoryLine.useWorldSpace = true;
            }
        }
		/// <summary>
		/// Looks for throw and grab inputs
		/// </summary>
        protected override void HandleInput()
        {
            // 保持原有抓取逻辑
            if (_inputManager.GrabButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
            {
                if (!Carrying) GrabAttempt();
            }

            // 新增鼠标控制逻辑
            if (Carrying)
            {
                HandleMouseInput();
            }
            // else
            // {
            //     // 保持原有按键投掷逻辑（可选保留）
            //     if (_inputManager.ThrowButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
            //     {
            //         if (PreventThrowIfCarryingOnGrab && (GetGrababbleObject() != null)) return;
            //         Throw();
            //     }
            // }
        }
		// 新增鼠标输入处理方法
		public override void ProcessAbility()
        {
            HandleMouseInput();
            UpdateTrajectory();
        }
        protected virtual void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartAiming();
            }

            if (_isAiming)
            {
                UpdateAimDirection();
                UpdateTrajectory();
                
                if (Input.GetMouseButtonUp(0))
                {
                    ExecuteMouseThrow();
                }
            }
        }

		/// <summary>
		/// Tries to grab by casting a raycast
		/// </summary>
		protected virtual void GrabAttempt()
		{
			if (!AbilityAuthorized
			    || ((_condition.CurrentState != CharacterStates.CharacterConditions.Normal) && (_condition.CurrentState != CharacterStates.CharacterConditions.ControlledMovement)))
			{
				return;
			}
            
			CarriedObject = GetGrababbleObject();    
			if (CarriedObject != null)
			{
				Grab();
			}
		}

		protected virtual GrabCarryAndThrowObject GetGrababbleObject()
		{
			_raycastOrigin = this.transform.position;
			_actualRaycastDirection = RaycastDirection;
			if (!_character.IsFacingRight)
			{
				_actualRaycastDirection = _actualRaycastDirection.MMSetX(-RaycastDirection.x);
			}
			RaycastHit2D hit = MMDebug.RayCast(_raycastOrigin, _actualRaycastDirection, RaycastDistance, DetectionLayerMask, Color.blue, _controller.Parameters.DrawRaycastsGizmos);
			if (hit)
			{
				// we make sure we have an object that can be carried
				return hit.collider.gameObject.MMGetComponentNoAlloc<GrabCarryAndThrowObject>();                
			}

			return null;
		}

		/// <summary>
		/// Sets the ability in carrying mode
		/// </summary>
		protected virtual void Grab()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
            
			Carrying = true;
			CarryingID = CarriedObject.CarryingAnimationID;
			CarriedObject.Grab(CarryParent);
			Grabbing = true;
			PlayAbilityStartFeedbacks();
			MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Grab, MMCharacterEvent.Moments.Start);
		}

		/// <summary>
		/// Throws the carried object
		/// </summary>
		/// 
        // 新增瞄准开始方法
        protected virtual void StartAiming()
        {
            _isAiming = true;
            if (TrajectoryLine != null) TrajectoryLine.enabled = true;
            CreateThrowPreview();
        }

        // 新增投掷预览创建
        protected virtual void CreateThrowPreview()
        {
            _throwPreview = new GameObject("ThrowPreview");
            _throwPreview.transform.position = CarryParent.position;
            var sr = _throwPreview.AddComponent<SpriteRenderer>();
            // sr.sprite = CarriedObject.GetComponent<SpriteRenderer>().sprite;
            sr.color = new Color(1, 1, 1, 0.5f);
        }

        // 新增方向计算
		protected virtual void UpdateAimDirection()
		{
			Vector3 mousePos = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -_mainCamera.transform.position.z));
			// mousePos.z = 0;
			Vector2 direction = (mousePos - CarryParent.transform.position).normalized;
            Vector2 direction2 = (mousePos - _character.transform.position).normalized;
			
			// 自动翻转角色（保持原有翻转逻辑）
         	if ((!_character.IsFacingRight && direction2.x > 0) || 
                (_character.IsFacingRight && direction2.x < 0))
            {
                _character.Flip();
            }
			
			// 计算标准化方向（保留原有方向处理）
			Vector2 rawDirection = (mousePos - CarryParent.position).normalized;
			_throwDirection = new Vector2(
				_character.IsFacingRight ? Mathf.Abs(rawDirection.x) : -Mathf.Abs(rawDirection.x),
				rawDirection.y
			);
			// _throwDirection = direction;
		}
        // 新增轨迹更新
		protected virtual void UpdateTrajectory()
		{
            if (!_isAiming || CarryParent == null) return;

            Vector2 startPos = CarryParent.transform.position;
            Vector2 startVelocity = _throwDirection * ThrowForce;
            
            Vector2 previousPoint = startPos;
            int actualPoints = TrajectoryPoints; // 每次循环前重置为完整点数

            // 强制重置LineRenderer的positionCount
            TrajectoryLine.positionCount = TrajectoryPoints; // 新增此行

            for (int i = 0; i < TrajectoryPoints; i++)
            {
                if (i >= TrajectoryLine.positionCount) break;

                // 计算理论轨迹点
                float time = i * SimulationStep;
                Vector2 point = startPos + 
                    startVelocity * time + 
                    0.5f * Physics2D.gravity * time * time;

                // 碰撞检测
                RaycastHit2D hit = Physics2D.Raycast(
                    previousPoint, 
                    (point - previousPoint).normalized, 
                    Vector2.Distance(previousPoint, point), 
                    ObstacleLayers
                );

                if (hit.collider != null)
                {
                    // 调整最终点为碰撞点
                    float distance = Vector2.Distance(previousPoint, hit.point);
                    point = previousPoint + (point - previousPoint).normalized * distance;
                    actualPoints = i + 1;
                    TrajectoryLine.positionCount = actualPoints;
                    TrajectoryLine.SetPosition(i, point);
                    break;
                }

                TrajectoryLine.SetPosition(i, point);
                previousPoint = point;
            }

            // 隐藏未使用的轨迹点
            if (actualPoints < TrajectoryPoints)
            {
                TrajectoryLine.positionCount = actualPoints;
            }
		}

        // 新增鼠标投掷执行
        protected virtual void ExecuteMouseThrow()
        {
            _isAiming = false;
            if (TrajectoryLine != null) TrajectoryLine.enabled = false;
            
            // 调用原有投掷逻辑
            Throw(_throwDirection);
            
            // 清理预览
            if (_throwPreview != null) Destroy(_throwPreview);
        }
		// protected virtual void Throw()
		// {
		// 	if (!AbilityAuthorized)
		// 	{
		// 		return;
		// 	}
            
		// 	if (CarriedObject == null)
		// 	{
		// 		return;
		// 	}

		// 	int direction = _character.IsFacingRight ? 1 : -1;
		// 	CarriedObject.Throw(direction, ThrowForce);

		// 	// apply recoil
		// 	if (RecoilModifier != 0f)
		// 	{
		// 		_recoilVector = (direction == 1) ? Vector2.left : Vector2.right;
		// 		_recoilVector *= RecoilModifier * CarriedObject.Recoil;
		// 		_controller.AddForce(_recoilVector);
		// 	}

		// 	StopFeedbacks();
		// 	CarriedObject = null;
		// 	CarryingID = -1;
		// 	Carrying = false;
		// 	Throwing = true;
		// }
		protected virtual void Throw(Vector2 inputDirection)
		{
			if (!AbilityAuthorized || CarriedObject == null) return;
			
			// 调用原有投掷方法
			CarriedObject.Throw(inputDirection, ThrowForce);

			// 添加垂直速度分量（保持原有物理系统兼容）
			if (CarriedObject.TryGetComponent<Rigidbody2D>(out var rb))
			{
				float verticalForce = Mathf.Clamp(inputDirection.y, -1f, 1f) * ThrowForce;
				rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalForce);
			}

			// 保持原有状态重置
			StopFeedbacks();
			CarriedObject = null;
			CarryingID = -1;
			Carrying = false;
			Throwing = true;
		}
		/// <summary>
		/// Stops all feedbacks
		/// </summary>
		protected virtual void StopFeedbacks()
		{
			if (_startFeedbackIsPlaying)
			{
				StopStartFeedbacks();
				PlayAbilityStopFeedbacks();
				MMCharacterEvent.Trigger(_character, MMCharacterEventTypes.Grab, MMCharacterEvent.Moments.End);
			}
		}
        
		/// <summary>
		/// On late update we reset our states
		/// </summary>
		protected virtual void LateUpdate()
		{
			Grabbing = false;
			Throwing = false;
			if (!Carrying && _isAiming)
            {
                _isAiming = false;
                if (TrajectoryLine != null) TrajectoryLine.enabled = false;
                if (_throwPreview != null) Destroy(_throwPreview);
            }
		}

		/// <summary>
		/// Adds required animator parameters to the animator parameters list if they exist
		/// </summary>
		protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_grabbingAnimationParameterName, AnimatorControllerParameterType.Bool, out _grabbingAnimationParameter);
			RegisterAnimatorParameter(_carryingAnimationParameterName, AnimatorControllerParameterType.Bool, out _carryingAnimationParameter);
			RegisterAnimatorParameter(_carryingIDAnimationParameterName, AnimatorControllerParameterType.Int, out _carryingIDAnimationParameter);
			RegisterAnimatorParameter(_throwingAnimationParameterName, AnimatorControllerParameterType.Bool, out _throwingAnimationParameter);
		}

		/// <summary>
		/// At the end of each cycle, we update our animator parameters with our current state
		/// </summary>
		public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _grabbingAnimationParameter, Grabbing, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _throwingAnimationParameter, Throwing, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _carryingAnimationParameter, Carrying, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _carryingIDAnimationParameter, CarryingID, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
		}

		/// <summary>
		/// On reset ability, we cancel all the changes made
		/// </summary>
		public override void ResetAbility()
		{
			base.ResetAbility();
			// Throw();
			Throw(_throwDirection);
			_isAiming = false;
            if (TrajectoryLine != null) TrajectoryLine.enabled = false;
            if (_throwPreview != null) Destroy(_throwPreview);
			if (_animator != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _grabbingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _throwingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _carryingAnimationParameter, false, _character._animatorParameters, _character.PerformAnimatorSanityChecks);
			}
		}
	}
}