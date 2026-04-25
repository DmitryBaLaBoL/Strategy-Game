using UnityEngine;
using System.Collections.Generic;

public class Building : MonoBehaviour
{
    [System.Serializable]
    public class ResourceCost
    {
        public ResourceType resourceType;
        public int amount;
    }

    [Header("Стоимость постройки")]
    public List<ResourceCost> buildCost = new List<ResourceCost>();

    [Header("При постройке")]
    public int peopleGenerated = 1; // Сколько людей дает здание
    public int foodConsumption = 1; // Сколько еды потребляет

    [Header("Визуализация")]
    public GameObject buildingModel;
    public GameObject previewModel;
    public Material canBuildMaterial;
    public Material cannotBuildMaterial;

    private bool isBuilt = false;
    private Renderer previewRenderer;

    void Start()
    {
        if (previewModel != null)
        {
            previewRenderer = previewModel.GetComponent<Renderer>();
        }

        if (buildingModel != null) buildingModel.SetActive(false);
        if (previewModel != null) previewModel.SetActive(true);

        UpdatePreviewColor();
    }

    void Update()
    {
        if (!isBuilt)
        {
            UpdatePreviewColor();

            if (Input.GetMouseButtonDown(0))
            {
                TryBuild();
            }
        }
    }

    void UpdatePreviewColor()
    {
        if (previewRenderer != null && canBuildMaterial != null && cannotBuildMaterial != null)
        {
            previewRenderer.material = CanAfford() ? canBuildMaterial : cannotBuildMaterial;
        }
    }

    bool CanAfford()
    {
        if (ResourcePanelManager.Instance == null) return false;

        foreach (var cost in buildCost)
        {
            if (!ResourcePanelManager.Instance.HasResource(cost.resourceType, cost.amount))
                return false;
        }
        return true;
    }

    void TryBuild()
    {
        if (!CanAfford())
        {
            Debug.Log("Недостаточно ресурсов для постройки!");
            return;
        }

        // Списываем ресурсы
        foreach (var cost in buildCost)
        {
            ResourcePanelManager.Instance.SpendResource(cost.resourceType, cost.amount);
        }

        Build();
    }

    void Build()
    {
        isBuilt = true;

        if (buildingModel != null) buildingModel.SetActive(true);
        if (previewModel != null) previewModel.SetActive(false);

        // Добавляем людей
        PeopleManager.Instance?.AddPeople(peopleGenerated);

        // Добавляем потребление еды
        if (foodConsumption > 0)
        {
            FoodConsumptionManager.Instance?.AddConsumer(foodConsumption);
        }

        Destroy(GetComponent<PlaceObjects>());
        Debug.Log($"Здание построено! +{peopleGenerated} людей");
    }
}