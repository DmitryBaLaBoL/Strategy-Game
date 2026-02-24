using UnityEngine;

public class ButtonPlaceDelete : MonoBehaviour
{
    public GameObject buildingPrefab;

    public void PlaceBuild()
    {
        
        // Создаем экземпляр здания
        GameObject newBuilding = Instantiate(buildingPrefab, Vector3.zero, Quaternion.identity);

        // Компонент BuildableObject сам запустит режим размещения
        // Ничего дополнительно делать не нужно
    }
}
