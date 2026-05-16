using UnityEngine;

public class PlaceShip : MonoBehaviour
{
    public LayerMask waterLayer;
    public LayerMask obstacleLayer;
    public float rotateSpeed = 60f;
    public Vector3 shipSize = Vector3.one;

    [Header("Стоимость")]
    public int woodCost = 10;
    public int stoneCost = 5;
    public int goldCost = 3;

    public System.Action<GameObject> OnShipPlaced;

    private bool canPlace = true;
    private Material originalMaterial;
    private Renderer shipRenderer;
    private Color originalColor;
    private bool isPlaced = false;
    private ResourcePanelManager resourceManager;

    private void Start()
    {
        shipRenderer = GetComponent<Renderer>();
        if (shipRenderer != null)
        {
            originalMaterial = shipRenderer.material;
            originalColor = shipRenderer.material.color;
        }

        resourceManager = ResourcePanelManager.Instance;
        PositionShip();
    }

    private void Update()
    {
        if (isPlaced) return;
        PositionShip();

        if (shipRenderer != null)
        {
            shipRenderer.material.color = canPlace ? Color.green : Color.red;
        }

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            PlaceShips();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            transform.Rotate(Vector3.up * 90);
        }
    }

    private void PlaceShips()
    {
        if (resourceManager != null)
        {
            resourceManager.SpendResource(ResourceType.Wood, woodCost);
            resourceManager.SpendResource(ResourceType.Stone, stoneCost);
            resourceManager.SpendResource(ResourceType.Gold, goldCost);
        }

        isPlaced = true;

        if (shipRenderer != null)
        {
            shipRenderer.material = originalMaterial;
            shipRenderer.material.color = originalColor;
        }

        OnShipPlaced?.Invoke(gameObject);
        Destroy(this);
    }

    private void PositionShip()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, waterLayer))
        {
            transform.position = hit.point;
            canPlace = CheckPlacement();
        }
        else
        {
            canPlace = false;
        }
    }

    private bool CheckPlacement()
    {
        Collider[] colliders = Physics.OverlapBox(
            transform.position + Vector3.up * (shipSize.y / 2f),
            shipSize / 2f,
            transform.rotation,
            obstacleLayer
        );

        Collider ownCollider = GetComponent<Collider>();
        foreach (Collider col in colliders)
        {
            if (col != ownCollider)
                return false;
        }
        return true;
    }
}