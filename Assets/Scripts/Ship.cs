using UnityEngine;

public class Ship : MonoBehaviour
{
    [Header("Характеристики")]
    public string shipName = "Корабль";
    public int maxWarriors = 8;
    public int currentWarriors = 0;
    public int attackPower = 5;

    [Header("Визуализация")]
    public GameObject shipModel;

    void Start()
    {
        currentWarriors = maxWarriors;

        if (ShipManager.Instance != null)
            ShipManager.Instance.RegisterShip(this);

        Debug.Log($"Корабль {shipName} создан. Воинов: {currentWarriors}");

        // Проверяем наличие коллайдера
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"У корабля {shipName} нет границ! Клик не будет работать!");
        }
        else
        {
            Debug.Log($"У корабля {shipName} есть коллайдер: {col.GetType().Name}");
        }
    }

    void OnDestroy()
    {
        if (ShipManager.Instance != null)
            ShipManager.Instance.UnregisterShip(this);

        Debug.Log($"Корабль {shipName} уничтожен!");
    }

    public void RemoveWarriors(int count)
    {
        currentWarriors = Mathf.Max(0, currentWarriors - count);
        Debug.Log($"На корабле {shipName} осталось воинов: {currentWarriors}");

        if (currentWarriors == 0)
        {
            Debug.Log($"Корабль {shipName} опустел");
            if (shipModel != null)
            {
                Renderer rend = shipModel.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = Color.gray;
                }
            }
        }
    }

    void OnMouseDown()
    {
        Debug.Log($"OnMouseDown сработал на корабле {shipName}!");
        Debug.Log($"Фаза: {GamePhaseManager.Instance?.currentPhase}");
        Debug.Log($"Воинов на корабле: {currentWarriors}");

        if (GamePhaseManager.Instance == null)
        {
            Debug.LogError("GamePhaseManager.Instance = null!");
            return;
        }

        if (GamePhaseManager.Instance.currentPhase != GamePhaseManager.GamePhase.Combat)
        {
            Debug.Log($"Не фаза сражения! Текущая фаза: {GamePhaseManager.Instance.currentPhase}");
            return;
        }

        if (currentWarriors <= 0)
        {
            Debug.Log($"На корабле {shipName} нет воинов!");
            return;
        }

        CombatUI combatUI = FindObjectOfType<CombatUI>();
        if (combatUI == null)
        {
            Debug.LogError("CombatUI не найден на сцене!");
            return;
        }

        Debug.Log($"Вызываем combatUI.StartAttack для корабля {shipName}");
        combatUI.StartAttack(this);
    }
}