using UnityEngine;
using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

public class ConditionalTeleporter : Teleporter 
{
    [Header("条件设置")]
    [Tooltip("用于检测的碰撞区域")]
    public Collider2D DetectionArea;
    [Tooltip("默认传送目标")]
    public Teleporter DefaultDestination;
    [Tooltip("条件满足时传送目标")]
    public Teleporter ConditionalDestination;
    [Tooltip("离开检测区域后保持条件的时间")]
    public float ConditionGracePeriod = 0.2f;

    private float _lastConditionTime;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (DetectionArea != null && DetectionArea.IsTouching(other))
        {
            _lastConditionTime = Time.time;
        }
    }
    protected override void Teleport(Collider2D collider)
    {
        // 实时检测碰撞区域接触状态
        bool conditionMet = (Time.time - _lastConditionTime <= ConditionGracePeriod)|(DetectionArea != null && DetectionArea.IsTouching(collider));

        
        // 动态设置目标
        Destination = conditionMet ? ConditionalDestination : DefaultDestination;

        // 调试信息
        Debug.Log($"传送条件: {conditionMet}, 目标: {Destination?.name ?? "null"}");
        
        // 调用基类传送逻辑
        base.Teleport(collider);
    }

    protected void OnDrawGizmos()
    {
        // 绘制检测区域连线
        if (DetectionArea != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, DetectionArea.bounds.center);
        }
    }
}