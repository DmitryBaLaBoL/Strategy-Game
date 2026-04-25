using UnityEngine;
using System.Collections.Generic;

public class FoodConsumptionManager : MonoBehaviour
{
    public static FoodConsumptionManager Instance { get; private set; }

    private int totalFoodConsumption = 0;
    private List<int> consumers = new List<int>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddConsumer(int foodNeeded)
    {
        consumers.Add(foodNeeded);
        RecalculateConsumption();
    }

    void RecalculateConsumption()
    {
        totalFoodConsumption = 0;
        foreach (var consumer in consumers)
        {
            totalFoodConsumption += consumer;
        }
    }

    public int GetTotalConsumption()
    {
        return totalFoodConsumption;
    }

    // Вызывается в конце хода
    public bool ConsumeFood()
    {
        int availableFood = ResourcePanelManager.Instance.GetResource(ResourceType.Food);

        if (availableFood >= totalFoodConsumption)
        {
            ResourcePanelManager.Instance.SpendResource(ResourceType.Food, totalFoodConsumption);
            Debug.Log($"Съедено еды: {totalFoodConsumption}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Недостаточно еды! Нужно: {totalFoodConsumption}, есть: {availableFood}");
            // todo: Штраф за голод
            return false;
        }
    }
}