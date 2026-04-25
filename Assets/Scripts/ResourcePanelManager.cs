using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ResourcePanelManager : MonoBehaviour
{
    public static ResourcePanelManager Instance { get; private set; }

    [System.Serializable]
    public class ResourceUI
    {
        public ResourceType type;
        public TextMeshProUGUI text;
        public GameObject panel;
    }

    [Header("UI элементы")]
    public List<ResourceUI> resourceUIs = new List<ResourceUI>();
    public GameObject resourcePanel;

    [Header("Начальные ресурсы")]
    public int startWood = 0;
    public int startStone = 0;
    public int startGold = 0;
    public int startFood = 0;
    public int startMoves = 3;
    public int startBooks = 0;
    public int startWars = 0;

    // Хранилище ресурсов
    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    // События
    public System.Action<ResourceType, int> OnResourceChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeResources();
    }

    void Start()
    {
        if (resourcePanel != null)
            resourcePanel.SetActive(true);

        UpdateAllUI();
    }

    void InitializeResources()
    {
        resources[ResourceType.Wood] = startWood;
        resources[ResourceType.Stone] = startStone;
        resources[ResourceType.Gold] = startGold;
        resources[ResourceType.Food] = startFood;
        resources[ResourceType.Moves] = startMoves;
        resources[ResourceType.Books] = startBooks;
        resources[ResourceType.Wars] = startWars;
    }

    // Добавление ресурса
    public void AddResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
            resources[type] = 0;

        resources[type] += amount;
        UpdateUI(type);
        OnResourceChanged?.Invoke(type, resources[type]);
        Debug.Log($"Добавлено {type}: +{amount}. Всего: {resources[type]}");
    }

    // Трата ресурса
    public bool SpendResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
            return false;

        if (resources[type] >= amount)
        {
            resources[type] -= amount;
            UpdateUI(type);
            OnResourceChanged?.Invoke(type, resources[type]);
            Debug.Log($"Потрачено {type}: -{amount}. Осталось: {resources[type]}");
            return true;
        }

        Debug.Log($"Недостаточно {type}! Нужно: {amount}, есть: {resources[type]}");
        return false;
    }

    // Проверка наличия ресурса
    public bool HasResource(ResourceType type, int amount)
    {
        if (!resources.ContainsKey(type))
            return false;
        return resources[type] >= amount;
    }

    // Получение количества ресурса
    public int GetResource(ResourceType type)
    {
        if (!resources.ContainsKey(type))
            return 0;
        return resources[type];
    }

    // Трата нескольких ресурсов
    public bool SpendResources(Dictionary<ResourceType, int> costs)
    {
        // Сначала проверяем
        foreach (var cost in costs)
        {
            if (!HasResource(cost.Key, cost.Value))
            {
                Debug.Log($"Недостаточно {cost.Key}! Нужно: {cost.Value}");
                return false;
            }
        }

        // Потом тратим
        foreach (var cost in costs)
        {
            SpendResource(cost.Key, cost.Value);
        }

        return true;
    }

    // Установка ресурса
    public void SetResource(ResourceType type, int amount)
    {
        resources[type] = amount;
        UpdateUI(type);
        OnResourceChanged?.Invoke(type, amount);
    }

    public void AddWood(int amount)
    {
        AddResource(ResourceType.Wood, amount);
    }

    public void AddStone(int amount)
    {
        AddResource(ResourceType.Stone, amount);
    }

    public void AddGold(int amount)
    {
        AddResource(ResourceType.Gold, amount);
    }

    public void AddFood(int amount)
    {
        AddResource(ResourceType.Food, amount);
    }

    public void AddMoves(int amount)
    {
        AddResource(ResourceType.Moves, amount);
    }

    public void AddBooks(int amount)
    {
        AddResource(ResourceType.Books, amount);
    }

    public void AddWars(int amount)
    {
        AddResource(ResourceType.Wars, amount);
    }

    public bool SpendWood(int amount)
    {
        return SpendResource(ResourceType.Wood, amount);
    }

    public bool SpendStone(int amount)
    {
        return SpendResource(ResourceType.Stone, amount);
    }

    public bool SpendResources(int woodAmount, int stoneAmount)
    {
        Dictionary<ResourceType, int> costs = new Dictionary<ResourceType, int>();
        if (woodAmount > 0) costs[ResourceType.Wood] = woodAmount;
        if (stoneAmount > 0) costs[ResourceType.Stone] = stoneAmount;
        return SpendResources(costs);
    }

    public int GetWood()
    {
        return GetResource(ResourceType.Wood);
    }

    public int GetStone()
    {
        return GetResource(ResourceType.Stone);
    }

    public int GetGold()
    {
        return GetResource(ResourceType.Gold);
    }

    public int GetFood()
    {
        return GetResource(ResourceType.Food);
    }

    public int GetMoves()
    {
        return GetResource(ResourceType.Moves);
    }

    public int GetBooks()
    {
        return GetResource(ResourceType.Books);
    }

    public int GetWars()
    {
        return GetResource(ResourceType.Wars);
    }

    public void SetResources(int wood, int stone)
    {
        SetResource(ResourceType.Wood, wood);
        SetResource(ResourceType.Stone, stone);
    }

    // Обновление UI для конкретного ресурса
    void UpdateUI(ResourceType type)
    {
        var ui = resourceUIs.Find(x => x.type == type);
        if (ui != null && ui.text != null)
        {
            ui.text.text = $"{resources[type]}";
        }
    }

    // Обновление всего UI
    void UpdateAllUI()
    {
        foreach (var ui in resourceUIs)
        {
            if (ui.text != null && resources.ContainsKey(ui.type))
            {
                ui.text.text = $"{resources[ui.type]}";
            }
        }
    }

    // Получение словаря всех ресурсов
    public Dictionary<ResourceType, int> GetAllResources()
    {
        return new Dictionary<ResourceType, int>(resources);
    }
}