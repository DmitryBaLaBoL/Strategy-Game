using UnityEngine;

[System.Serializable]
public class DiceFaceConfig
{
    public ResourceType resourceType;
    public int amount;
}

[System.Serializable]
public class DiceConfig
{
    public string buildingName;
    public DiceFaceConfig[] faces = new DiceFaceConfig[6];
}

public class BuildingDice : MonoBehaviour
{
    private DiceConfig config;

    public void Initialize(DiceConfig diceConfig)
    {
        config = diceConfig;
    }

    public ResourceType RollDice()
    {
        if (config == null || config.faces.Length == 0)
        {
            Debug.LogWarning($"У здания {gameObject.name} нет конфигурации кубика!");
            return ResourceType.Wood;
        }

        int faceIndex = Random.Range(0, config.faces.Length);
        DiceFaceConfig result = config.faces[faceIndex];

        Debug.Log($"Здание {config.buildingName} принесло: {result.resourceType} +{result.amount}");

        // Добавляем ресурс
        if (ResourcePanelManager.Instance != null)
        {
            ResourcePanelManager.Instance.AddResource(result.resourceType, result.amount);
        }

        return result.resourceType;
    }

    public string GetBuildingName()
    {
        return config != null ? config.buildingName : "Неизвестное здание";
    }
}