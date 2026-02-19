using UnityEngine;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Island Strategy/Building Data")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public GameObject prefab;
    public Sprite icon;

    [Header("Размеры в клетках")]
    public int width = 1;
    public int height = 1;

    [Header("Стоимость")]
    public int woodCost = 10;
    public int stoneCost = 5;

    [Header("Характеристики")]
    public BuildingType type;

    public enum BuildingType
    {
        MainBuilding,  // Главное здание
        House,         // Жилой дом
        LumberMill,    // Лесопилка
        Mine,          // Шахта
        Barracks,      // Казармы
        Wall           // Стена
    }
}