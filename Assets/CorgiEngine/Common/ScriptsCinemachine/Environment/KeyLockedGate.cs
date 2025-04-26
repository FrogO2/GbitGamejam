// using DialogueEditor;
using UnityEngine;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;
using System.Collections;
using UnityEditor.UI;



[RequireComponent(typeof(Collider2D))]
public class KeyLockedGate : MonoBehaviour
{
    [Header("钥匙设置")]
    [SerializeField] private string _targetInventoryName = "MainInventory";
    [SerializeField] private string _requiredKeyID = "YellowKey";
    [SerializeField][Min(1)] private int _requiredKeys = 1;

    [Header("碰撞体配置")]
    [Tooltip("用于触发检测的碰撞体(IsTrigger=true)")]
    [SerializeField] private Collider2D _triggerCollider;
    [Tooltip("用于物理阻挡的碰撞体(IsTrigger=false)")]
    [SerializeField] private Collider2D _blockCollider;

    [Header("门状态")]
    [SerializeField] private bool _permanentUnlock = true;
    [SerializeField] private Sprite _lockedSprite;
    [SerializeField] private Sprite _unlockedSprite;

    [Header("反馈系统")]
    [SerializeField] private MMFeedbacks _unlockFeedback;
    [SerializeField] private MMFeedbacks _accessDeniedFeedback;

    // 组件缓存
    private SpriteRenderer _spriteRenderer;
    private Inventory _mainInventory;
    private bool _isUnlocked;

    private void Awake()
    {
        InitializeComponents();
        LocateInventory();
        ConfigureColliders();
        UpdateGateState();
    }

    private void InitializeComponents()
    {
        // 自动获取必要组件
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            Debug.LogError("缺少SpriteRenderer组件", gameObject);

        // 自动创建碰撞体系统
        if (_triggerCollider == null)
        {
            _triggerCollider = gameObject.AddComponent<BoxCollider2D>();
            _triggerCollider.isTrigger = true;
            Debug.Log("自动创建触发碰撞体", gameObject);
        }

        if (_blockCollider == null)
        {
            var newCollider = gameObject.AddComponent<BoxCollider2D>();
            newCollider.isTrigger = false;
            _blockCollider = newCollider;
            Debug.Log("自动创建阻挡碰撞体", gameObject);
        }
    }

    private void ConfigureColliders()
    {
        // 设置碰撞体尺寸
        if (_triggerCollider is BoxCollider2D triggerBox)
        {
            triggerBox.size = new Vector2(1f, 4f); // 稍小于实际门尺寸
        }

        if (_blockCollider is BoxCollider2D blockBox)
        {
            blockBox.size = new Vector2(1f, 4f); // 精确匹配门尺寸
        }
    }

    private void LocateInventory()
    {
        GameObject inventoryHolder = GameObject.Find("Managers/Inventories");
        if (inventoryHolder == null)
        {
            Debug.LogError("库存管理器路径不存在", gameObject);
            return;
        }

        _mainInventory = inventoryHolder.GetComponentInChildren<Inventory>(true);
        if (_mainInventory == null)
            Debug.LogError("未找到库存组件", gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isUnlocked) return;
        if (!other.CompareTag("Player")) return;

        AttemptUnlockGate();
    }

    private void AttemptUnlockGate()
    {
        if (_mainInventory == null)
        {
            Debug.LogWarning("库存系统未初始化");
            return;
        }

        int keyCount = _mainInventory.GetQuantity(_requiredKeyID);
        Debug.Log($"钥匙检测：需要{_requiredKeys}，当前{keyCount}");

        if (keyCount >= _requiredKeys)
        {
            ProcessSuccessfulUnlock();
        }
        else
        {
            DenyAccess();
        }
    }

    private void ProcessSuccessfulUnlock()
    {
        if (!_mainInventory.RemoveItemByID(_requiredKeyID, _requiredKeys))
        {
            Debug.LogWarning("钥匙扣除失败");
            return;
        }

        _isUnlocked = true;
        UpdateGateState();
        _unlockFeedback?.PlayFeedbacks();

        if (!_permanentUnlock)
            StartCoroutine(ResetGateCoroutine(5f));
    }

    private IEnumerator ResetGateCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isUnlocked = false;
        UpdateGateState();
    }

    private void DenyAccess()
    {
        _accessDeniedFeedback?.PlayFeedbacks();
        Debug.Log($"需要 {_requiredKeys} 把 {_requiredKeyID} 钥匙");
    }

    private void UpdateGateState()
    {
        // 更新阻挡碰撞体状态
        if (_blockCollider != null)
        {
            bool shouldBlock = !_isUnlocked;
            _blockCollider.enabled = shouldBlock;
            Physics2D.SyncTransforms();
            Debug.Log($"阻挡碰撞体状态: {shouldBlock}");
        }

        // 更新视觉表现
        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = _isUnlocked ? _unlockedSprite : _lockedSprite;
        }
    }
}