using UnityEngine;
using UnityEngine.UI;

public class DescriptionUI : MonoBehaviour
{
    [SerializeField] private Text descriptionText;
    [SerializeField] private Button closeButton;
    private InteractableObject _parentObject;
    private void Start()
    {
        closeButton.onClick.AddListener(ClosePanel);
    }

    // 设置描述文本
    public void SetText(string text)
    {
        descriptionText.text = text;
    }
    
    public void Initialize(InteractableObject parent)
    {
        _parentObject = parent;
    }

    public void ClosePanel()
    {
        Destroy(gameObject);
        _parentObject?.OnPanelClosed(); // 通知父对象面板已关闭
    }
}