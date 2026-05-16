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

    public System.Action<GameObject> OnBuildingPlaced;
    public System.Action OnBuildingCancelled;
    public bool isStartBuilding = false;
    public bool isForcedPlacement = false;

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
            if (canPlace && CanAfford())
            {
                objectRenderer.material.color = Color.green;
            }
            else
            {
                objectRenderer.material.color = Color.red;
            }
        }

        // Левая кнопка - поставить
        if (Input.GetMouseButtonDown(0) && canPlace && CanAfford())
        {
            PlaceBuilding();
        }

        // Правая кнопка - отмена (только если не принудительная)
        if (Input.GetMouseButtonDown(1) && !isForcedPlacement)
        {
            CancelBuilding();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            transform.Rotate(Vector3.up * 90);
        }
    }

    private void CancelBuilding()
    {
        Debug.Log("Строительство отменено");
        OnBuildingCancelled?.Invoke();
        Destroy(gameObject);
    }

    private void PlaceBuilding()
    {
        // Для стартового строительства - не тратим ресурсы
        if (!isStartBuilding)
        {
            if (resourceManager != null)
            {
                if (!resourceManager.SpendResources(woodCost, stoneCost))
                {
                    Debug.Log($"Недостаточно ресурсов! Нужно: 🌲={woodCost}, 🪨={stoneCost}");
                    return;
                }
            }
        }

        isPlaced = true;

        if (objectRenderer != null)
        {
            objectRenderer.material = originalMaterial;
            objectRenderer.material.color = originalColor;
        }

        Debug.Log($"Здание построено!{(isStartBuilding ? " (стартовое)" : "")}");

        // Вызываем событие перед удалением компонента
        OnBuildingPlaced?.Invoke(gameObject);

        // Удаляем только компонент PlaceObjects, а не весь объект!
        Destroy(this);
    }

    private bool CanAfford()
    {
        if (isStartBuilding) return true;

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
            int b = Mathf.RoundToInt(a.x);
            int c = Mathf.RoundToInt(a.z);

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

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = canPlace ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.up * (objectSize.y / 2f), objectSize);
    }
}