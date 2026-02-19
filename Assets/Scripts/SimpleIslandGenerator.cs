using UnityEngine;
using System.Collections.Generic;

public class SimpleIslandGenerator : MonoBehaviour
{
    public Dictionary<Vector2Int, Cell> cellsDictionary = new Dictionary<Vector2Int, Cell>();

    [Header("Размеры острова")]
    public int width = 15;
    public int height = 15;
    public float cellSize = 1.1f;

    [Header("Позиция острова")]
    public float startX = 0f;
    public float startZ = 0f;

    [Header("Настройки формы")]
    [Range(0.1f, 1f)]
    public float islandSize = 0.7f;
    [Range(0, 1f)]
    public float roughness = 0.3f;

    [Header("Префабы")]
    public GameObject groundTilePrefab;

    [Header("Ресурсы")]
    public GameObject treePrefab;
    public GameObject stonePrefab;
    public int treeCount = 10;
    public int stoneCount = 10;

    [Header("Настройки ресурсов")]
    public float resourceHeight = 0.2f;
    public float minDistanceBetweenResources = 0.5f;

    // Список для хранения позиций ресурсов
    private List<Vector3> resourcePositions = new List<Vector3>();

    [ContextMenu("Сгенерировать новый остров")]
    public void GenerateIsland()
    {
        // Очищаем старые клетки
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Очищаем список ресурсов
        resourcePositions.Clear();

        // Центр острова в координатах сетки
        Vector2 center = new Vector2(width / 2f, height / 2f);

        // Создаем массив для хранения типа клеток
        bool[,] isLand = new bool[width, height];

        // Сначала помечаем все как пустое
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                isLand[x, y] = false;
            }
        }

        // Рисуем круг в центре (базовая форма острова)
        int centerX = width / 2;
        int centerY = height / 2;
        int radius = Mathf.FloorToInt(Mathf.Min(width, height) * islandSize / 2);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float distanceToCenter = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));

                // Базовый круг с случайными искажениями
                float noise = Mathf.PerlinNoise(x * 0.5f, y * 0.5f) * roughness;
                float distortedRadius = radius * (1 - roughness / 2) + noise * radius * roughness;

                if (distanceToCenter < distortedRadius)
                {
                    isLand[x, y] = true;
                }
            }
        }

        // Убеждаемся, что центр точно суша
        isLand[centerX, centerY] = true;
        isLand[centerX + 1, centerY] = true;
        isLand[centerX - 1, centerY] = true;
        isLand[centerX, centerY + 1] = true;
        isLand[centerX, centerY - 1] = true;

        // Убираем отдельно стоящие клетки суши
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (isLand[x, y])
                {
                    int neighbors = 0;
                    if (isLand[x + 1, y]) neighbors++;
                    if (isLand[x - 1, y]) neighbors++;
                    if (isLand[x, y + 1]) neighbors++;
                    if (isLand[x, y - 1]) neighbors++;

                    if (neighbors == 0)
                    {
                        isLand[x, y] = false;
                    }
                }
            }
        }

        // Создаем все клетки земли с учетом стартовой позиции
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (isLand[x, y])
                {
                    // Добавляем startX и startZ к позиции
                    Vector3 position = new Vector3(
                        startX + x * cellSize,
                        0,
                        startZ + y * cellSize
                    );

                    GameObject cell = Instantiate(groundTilePrefab, position, Quaternion.identity, transform);
                    cell.transform.position = new Vector3(position.x, 0.1f, position.z);
                    cell.name = $"Cell_{x}_{y}";

                    Cell cellComponent = cell.AddComponent<Cell>();
                    cellComponent.gridPosition = new Vector2Int(x, y);

                    /*if ( тут проверка на ресурс )
                    {
                        cellComponent.resource = ResourceType.Tree; // или Stone
                    }*/

                    cellsDictionary[new Vector2Int(x, y)] = cellComponent;
                }
            }
        }

        // Генерируем ресурсы с учетом стартовой позиции
        GenerateResources(isLand);

        Debug.Log($"Остров сгенерирован! Позиция: ({startX}, {startZ}), Размер: {width}x{height}");
    }

    void GenerateResources(bool[,] isLand)
    {
        // Генерируем деревья
        for (int i = 0; i < treeCount; i++)
        {
            TryPlaceResource(treePrefab, isLand);
        }

        // Генерируем камни
        for (int i = 0; i < stoneCount; i++)
        {
            TryPlaceResource(stonePrefab, isLand);
        }
    }

    void TryPlaceResource(GameObject resourcePrefab, bool[,] isLand)
    {
        int maxAttempts = 1000;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            attempts++;

            // Выбираем случайную клетку
            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            // Проверяем, что это суша
            if (isLand[x, y])
            {
                // Вычисляем позицию в пределах клетки с учетом startX и startZ
                float offsetX = Random.Range(-0.4f, 0.4f) * cellSize;
                float offsetZ = Random.Range(-0.4f, 0.4f) * cellSize;

                Vector3 resourcePos = new Vector3(
                    startX + x * cellSize + offsetX,
                    resourceHeight,
                    startZ + y * cellSize + offsetZ
                );

                // Проверяем, не слишком близко к другим ресурсам
                bool tooClose = false;
                foreach (Vector3 pos in resourcePositions)
                {
                    if (Vector3.Distance(resourcePos, pos) < minDistanceBetweenResources)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    // Создаем ресурс
                    GameObject resource = Instantiate(resourcePrefab, resourcePos, Quaternion.identity, transform);

                    // Случайный поворот
                    resource.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                    // Добавляем в список
                    resourcePositions.Add(resourcePos);

                    // Даем осмысленное имя
                    resource.name = $"{resourcePrefab.name}_{x}_{y}";

                    return; // Успешно разместили
                }
            }
        }

        Debug.LogWarning($"Не удалось разместить {resourcePrefab.name} после {maxAttempts} попыток");
    }

    public void SetResourceCounts(int trees, int stones)
    {
        treeCount = trees;
        stoneCount = stones;
    }

    // Дополнительный метод для установки позиции
    public void SetPosition(float x, float z)
    {
        startX = x;
        startZ = z;
    }
}