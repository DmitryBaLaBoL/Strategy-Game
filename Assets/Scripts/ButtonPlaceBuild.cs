using UnityEngine;

public class ButtonPlaceBuild : MonoBehaviour
{
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    public void PlaceBuild()
    {
       
        GameObject buildingPrefab = GetSelectedBuildingPrefab();

        if (buildingPrefab == null)
        {
            Debug.LogError("Не выбран префаб здания!");
            return;
        }

        ResourcePanelManager resourceManager = ResourcePanelManager.Instance;

        if (resourceManager == null)
        {
            Debug.LogError("ResourcePanelManager не найден!");
            return;
        }

        // Получаем стоимость из компонента PlaceObjects в префабе
        PlaceObjects prefabPlaceScript = buildingPrefab.GetComponent<PlaceObjects>();
        if (prefabPlaceScript == null)
        {
            Debug.LogError("У префаба нет компонента PlaceObjects!");
            return;
        }

        int woodCost = prefabPlaceScript.woodCost;
        int stoneCost = prefabPlaceScript.stoneCost;

        // Проверяем достаточно ли ресурсов
        if (resourceManager.GetWood() < woodCost || resourceManager.GetStone() < stoneCost)
        {
            Debug.Log($"Недостаточно ресурсов! Нужно: Дерева={woodCost}, Камня={stoneCost}");
            return;
        }

        // Создаем экземпляр здания
        GameObject newBuilding = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);

        // Настраиваем компонент PlaceObjects у нового здания
        PlaceObjects placeScript = newBuilding.GetComponent<PlaceObjects>();
        if (placeScript != null)
        {
            placeScript.layer = groundLayer;
            placeScript.obstacleLayer = obstacleLayer;
            // Стоимость уже скопирована из префаба, ничего не меняем

            Collider buildingCollider = newBuilding.GetComponent<Collider>();
            if (buildingCollider != null)
            {
                placeScript.objectSize = buildingCollider.bounds.size;
            }
        }

        Debug.Log($"Здание выбрано для постройки. Стоимость: Дерева={woodCost}, Камня={stoneCost}");
    }

    // Временный метод - потом заменишь на выбор из UI
    public GameObject selectedPrefab;
    private GameObject GetSelectedBuildingPrefab()
    {
        return selectedPrefab;
    }
}