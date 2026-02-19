using UnityEngine;

public class Building : MonoBehaviour
{
    public BuildingData buildingData;
    public Vector2Int originCell; // Левая нижняя клетка, которую занимает здание

    // Клетки, которые занимает здание
    private Vector2Int[] occupiedCells;

    public void Initialize(BuildingData data, Vector2Int origin)
    {
        buildingData = data;
        originCell = origin;

        // Вычисляем все занимаемые клетки
        occupiedCells = new Vector2Int[data.width * data.height];
        int index = 0;

        for (int x = 0; x < data.width; x++)
        {
            for (int y = 0; y < data.height; y++)
            {
                occupiedCells[index++] = new Vector2Int(origin.x + x, origin.y + y);
            }
        }
    }

    public bool IsCellPartOfBuilding(Vector2Int cellPos)
    {
        foreach (Vector2Int occupied in occupiedCells)
        {
            if (occupied == cellPos)
                return true;
        }
        return false;
    }

    // Получить центр здания для позиционирования
    public Vector3 GetCenterPosition(float cellSize)
    {
        return new Vector3(
            originCell.x + buildingData.width / 2f,
            0,
            originCell.y + buildingData.height / 2f
        ) * cellSize;
    }
}