using UnityEngine;

public class ButtonPlaceBuild : MonoBehaviour
{
    public GameObject buildingPrefab;
    public LayerMask groundLayer; // Слой земли
    public LayerMask obstacleLayer; // Слой препятствий

    public void PlaceBuild()
    {
        // Создаем экземпляр здания
        GameObject newBuilding = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);

        // Настраиваем компонент PlaceObjects
        PlaceObjects placeScript = newBuilding.GetComponent<PlaceObjects>();
        if (placeScript != null)
        {
            placeScript.layer = groundLayer;
            placeScript.obstacleLayer = obstacleLayer;

            // Можно задать размер объекта для проверки коллизий
            // Автоматически определяем размер по коллайдеру
            Collider buildingCollider = newBuilding.GetComponent<Collider>();
            if (buildingCollider != null)
            {
                placeScript.objectSize = buildingCollider.bounds.size;
            }
        }
    }
}