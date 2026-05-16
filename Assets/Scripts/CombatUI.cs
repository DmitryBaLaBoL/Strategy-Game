using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatUI : MonoBehaviour
{
    [Header("Главная панель")]
    public GameObject combatPanel;

    [Header("Информация о силах")]
    public TextMeshProUGUI myShipsCountText;
    public TextMeshProUGUI myWarriorsCountText;

    [Header("Выбор войнов на корабле")]
    public GameObject warriorSelectorPanel;
    public Slider warriorCountSlider;
    public TextMeshProUGUI warriorCountText;
    public Button confirmWarriorsButton;
    public Button cancelWarriorsButton;

    [Header("Статус")]
    public TextMeshProUGUI statusText;
    public Button cancelButton;

    [Header("Настройки")]
    public int maxWarriorsPerShip = 8;

    private int selectedWarriorCount = 1;

    void Start()
    {
        if (warriorCountSlider != null)
        {
            warriorCountSlider.minValue = 1;
            warriorCountSlider.maxValue = maxWarriorsPerShip;
            warriorCountSlider.value = 1;
            warriorCountSlider.onValueChanged.AddListener(OnWarriorCountChanged);
        }

        if (confirmWarriorsButton != null)
            confirmWarriorsButton.onClick.AddListener(ConfirmWarriors);

        if (cancelWarriorsButton != null)
            cancelWarriorsButton.onClick.AddListener(CancelWarriorSelection);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelCombat);

        if (warriorSelectorPanel != null)
            warriorSelectorPanel.SetActive(false);

        Hide();
    }

    public void Show()
    {
        if (combatPanel != null)
            combatPanel.SetActive(true);
        UpdateCombatInfo();
    }

    public void Hide()
    {
        if (combatPanel != null)
            combatPanel.SetActive(false);
        if (warriorSelectorPanel != null)
            warriorSelectorPanel.SetActive(false);
    }

    public void UpdateCombatInfo()
    {
        if (ShipManager.Instance != null)
        {
            if (myShipsCountText != null)
                myShipsCountText.text = $"{ShipManager.Instance.GetShipsCount()}";
            if (myWarriorsCountText != null)
                myWarriorsCountText.text = $"{ShipManager.Instance.GetTotalWarriors()}";
        }
    }

    public void StartAttack(Ship ship)
    {
        Debug.Log($"CombatUI.StartAttack вызван с кораблем: {(ship == null ? "null" : ship.shipName)}");

        if (ship == null)
        {
            Debug.LogError("StartAttack: Корабль null!");
            return;
        }

        if (ship.currentWarriors <= 0)
        {
            if (statusText != null)
                statusText.text = "На этом корабле нет воинов!";
            Debug.Log($"На корабле {ship.shipName} нет воинов");
            return;
        }

        // Сохраняем корабль в CombatManager
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.SetPendingShip(ship);
            Debug.Log($"Корабль {ship.shipName} сохранен в CombatManager");
        }
        else
        {
            Debug.LogError("CombatManager.Instance = null!");
            return;
        }

        selectedWarriorCount = 1;
        int maxSelectable = Mathf.Min(maxWarriorsPerShip, ship.currentWarriors);

        if (warriorCountSlider != null)
        {
            warriorCountSlider.maxValue = maxSelectable;
            warriorCountSlider.value = 1;
        }

        if (warriorCountText != null)
            warriorCountText.text = "1";

        if (warriorSelectorPanel != null)
            warriorSelectorPanel.SetActive(true);

        if (statusText != null)
            statusText.text = $"Выберите количество воинов (1-{maxSelectable}) для корабля {ship.shipName}";

        Debug.Log($"Панель выбора воинов открыта для корабля {ship.shipName}");
    }

    void OnWarriorCountChanged(float value)
    {
        selectedWarriorCount = Mathf.RoundToInt(value);
        if (warriorCountText != null)
            warriorCountText.text = $"{selectedWarriorCount}";
    }

    void ConfirmWarriors()
    {
        Debug.Log("ConfirmWarriors вызван");

        // Получаем корабль из CombatManager
        Ship attackingShip = CombatManager.Instance?.GetPendingShip();

        Debug.Log($"attackingShip = {(attackingShip == null ? "null" : attackingShip.shipName)}");

        if (attackingShip == null)
        {
            Debug.LogError("ConfirmWarriors: Нет корабля в CombatManager!");
            if (statusText != null)
                statusText.text = "Ошибка: корабль не найден! Выберите корабль заново.";
            warriorSelectorPanel.SetActive(false);
            return;
        }

        // Проверяем, что корабль все еще существует
        if (attackingShip.gameObject == null)
        {
            Debug.LogError("ConfirmWarriors: Корабль уничтожен!");
            if (statusText != null)
                statusText.text = "Ошибка: корабль уничтожен!";
            warriorSelectorPanel.SetActive(false);
            CombatManager.Instance.ClearPendingShip();
            return;
        }

        Debug.Log($"ConfirmWarriors: Корабль {attackingShip.shipName}, воинов: {selectedWarriorCount}");

        // Запускаем атаку
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.StartAttack(attackingShip, selectedWarriorCount);
        }

        // Закрываем панель
        warriorSelectorPanel.SetActive(false);

        if (statusText != null)
            statusText.text = "Выберите здание для атаки";
    }

    void CancelWarriorSelection()
    {
        warriorSelectorPanel.SetActive(false);
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.ClearPendingShip();
        }

        if (statusText != null)
            statusText.text = "Выбор воинов отменен";
    }

    void CancelCombat()
    {
        Hide();
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.ClearPendingShip();
            CombatManager.Instance.CancelCombat();
        }
    }
}