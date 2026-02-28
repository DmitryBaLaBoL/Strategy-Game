using UnityEngine;
using TMPro;

public class GamePhaseManager : MonoBehaviour
{
    public enum GamePhase
    {
        DiceRolling,
        Building,
        Combat
    }

    [Header("Текущая фаза")]
    public GamePhase currentPhase = GamePhase.DiceRolling;

    [Header("Менеджеры")]
    public DiceManager diceManager;
    public SimpleIslandGenerator islandGenerator;
    public GameObject buildingUI; // UI для строительства
    public GameObject combatUI; // UI для сражения

    [Header("UI для фаз")]
    public TextMeshProUGUI phaseText;
    public GameObject phaseTransitionPanel;

    void Start()
    {
        StartDicePhase();
    }

    public void StartDicePhase()
    {
        currentPhase = GamePhase.DiceRolling;
        UpdatePhaseUI("ФАЗА 1: БРОСОК КУБИКОВ");

        // Скрываем UI других фаз
        if (buildingUI != null) buildingUI.SetActive(false);
        if (combatUI != null) combatUI.SetActive(false);

        // Запускаем бросок кубиков
        if (diceManager != null)
        {
            diceManager.StartDicePhase();
        }
    }

    public void StartBuildPhase()
    {
        currentPhase = GamePhase.Building;
        UpdatePhaseUI("ФАЗА 2: СТРОИТЕЛЬСТВО");

        // Показываем UI строительства
        if (buildingUI != null) buildingUI.SetActive(true);
        if (combatUI != null) combatUI.SetActive(false);

        Debug.Log("Начало фазы строительства");
    }

    public void StartCombatPhase()
    {
        currentPhase = GamePhase.Combat;
        UpdatePhaseUI("ФАЗА 3: СРАЖЕНИЕ");

        // Показываем UI сражения
        if (buildingUI != null) buildingUI.SetActive(false);
        if (combatUI != null) combatUI.SetActive(true);

        Debug.Log("Начало фазы сражения");
    }

    void UpdatePhaseUI(string message)
    {
        if (phaseText != null)
            phaseText.text = message;

        // Показываем уведомление о смене фазы
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