using UnityEngine;

public class ButtonPlaceBuild : MonoBehaviour
{
    [Header("Настройки размещения")]
    public LayerMask groundLayer;    // Для зданий
    public LayerMask waterLayer;     // Для кораблей
    public LayerMask obstacleLayer;

    [Header("Тип объекта")]
    public bool isShip = false;      // Если true - ставим корабль на воду, если false - здание на землю

    public void PlaceBuild()
    {
        GameObject selectedPrefab = GetSelectedBuildingPrefab();

        if (selectedPrefab == null)
        {
            Debug.LogError("Не выбран префаб!");
            return;
        }

        ResourcePanelManager resourceManager = ResourcePanelManager.Instance;

        if (resourceManager == null)
        {
            Debug.LogError("ResourcePanelManager не найден!");
            return;
        }

        // Проверяем тип объекта и получаем стоимость
        int woodCost = 0;
        int stoneCost = 0;
        int goldCost = 0;

        if (isShip)
        {
            // Для корабля используем PlaceShip
            PlaceShip prefabShipScript = selectedPrefab.GetComponent<PlaceShip>();
            if (prefabShipScript == null)
            {
                Debug.LogError("У префаба корабля нет компонента PlaceShip!");
                return;
            }
            woodCost = prefabShipScript.woodCost;
            stoneCost = prefabShipScript.stoneCost;
            goldCost = prefabShipScript.goldCost;

            // Проверяем ресурсы для корабля
            if (resourceManager.GetWood() < woodCost ||
                resourceManager.GetStone() < stoneCost ||
                resourceManager.GetGold() < goldCost)
            {
                Debug.Log($"Недостаточно ресурсов для корабля! Нужно: 🌲={woodCost}, 🪨={stoneCost}, 💛={goldCost}");
                return;
            }
        }
        else
        {
            // Для здания используем PlaceObjects
            PlaceObjects prefabPlaceScript = selectedPrefab.GetComponent<PlaceObjects>();
            if (prefabPlaceScript == null)
            {
                Debug.LogError("У префаба здания нет компонента PlaceObjects!");
                return;
            }
            woodCost = prefabPlaceScript.woodCost;
            stoneCost = prefabPlaceScript.stoneCost;

            // Проверяем ресурсы для здания
            if (resourceManager.GetWood() < woodCost || resourceManager.GetStone() < stoneCost)
            {
                Debug.Log($"Недостаточно ресурсов для здания! Нужно: 🌲={woodCost}, 🪨={stoneCost}");
                return;
            }
        }

        // Создаем экземпляр
        GameObject newObject = Instantiate(selectedPrefab, Vector3.zero, Quaternion.identity);

        if (isShip)
        {
            // Настраиваем корабль
            PlaceShip placeScript = newObject.GetComponent<PlaceShip>();
            if (placeScript != null)
            {
                placeScript.waterLayer = waterLayer;
                placeScript.obstacleLayer = obstacleLayer;

                Collider shipCollider = newObject.GetComponent<Collider>();
                if (shipCollider != null)
                {
                    placeScript.shipSize = shipCollider.bounds.size;
                }
            }
            Debug.Log($"Корабль выбран для постройки. Стоимость: 🌲={woodCost}, 🪨={stoneCost}, 💛={goldCost}");
        }
        else
        {
            // Настраиваем здание
            PlaceObjects placeScript = newObject.GetComponent<PlaceObjects>();
            if (placeScript != null)
            {
                placeScript.layer = groundLayer;
                placeScript.obstacleLayer = obstacleLayer;

                Collider buildingCollider = newObject.GetComponent<Collider>();
                if (buildingCollider != null)
                {
                    placeScript.objectSize = buildingCollider.bounds.size;
                }
            }
            Debug.Log($"Здание выбрано для постройки. Стоимость: 🌲={woodCost}, 🪨={stoneCost}");
        }
    }

    // Метод для выбора префаба (можно расширить для выбора разных кораблей/зданий)
    public GameObject selectedPrefab;
    private GameObject GetSelectedBuildingPrefab()
    {
        return selectedPrefab;
    }
}