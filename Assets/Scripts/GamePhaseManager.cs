using UnityEngine;
using TMPro;

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager Instance { get; private set; }  // Добавляем синглтон

    public enum GamePhase
    {
        StartBuilding,  // Новая фаза - начальное строительство
        DiceRolling,
        Building,
        Combat
    }

    [Header("Текущая фаза")]
    public GamePhase currentPhase = GamePhase.StartBuilding;

    [Header("Менеджеры")]
    public DiceManager diceManager;
    public SimpleIslandGenerator islandGenerator;
    public GameObject buildingUI;
    public GameObject combatUI;
    public GameObject resourcePanel;
    public StartBuildingManager startBuildingManager; // Новый менеджер

    [Header("UI для фаз")]
    public TextMeshProUGUI phaseText;
    public GameObject phaseTransitionPanel;

    void Start()
    {
        StartStartBuildingPhase();
    }

    public void StartStartBuildingPhase()
    {
        currentPhase = GamePhase.StartBuilding;
        UpdatePhaseUI("ПОСТРОЙТЕ ОБЯЗАТЕЛЬНЫЕ ЗДАНИЯ");

        // Скрываем все UI строительства
        if (buildingUI != null) buildingUI.SetActive(false);
        if (combatUI != null) combatUI.SetActive(false);

        // Панель ресурсов скрываем в этой фазе (ресурсов еще нет)
        if (resourcePanel != null) resourcePanel.SetActive(false);

        if (startBuildingManager != null)
        {
            startBuildingManager.enabled = true;
        }
    }

    public void StartDicePhase()
    {
        currentPhase = GamePhase.DiceRolling;
        UpdatePhaseUI("ФАЗА 1: БРОСОК КУБИКОВ");
        UpdateUIForPhase();

        if (diceManager != null)
        {
            diceManager.StartDicePhase();
        }
    }

    public void StartBuildPhase()
    {
        currentPhase = GamePhase.Building;
        UpdatePhaseUI("ФАЗА 2: СТРОИТЕЛЬСТВО");
        UpdateUIForPhase();

        Debug.Log("Начало фазы строительства");
    }

    public void StartCombatPhase()
    {
        currentPhase = GamePhase.Combat;
        UpdatePhaseUI("ФАЗА 3: СРАЖЕНИЕ");

        if (buildingUI != null)
            buildingUI.SetActive(false);

        if (combatUI != null)
            combatUI.SetActive(true);

        if (resourcePanel != null)
            resourcePanel.SetActive(true);

        //if (startCombatButton != null)
          //  startCombatButton.gameObject.SetActive(false);

        Debug.Log("НАЧАЛО ФАЗЫ СРАЖЕНИЯ");
        Debug.Log("Теперь можно кликать на корабли для атаки");
    }

    void UpdateUIForPhase()
    {
        // Скрываем все UI
        if (buildingUI != null) buildingUI.SetActive(false);
        if (combatUI != null) combatUI.SetActive(false);

        // Показываем нужный UI
        if (currentPhase == GamePhase.Building && buildingUI != null)
            buildingUI.SetActive(true);
        else if (currentPhase == GamePhase.Combat && combatUI != null)
            combatUI.SetActive(true);

        // Панель ресурсов всегда видна
        if (resourcePanel != null)
            resourcePanel.SetActive(true);
    }

    void UpdatePhaseUI(string message)
    {
        if (phaseText != null)
            phaseText.text = message;

        if (phaseTransitionPanel != null)
        {
            phaseTransitionPanel.SetActive(true);
            Invoke("HideTransitionPanel", 2f);
        }
    }

    void HideTransitionPanel()
    {
        if (phaseTransitionPanel != null)
            phaseTransitionPanel.SetActive(false);
    }
}