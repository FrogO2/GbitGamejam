using System.Collections;
using UnityEngine;
using MoreMountains.CorgiEngine;
using DialogueEditor; // 新增对话系统引用

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer interactionDot;
    [SerializeField] private float activationDistance = 3f;
    
    // 移除旧的面板相关字段
    [SerializeField] private NPCConversation conversation; // 新增对话资源引用
    
    private Transform _player;
    private Color _originalColor;
    private bool _isMouseOver;
    private bool _isHighlighted;
    private bool _isInConversation; // 新增对话状态标志

    void Start()
    {
        StartCoroutine(FindPlayerAfterDelay());
        _originalColor = interactionDot.color;
        interactionDot.gameObject.SetActive(false);
        
        // 自动获取对话组件（如果未手动分配）
        if (conversation == null)
        {
            conversation = GetComponent<NPCConversation>();
            if (conversation == null)
            {
                Debug.LogError("未找到NPCConversation组件！", this);
            }
        }
    }

    IEnumerator FindPlayerAfterDelay()
    {
        yield return null;
        _player = FindObjectOfType<Character>()?.transform;
        if (_player == null)
        {
            Debug.LogError("未找到玩家对象！");
        }
    }

    void Update()
    {
        if (_player == null || !_isMouseOver) return;

        float distance = Vector3.Distance(transform.position, _player.position);
        _isHighlighted = distance <= activationDistance;
        
        // 更新颜色和对话触发
        interactionDot.color = _isHighlighted ? Color.yellow : _originalColor;

        // 新增对话触发逻辑
        if (_isHighlighted && Input.GetMouseButtonDown(1) && !_isInConversation)
        {
            StartConversation();
        }
    }

    // 新增对话管理方法
    private void StartConversation()
    {
        // if (conversation == null || !ConversationManager.Instance)
        // {
        //     Debug.LogError("对话系统未正确配置！");
        //     return;
        // }

        _isInConversation = true;
        interactionDot.gameObject.SetActive(false);
        
        // 开始对话
        ConversationManager.Instance.StartConversation(GetComponent<NPCConversation>());
        
        // 注册对话结束事件
        ConversationManager.OnConversationEnded += EndConversation;
    }

    private void EndConversation()
    {
        _isInConversation = false;
        interactionDot.gameObject.SetActive(_isMouseOver);
        ConversationManager.OnConversationEnded -= EndConversation;
    }

    void OnMouseEnter()
    {
        if (_isInConversation) return;
        _isMouseOver = true;
        interactionDot.gameObject.SetActive(true);
    }

    void OnMouseExit()
    {
        _isMouseOver = false;
        interactionDot.gameObject.SetActive(false);
    }

    // 移除旧的面板相关方法
}