using UnityEngine;

public class PlaceObjects : MonoBehaviour
{
    public LayerMask layer;
    public float rotateSpeed = 60f;
    public LayerMask obstacleLayer;
    public Vector3 objectSize = Vector3.one;

    [Header("Стоимость постройки")]
    public int woodCost = 0;
    public int stoneCost = 0;

    private bool canPlace = true;
    private Material originalMaterial;
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isPlaced = false;
    private ResourcePanelManager resourceManager;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
            originalColor = objectRenderer.material.color;
        }

        resourceManager = ResourcePanelManager.Instance;
        PositionObject();
    }

    private void Update()
    {
        if (isPlaced) return;

        PositionObject();

        if (objectRenderer != null)
        {
            // Проверяем можно ли поставить и хватает ли ресурсов
            if (canPlace && CanAfford())
            {
                objectRenderer.material.color = Color.green;
            }
            else
            {
                objectRenderer.material.color = Color.red;
            }
        }

        if (Input.GetMouseButtonDown(0) && canPlace && CanAfford())
        {
            PlaceBuilding();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            transform.Rotate(Vector3.up * 90);
        }
    }

    private void PlaceBuilding()
    {
        // Списываем ресурсы
        if (resourceManager != null)
        {
            // Используем правильный метод SpendResources с 2 параметрами (дерево, камень)
            if (!resourceManager.SpendResources(woodCost, stoneCost))
            {
                Debug.Log($"Недостаточно ресурсов для постройки! Нужно: Дерева={woodCost}, камня={stoneCost}");
                return;
            }
        }

        isPlaced = true;

        if (objectRenderer != null)
        {
            objectRenderer.material = originalMaterial;
            objectRenderer.material.color = originalColor;
        }

        Debug.Log($"Здание построено! Потрачено: Дерева={woodCost}, камня={stoneCost}");
        Destroy(gameObject.GetComponent<PlaceObjects>());
    }

    bool CanAfford()
    {
        if (resourceManager == null) return true;
        return resourceManager.GetWood() >= woodCost && resourceManager.GetStone() >= stoneCost;
    }

    private void PositionObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, layer))
        {
            Vector3 a = hit.point;
            int b = 0, c = 0;

            b = Mathf.RoundToInt(a.x);
            c = Mathf.RoundToInt(a.z);

            a.x = b;
            a.z = c;
            transform.position = a;
            canPlace = CheckPlacement();
        }
    }

    private bool CheckPlacement()
    {
        Collider[] colliders = Physics.OverlapBox(
            transform.position + Vector3.up * (objectSize.y / 2f),
            objectSize / 2f,
            transform.rotation,
            obstacleLayer
        );

        Collider ownCollider = GetComponent<Collider>();
        foreach (Collider col in colliders)
        {
            if (col != ownCollider)
            {
                return false;
            }
        }
        return true;
    }
}