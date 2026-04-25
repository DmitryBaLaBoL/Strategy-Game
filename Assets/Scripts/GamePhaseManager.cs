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
    public GameObject buildingUI;
    public GameObject combatUI;
    public GameObject resourcePanel; // Ссылка на ResourcePanel

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
        UpdatePhaseUI("Фаза 1: Бросок кубиков");

        if (buildingUI != null) buildingUI.SetActive(false);
        if (combatUI != null) combatUI.SetActive(false);

        // Показываем панель ресурсов
        if (resourcePanel != null) resourcePanel.SetActive(true);

        if (diceManager != null)
        {
            diceManager.StartDicePhase();
        }
    }

    public void StartBuildPhase()
    {
        currentPhase = GamePhase.Building;
        UpdatePhaseUI("Фаза 2: Строительство");

        if (buildingUI != null) buildingUI.SetActive(true);
        if (combatUI != null) combatUI.SetActive(false);

        // Панель ресурсов должна оставаться видимой
        if (resourcePanel != null) resourcePanel.SetActive(true);

        Debug.Log("Начало фазы строительства");
    }

    public void StartCombatPhase()
    {
        currentPhase = GamePhase.Combat;
        UpdatePhaseUI("Фаза 3: Сражение");

        if (buildingUI != null) buildingUI.SetActive(false);
        if (combatUI != null) combatUI.SetActive(true);

        // Панель ресурсов видна и в фазе сражения (если нужна)
        if (resourcePanel != null) resourcePanel.SetActive(true);

        Debug.Log("Начало фазы сражения");
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