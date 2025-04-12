// CharacterThrowAbility.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using MoreMountains.CorgiEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    [AddComponentMenu("Corgi Engine/Character/Abilities/Character Throw")] 
    public class CharacterThrowAbility : CharacterAbility
    {
        [Header("ThrowAbility Settings")]
        public GameObject ThrowablePrefab;
        public float ThrowForce = 30f; 
        public LineRenderer TrajectoryLine;
        public int TrajectoryPoints = 20; 
        public float SimulationStep = 0.1f; 

        private Camera _mainCamera;
        private bool _isAiming;
        private Vector2 _throwDirection;
        private GameObject _currentProjectile;

        public LayerMask ObstacleLayers; // 在Inspector中指定需要检测的层级（如背景物体）
        public float CollisionRadius = 0.1f; // 检测精度

        protected override void Initialization()
        {
            base.Initialization();
            _mainCamera = Camera.main;
            TrajectoryLine.positionCount = TrajectoryPoints;
            TrajectoryLine.enabled = false;
            TrajectoryLine.useWorldSpace = true; // 确保使用世界坐标
        }

        public override void ProcessAbility()
        {
            HandleAiming();
            UpdateTrajectory();
        }

        private void HandleAiming()
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartAiming();
            }

            if (_isAiming)
            {
                CalculateDirection();
                
                if (Input.GetMouseButtonUp(0))
                {
                    ExecuteThrow();
                }
            }
        }

        private void StartAiming()
        {
            _isAiming = true;
            TrajectoryLine.enabled = true;
            TrajectoryLine.positionCount = TrajectoryPoints;

            // 生成虚拟投掷物（无物理组件）
            _currentProjectile = new GameObject("ThrowableDummy");
            _currentProjectile.transform.position = GetSpawnPosition();

            // 添加可视化标记（可选）
            var sprite = _currentProjectile.AddComponent<SpriteRenderer>();
            sprite.sprite = ThrowablePrefab.GetComponent<SpriteRenderer>().sprite;
            sprite.color = new Color(1, 1, 1, 0.5f); // 半透明显示
                }

        private void CalculateDirection()
        {
            Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -_mainCamera.transform.position.z));
            Vector2 direction = (mouseWorldPos - _currentProjectile.transform.position).normalized;
            Vector2 direction2 = (mouseWorldPos - _character.transform.position).normalized;
            
            if ((!_character.IsFacingRight && direction2.x > 0) || 
                (_character.IsFacingRight && direction2.x < 0))
            {
                _character.Flip();
            }
            
            _throwDirection = direction;
        }

        private Vector3 GetSpawnPosition()
        {
            // 根据角色朝向计算偏移
            float xOffset = _character.IsFacingRight ? 0.5f : -0.5f;
            // Debug.Log(_character.transform.position);
            return _character.transform.position + new Vector3(xOffset, 0.2f, 0);
        }

        // private void UpdateTrajectory()
        // {
        //     if (!_isAiming || _currentProjectile == null) return;

        //     // 实时更新虚拟投掷物位置
        //     _currentProjectile.transform.position = GetSpawnPosition();

        //     // 轨迹计算（无需物理组件）
        //     Vector2 startPos = _currentProjectile.transform.position;
        //     Vector2 startVelocity = _throwDirection * ThrowForce;

        //     for (int i = 0; i < TrajectoryPoints; i++)
        //     {
        //         float time = i * SimulationStep;
        //         Vector2 point = startPos + 
        //             startVelocity * time + 
        //             0.5f * Physics2D.gravity * time * time;
        //         TrajectoryLine.SetPosition(i, point);
        //     }
        //     // Debug.Log($"轨迹起点: {TrajectoryLine.GetPosition(0)}");
        //     // Debug.Log($"轨迹终点: {TrajectoryLine.GetPosition(TrajectoryPoints - 1)}");
        // }

        private void UpdateTrajectory()
        {
            if (!_isAiming || _currentProjectile == null) return;

            Vector2 startPos = _currentProjectile.transform.position;
            Vector2 startVelocity = _throwDirection * ThrowForce;
            
            Vector2 previousPoint = startPos;
            int actualPoints = TrajectoryPoints; // 每次循环前重置为完整点数

            // 强制重置LineRenderer的positionCount
            TrajectoryLine.positionCount = TrajectoryPoints; // 新增此行
            _currentProjectile.transform.position = GetSpawnPosition();

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

        private void ExecuteThrow()
        {
            // 销毁虚拟投掷物
            Destroy(_currentProjectile);

            // 生成实际投掷物
            GameObject realProjectile = Instantiate(
                ThrowablePrefab,
                GetSpawnPosition(),
                Quaternion.identity
            );

            // 应用物理效果
            Rigidbody2D rb = realProjectile.GetComponent<Rigidbody2D>();
            rb.linearVelocity = _throwDirection * ThrowForce;

            // 重置状态
            _isAiming = false;
            TrajectoryLine.enabled = false;
        }
    }
}
