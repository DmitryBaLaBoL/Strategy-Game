using UnityEngine;
using System.Collections;

public class StartBuildingManager : MonoBehaviour
{
    [Header("Обязательные здания (по порядку)")]
    public GameObject mainBuildingPrefab;    // Главное здание (ставится первым)
    public GameObject housePrefab;            // Обычный дом

    [Header("Настройки")]
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    [Header("UI")]
    public UnityEngine.UI.Text statusText;

    // Очередь строительства
    private enum BuildStep
    {
        MainBuilding,
        House1,
        House2,
        Complete
    }

    private BuildStep currentStep = BuildStep.MainBuilding;
    private GameObject currentPreviewBuilding;
    private PlaceObjects currentPlaceScript;
    private int housesPlaced = 0;

    void Start()
    {
        StartCoroutine(StartBuildingSequence());
    }

    IEnumerator StartBuildingSequence()
    {
        Debug.Log("НАЧАЛО ПЕРВОНАЛЬНОГО СТРОИТЕЛЬСТВА");

        // Ждем один кадр для инициализации
        yield return null;

        // Начинаем с главного здания
        StartNextBuilding();
    }

    void StartNextBuilding()
    {
        // Удаляем старый превью, если есть
        if (currentPreviewBuilding != null)
        {
            Destroy(currentPreviewBuilding);
        }

        GameObject prefabToBuild = null;
        string buildingName = "";

        switch (currentStep)
        {
            case BuildStep.MainBuilding:
                prefabToBuild = mainBuildingPrefab;
                buildingName = "ГЛАВНОЕ ЗДАНИЕ";
                break;
            case BuildStep.House1:
            case BuildStep.House2:
                prefabToBuild = housePrefab;
                buildingName = $"ДОМ {housesPlaced + 1}";
                break;
            case BuildStep.Complete:
                CompleteAllBuildings();
                return;
        }

        Debug.Log($"Поставьте {buildingName}");
        UpdateStatusText($"Поставьте {buildingName}\n(Левая кнопка мыши - поставить, ПКМ - отмена)");

        // Создаем превью здания
        currentPreviewBuilding = Instantiate(prefabToBuild, Vector3.zero, Quaternion.identity);
        currentPlaceScript = currentPreviewBuilding.GetComponent<PlaceObjects>();

        if (currentPlaceScript != null)
        {
            currentPlaceScript.layer = groundLayer;
            currentPlaceScript.obstacleLayer = obstacleLayer;
            currentPlaceScript.isStartBuilding = true;
            currentPlaceScript.isForcedPlacement = true; // Принудительная постановка

            // Подписываемся на событие постройки
            currentPlaceScript.OnBuildingPlaced += OnBuildingPlaced;
            currentPlaceScript.OnBuildingCancelled += OnBuildingCancelled;

            Collider collider = currentPreviewBuilding.GetComponent<Collider>();
            if (collider != null)
            {
                currentPlaceScript.objectSize = collider.bounds.size;
            }
        }
    }

    void OnBuildingPlaced(GameObject building)
    {
        Debug.Log($"Построено: {building.name}");

        // Проверяем, что здание не уничтожено
        if (building == null)
        {
            Debug.LogError("Здание уничтожено после постройки!");
            return;
        }

        // Добавляем компонент Building к зданию
        Building buildingScript = building.GetComponent<Building>();
        if (buildingScript == null)
        {
            buildingScript = building.AddComponent<Building>();
        }

        switch (currentStep)
        {
            case BuildStep.MainBuilding:
                buildingScript.peopleGenerated = 2;
                buildingScript.foodConsumption = 1;
                //buildingScript.isMainBuilding = true;
                AddDiceToBuilding(building, GetMainBuildingDiceConfig());
                break;

            case BuildStep.House1:
            case BuildStep.House2:
                housesPlaced++;
                buildingScript.peopleGenerated = 1;
                buildingScript.foodConsumption = 1;
                //buildingScript.isMainBuilding = false;
                AddDiceToBuilding(building, GetHouseDiceConfig(housesPlaced));
                break;
        }

        // Переходим к следующему шагу
        currentStep++;

        // Очищаем ссылку на превью (он уже построен и скрипт PlaceObjects удален)
        currentPreviewBuilding = null;
        currentPlaceScript = null;

        // Если не все построили, начинаем следующий
        if (currentStep != BuildStep.Complete)
        {
            StartNextBuilding();
        }
        else
        {
            CompleteAllBuildings();
        }
    }

    void OnBuildingCancelled()
    {
        Debug.Log("Постройка отменена, но это обязательное здание!");
        UpdateStatusText("Это обязательное здание! Вы должны его поставить!");

        // Не переходим дальше, ждем пока поставят
        StartCoroutine(ShowWarningAndWait());
    }

    IEnumerator ShowWarningAndWait()
    {
        // Мигаем текстом
        float elapsed = 0f;
        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            if (statusText != null)
            {
                statusText.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time * 5f, 1f));
            }
            yield return null;
        }

        if (statusText != null)
            statusText.color = Color.white;

        UpdateStatusText(GetCurrentBuildingName());
    }

    string GetCurrentBuildingName()
    {
        switch (currentStep)
        {
            case BuildStep.MainBuilding: return "Поставьте ГЛАВНОЕ ЗДАНИЕ";
            case BuildStep.House1: return "Поставьте ДОМ 1";
            case BuildStep.House2: return "Поставьте ДОМ 2";
            default: return "";
        }
    }

    void AddDiceToBuilding(GameObject building, DiceConfig config)
    {
        BuildingDice buildingDice = building.AddComponent<BuildingDice>();
        buildingDice.Initialize(config);

        // Сохраняем в DiceManager
        if (DiceManager.Instance != null)
        {
            DiceManager.Instance.RegisterBuildingDice(buildingDice);
        }

        Debug.Log($"Добавлен кубик для {config.buildingName}");
    }

    DiceConfig GetMainBuildingDiceConfig()
    {
        return new DiceConfig
        {
            buildingName = "Главное здание",
            faces = new DiceFaceConfig[]
            {
                new DiceFaceConfig { resourceType = ResourceType.Wood, amount = 3 },
                new DiceFaceConfig { resourceType = ResourceType.Stone, amount = 2 },
                new DiceFaceConfig { resourceType = ResourceType.Gold, amount = 2 },
                new DiceFaceConfig { resourceType = ResourceType.Food, amount = 2 },
                new DiceFaceConfig { resourceType = ResourceType.Books, amount = 1 },
                new DiceFaceConfig { resourceType = ResourceType.Wars, amount = 1 }
            }
        };
    }

    DiceConfig GetHouseDiceConfig(int houseNumber)
    {
        if (houseNumber == 1)
        {
            return new DiceConfig
            {
                buildingName = "Дом 1",
                faces = new DiceFaceConfig[]
                {
                    new DiceFaceConfig { resourceType = ResourceType.Wood, amount = 2 },
                    new DiceFaceConfig { resourceType = ResourceType.Wood, amount = 1 },
                    new DiceFaceConfig { resourceType = ResourceType.Food, amount = 2 },
                    new DiceFaceConfig { resourceType = ResourceType.Food, amount = 1 },
                    new DiceFaceConfig { resourceType = ResourceType.Gold, amount = 1 },
                    new DiceFaceConfig { resourceType = ResourceType.Stone, amount = 1 }
                }
            };
        }
        else
        {
            return new DiceConfig
            {
                buildingName = "Дом 2",
                faces = new DiceFaceConfig[]
                {
                    new DiceFaceConfig { resourceType = ResourceType.Stone, amount = 2 },
                    new DiceFaceConfig { resourceType = ResourceType.Stone, amount = 1 },
                    new DiceFaceConfig { resourceType = ResourceType.Food, amount = 2 },
                    new DiceFaceConfig { resourceType = ResourceType.Wood, amount = 2 },
                    new DiceFaceConfig { resourceType = ResourceType.Gold, amount = 1 },
                    new DiceFaceConfig { resourceType = ResourceType.Books, amount = 1 }
                }
            };
        }
    }

    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    void CompleteAllBuildings()
    {
        Debug.Log("ВСЕ ЗДАНИЯ ПОСТРОЕНЫ");
        UpdateStatusText("Все здания построены! Начинаем игру...");

        // Запускаем фазу броска кубиков через 1 секунду
        StartCoroutine(StartDicePhaseAfterDelay());
    }

    IEnumerator StartDicePhaseAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        GamePhaseManager phaseManager = FindObjectOfType<GamePhaseManager>();
        if (phaseManager != null)
        {
            phaseManager.StartDicePhase();
        }

        // Удаляем этот скрипт, он больше не нужен
        Destroy(gameObject, 2f);
    }
}