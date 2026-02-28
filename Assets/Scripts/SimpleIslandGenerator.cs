using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq;

public class SimpleIslandGenerator : MonoBehaviour
{
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
    public TextMeshProUGUI countTrees;
    public TextMeshProUGUI countStone;
    public GameObject treePrefab;
    public GameObject stonePrefab;
    public int treeCount = 10;
    public int stoneCount = 10;

    [Header("Настройки ресурсов")]
    public float resourceHeight = 0.2f;
    public float minDistanceBetweenResources = 0.5f;

    // Обновленный словарь для хранения ресурсов и их позиций
    public Dictionary<GameObject, Vector3> resources = new Dictionary<GameObject, Vector3>();

    // Дополнительный словарь для отслеживания типов ресурсов
    private Dictionary<GameObject, ResourceType> resourceTypes = new Dictionary<GameObject, ResourceType>();

    // Ссылка на менеджер удаления
    private DeleteModeManager deleteModeManager;

    // Типы ресурсов
    public enum ResourceType
    {
        Tree,
        Stone
    }

    void Start()
    {
        // Находим менеджер удаления
        deleteModeManager = FindObjectOfType<DeleteModeManager>();

        // Обновляем счетчики
        UpdateResourceCounters();
    }

    public void GenerateIsland()
    {
        countTrees.text = $"{treeCount}";
        countStone.text = $"{stoneCount}";

        // Очищаем старые клетки и ресурсы
        ClearIsland();

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
                    Vector3 position = new Vector3(
                        startX + x * cellSize,
                        0,
                        startZ + y * cellSize
                    );

                    GameObject cell = Instantiate(groundTilePrefab, position, Quaternion.identity, transform);
                    cell.transform.position = new Vector3(position.x, 0.1f, position.z);
                    cell.name = $"Cell_{x}_{y}";
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
            TryPlaceResource(treePrefab, isLand, ResourceType.Tree);
        }

        // Генерируем камни
        for (int i = 0; i < stoneCount; i++)
        {
            TryPlaceResource(stonePrefab, isLand, ResourceType.Stone);
        }
    }

    void TryPlaceResource(GameObject resourcePrefab, bool[,] isLand, ResourceType type)
    {
        int maxAttempts = 1000;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            attempts++;

            int x = UnityEngine.Random.Range(0, width);
            int y = UnityEngine.Random.Range(0, height);

            if (isLand[x, y])
            {
                Vector3 resourcePos = new Vector3(
                    startX + x,
                    resourceHeight,
                    startZ + y
                );

                if (resources.Values.Contains(resourcePos))
                    continue;
                else
                {
                    // Создаем ресурс
                    GameObject resource = Instantiate(resourcePrefab, resourcePos, Quaternion.identity, transform);

                    // Добавляем компонент DeletableObject
                    DeletableObject deletable = resource.AddComponent<DeletableObject>();
                    deletable.objectType = type == ResourceType.Tree ?
                        DeletableObject.ObjectType.Tree :
                        DeletableObject.ObjectType.Stone;


                    // Настраиваем визуальные эффекты (опционально)
                    // deletable.highlightColor = Color.red;

                    // Случайный поворот
                    resource.transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

                    // Сохраняем в словари
                    resources.Add(resource, resourcePos);
                    resourceTypes.Add(resource, type);

                    // Даем осмысленное имя
                    resource.name = $"{resourcePrefab.name}_{x}_{y}";

                    return;
                }
            }
        }

        Debug.LogWarning($"Не удалось разместить {resourcePrefab.name} после {maxAttempts} попыток");
    }

    // НОВЫЙ МЕТОД: Очистка острова
    public void ClearIsland()
    {
        // Удаляем все клетки земли
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        // Очищаем словари ресурсов
        resources.Clear();
        resourceTypes.Clear();
    }

    // НОВЫЙ МЕТОД: Удаление конкретного ресурса
    public void RemoveResource(GameObject resource)
    {
        if (resources.ContainsKey(resource))
        {
            // Определяем тип ресурса для обновления счетчика
            if (resourceTypes.ContainsKey(resource))
            {
                if (resourceTypes[resource] == ResourceType.Tree)
                {
                    treeCount = Mathf.Max(0, treeCount + 1);
                }
                else if (resourceTypes[resource] == ResourceType.Stone)
                {
                    stoneCount = Mathf.Max(0, stoneCount + 1);
                }
            }

            // Удаляем из словарей
            resources.Remove(resource);
            resourceTypes.Remove(resource);

            // Обновляем счетчики на UI
            UpdateResourceCounters();
        }
    }

    // НОВЫЙ МЕТОД: Обновление счетчиков ресурсов
    public void UpdateResourceCounters()
    {
        if (countTrees != null)
        {
            countTrees.text = $"{treeCount}";
        }

        if (countStone != null)
        {
            countStone.text = $"{stoneCount}";
        }
    }

    // НОВЫЙ МЕТОД: Получить все ресурсы определенного типа
    public List<GameObject> GetResourcesByType(ResourceType type)
    {
        return resourceTypes
            .Where(kvp => kvp.Value == type)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    // НОВЫЙ МЕТОД: Добавить новый ресурс вручную
    public void AddResourceManually(GameObject resourcePrefab, Vector3 position, ResourceType type)
    {
        GameObject newResource = Instantiate(resourcePrefab, position, Quaternion.identity, transform);

        // Добавляем компонент DeletableObject
        DeletableObject deletable = newResource.AddComponent<DeletableObject>();
        deletable.objectType = type == ResourceType.Tree ?
            DeletableObject.ObjectType.Tree :
            DeletableObject.ObjectType.Stone;

        resources.Add(newResource, position);
        resourceTypes.Add(newResource, type);

        if (type == ResourceType.Tree)
        {
            treeCount++;
        }
        else if (type == ResourceType.Stone)
        {
            stoneCount++;
        }

        UpdateResourceCounters();
    }

    public void SetResourceCounts(int trees, int stones)
    {
        treeCount = trees;
        stoneCount = stones;
        UpdateResourceCounters();
    }

    public void SetPosition(float x, float z)
    {
        startX = x;
        startZ = z;
    }
}