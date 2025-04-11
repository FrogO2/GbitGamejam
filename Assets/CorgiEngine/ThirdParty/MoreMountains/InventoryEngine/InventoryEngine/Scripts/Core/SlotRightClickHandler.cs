using UnityEngine;
using UnityEngine.EventSystems;
using MoreMountains.InventoryEngine;

public class SlotRightClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("引用设置")]
    public InventoryInputManager inventoryInputManager;
    
    // 实现接口方法
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 左键保持原有功能（自动通过Button组件处理）
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            GetComponent<InventorySlot>().SlotClicked();
            inventoryInputManager.ToggleInventory();
        }
    }
}