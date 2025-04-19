using System.Collections;
using UnityEngine;
using MoreMountains.CorgiEngine;
using Unity.VisualScripting; // 引入Corgi Engine命名空间

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private SpriteRenderer interactionDot;
    [SerializeField] private float activationDistance = 3f;
    [SerializeField] private GameObject descriptionPanelPrefab;
    [SerializeField] private float displayDuration = 3f; // 面板显示时间（秒）

    private GameObject _currentPanel; // 当前显示的面板实例
    private bool _isPanelActive; // 面板是否正在显示

    private Transform _player;
    private Color _originalColor;
    private bool _isMouseOver;
    private bool _isHighlighted;

    void Start()
    {
        StartCoroutine(FindPlayerAfterDelay()); // 延迟查找玩家
        _originalColor = interactionDot.color;
        interactionDot.gameObject.SetActive(false); 
    }

    IEnumerator FindPlayerAfterDelay()
    {
        yield return null; // 等待一帧
        _player = FindObjectOfType<Character>()?.transform; // 安全获取玩家
        if (_player == null)
        {
            Debug.LogError("未找到玩家对象！");
        }
    }
    

    void Update()
    {
        if (!_isMouseOver || _player == null) return; // 添加_player的空值检查

        // 计算角色与物体的距离
        float distance = Vector3.Distance(transform.position, _player.position);
    
        // 根据距离切换颜色并更新高亮状态
        _isHighlighted = (distance <= activationDistance); // 新增此行
        interactionDot.color = _isHighlighted ? Color.yellow : _originalColor;

        // 新增右键检测逻辑：仅在角色靠近时触发
        if (_isHighlighted && Input.GetMouseButtonDown(1)) 
        {
            ShowDescription();
        }
    }

    // private void ShowDescription()
    // {
    //     if (descriptionPanelPrefab == null) return;
    //
    //     // 实例化面板并设置位置
    //     GameObject panel = Instantiate(descriptionPanelPrefab);
    //
    //     // 将面板绑定到Canvas（假设Canvas已设为目标坐标）
    //     panel.transform.SetParent(GameObject.Find("Canvas").transform, false);
    //
    //     // 强制设置面板本地坐标为(0,0,0)，继承Canvas的位置
    //     panel.GetComponent<RectTransform>().localPosition = Vector3.zero;
    // }

    // 鼠标悬停时触发
    void OnMouseEnter()
    {
        _isMouseOver = true;
        interactionDot.gameObject.SetActive(true);
    }

    // 鼠标离开时触发
    void OnMouseExit()
    {
        _isMouseOver = false;
        interactionDot.gameObject.SetActive(false);
    }
    
    public void OnPanelClosed()
    {
        _isPanelActive = false;
        _currentPanel = null;
    }

// 修改ShowDescription方法，传递引用
    private void ShowDescription()
    {
        if (_isPanelActive || descriptionPanelPrefab == null) return;

        _currentPanel = Instantiate(descriptionPanelPrefab, GameObject.Find("Canvas").transform);
        _currentPanel.GetComponent<DescriptionUI>().Initialize(this); // 传递引用
        _isPanelActive = true;
        StartCoroutine(AutoClosePanel());
    }

    // 协程：等待指定时间后销毁面板
    private IEnumerator AutoClosePanel()
    {
        yield return new WaitForSeconds(displayDuration);
        
        if (_currentPanel != null)
        {
            Destroy(_currentPanel);
            _isPanelActive = false;
        }
    }

}