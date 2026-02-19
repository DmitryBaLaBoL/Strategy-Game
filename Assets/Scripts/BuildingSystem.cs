using UnityEngine;
using System.Collections.Generic;

public class BuildingSystem : MonoBehaviour
{
    [Header("Ссылки")]
    public SimpleIslandGenerator islandGenerator;
    public Camera mainCamera;

    [Header("Настройки")]
    public LayerMask cellLayerMask;
    public Color validPlacementColor = Color.green;
    public Color invalidPlacementColor = Color.red;
    public Color highlightColor = Color.yellow;

    [Header("Доступные здания")]
    public BuildingData[] availableBuildings;

    // Текущее состояние
    private BuildingData selectedBuilding;
    private List<Cell> currentPreviewCells = new List<Cell>();
    private bool placementValid = false;
    private Vector2Int hoveredCell;

    void Update()
    {
        if (selectedBuilding != null)
        {
            HandleBuildingPlacement();
        }

        // Нажатие Escape для отмены строительства
        if (Input.GetKeyDown(KeyCode.Escape) && selectedBuilding != null)
        {
            CancelBuildingPlacement();
        }
    }

    // Вызывается из UI при выборе здания
    public void SelectBuilding(BuildingData building)
    {
        selectedBuilding = building;
        Debug.Log($"Выбрано здание: {building.buildingName}");
        ClearPreview();
    }

    void HandleBuildingPlacement()
    {
        // Луч из камеры в позицию мыши
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, cellLayerMask))
        {
            Cell cell = hit.collider.GetComponent<Cell>();

            if (cell != null)
            {
                Vector2Int newHoveredCell = cell.gridPosition;

                // Если навели на новую клетку, обновляем превью
                if (newHoveredCell != hoveredCell)
                {
                    hoveredCell = newHoveredCell;
                    UpdatePlacementPreview(hoveredCell);
                }

                // Проверяем валидность размещения
                placementValid = CanPlaceBuilding(selectedBuilding, hoveredCell);

                // Обработка клика для постройки
                if (Input.GetMouseButtonDown(0) && placementValid)
                {
                    PlaceBuilding(selectedBuilding, hoveredCell);
                }
            }
        }
        else
        {
            // Если не навели ни на одну клетку, очищаем превью
            ClearPreview();
        }
    }

    bool CanPlaceBuilding(BuildingData building, Vector2Int originCell)
    {
        // Проверяем, не выходит ли здание за границы сетки
        if (originCell.x + building.width > islandGenerator.width ||
            originCell.y + building.height > islandGenerator.height)
        {
            return false;
        }

        // Проверяем каждую клетку, которую займет здание
        for (int x = 0; x < building.width; x++)
        {
            for (int y = 0; y < building.height; y++)
            {
                Vector2Int checkPos = new Vector2Int(originCell.x + x, originCell.y + y);

                // Получаем клетку
                Cell cell = GetCellAtPosition(checkPos);

                // Если клетки нет или она занята - нельзя строить
                if (cell == null || !cell.CanBuild())
                {
                    return false;
                }

                // Проверяем, есть ли на клетке ресурс
                if (cell.resource != ResourceType.None)
                {
                    // Нельзя строить на ресурсах (можно добавить возможность вырубки)
                    return false;
                }
            }
        }

        return true;
    }

    void PlaceBuilding(BuildingData building, Vector2Int originCell)
    {
        // Создаем здание
        Vector3 centerPos = new Vector3(
            originCell.x + building.width / 2f,
            0,
            originCell.y + building.height / 2f
        ) * islandGenerator.cellSize;

        GameObject buildingObj = Instantiate(building.prefab, centerPos, Quaternion.identity);
        Building buildingComponent = buildingObj.AddComponent<Building>();
        buildingComponent.Initialize(building, originCell);

        // Помечаем клетки как занятые
        for (int x = 0; x < building.width; x++)
        {
            for (int y = 0; y < building.height; y++)
            {
                Vector2Int cellPos = new Vector2Int(originCell.x + x, originCell.y + y);
                Cell cell = GetCellAtPosition(cellPos);

                if (cell != null)
                {
                    cell.isOccupied = true;
                    cell.currentBuilding = buildingComponent;
                }
            }
        }

        Debug.Log($"Построено {building.buildingName} в позиции {originCell}");

        // Не сбрасываем выбранное здание, чтобы можно было строить несколько
        ClearPreview();
    }

    void UpdatePlacementPreview(Vector2Int originCell)
    {
        // Очищаем предыдущее превью
        ClearPreview();

        // Подсвечиваем клетки, которые займет здание
        for (int x = 0; x < selectedBuilding.width; x++)
        {
            for (int y = 0; y < selectedBuilding.height; y++)
            {
                Vector2Int checkPos = new Vector2Int(originCell.x + x, originCell.y + y);
                Cell cell = GetCellAtPosition(checkPos);

                if (cell != null)
                {
                    currentPreviewCells.Add(cell);

                    // Временная подсветка
                    bool canPlace = CanPlaceBuilding(selectedBuilding, originCell);
                    Color previewColor = canPlace ? validPlacementColor : invalidPlacementColor;
                    cell.SetHighlight(true, previewColor);
                }
            }
        }
    }

    void ClearPreview()
    {
        foreach (Cell cell in currentPreviewCells)
        {
            if (cell != null)
                cell.SetHighlight(false, Color.white);
        }
        currentPreviewCells.Clear();
    }

    void CancelBuildingPlacement()
    {
        selectedBuilding = null;
        ClearPreview();
        Debug.Log("Строительство отменено");
    }

    Cell GetCellAtPosition(Vector2Int gridPos)
    {
        // Ищем клетку по имени или можно хранить словарь
        string cellName = $"Cell_{gridPos.x}_{gridPos.y}";
        GameObject cellObj = GameObject.Find(cellName);

        if (cellObj != null)
            return cellObj.GetComponent<Cell>();

        return null;
    }
}