using UnityEngine;
using UnityEngine.UIElements;

public class Cell : MonoBehaviour
{
    public Vector2Int gridPosition;
    public bool isOccupied = false;
    public Building currentBuilding;
    public ResourceType resource; // Камень, дерево и т.д.

    private Renderer cellRenderer;
    private Color originalColor;

    void Awake()
    {
        cellRenderer = GetComponent<Renderer>();
        if (cellRenderer != null)
            originalColor = cellRenderer.material.color;
    }

    public void SetHighlight(bool highlighted, Color color)
    {
        if (cellRenderer != null)
        {
            if (highlighted)
                cellRenderer.material.color = color;
            else
                cellRenderer.material.color = originalColor;
        }
    }

    public bool CanBuild()
    {
        return !isOccupied && currentBuilding == null;
    }
}

public enum ResourceType
{
    None,
    Tree,
    Stone
}