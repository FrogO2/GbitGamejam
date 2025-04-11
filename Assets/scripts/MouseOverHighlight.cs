using UnityEngine;

public class MouseOverHighlight : MonoBehaviour
{
    private Material originalMaterial;
    public Material highlightMaterial;
    private Renderer renderer;
    
    void Start()
    {
        renderer = GetComponent<Renderer>();
        originalMaterial = renderer.material;
    }
    
    void OnMouseEnter()
    {
        renderer.material = highlightMaterial;
    }
    
    void OnMouseExit()
    {
        renderer.material = originalMaterial;
    }
}
