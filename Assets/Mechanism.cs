using UnityEngine;

public class Mechanism : MonoBehaviour
{
    [Header("触发效果")]
    public Animator targetAnimator;
    public string activateTrigger = "Activate";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 触发动画
        if (targetAnimator != null)
            targetAnimator.SetTrigger(activateTrigger);

        // 其他逻辑（如播放声音、移动平台等）
        Debug.Log("机关已触发！");
    }
}