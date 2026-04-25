using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Генератор острова")]
    public SimpleIslandGenerator islandGenerator;

    [Header("Менеджеры фаз")] 
    public GamePhaseManager phaseManager;
    public DiceManager diceManager;

    [Header("Настройки игры")]
    public bool startWithDicePhase = true; // Начинать ли с броска кубиков

    void Start()
    {
        // Генерируем остров при старте
        if (islandGenerator != null)
        {
            islandGenerator.GenerateIsland();
        }

        // Запускаем первую фазу
        if (startWithDicePhase && phaseManager != null)
        {
            // Небольшая задержка, чтобы остров точно сгенерировался
            Invoke("StartFirstPhase", 0.5f);
        }
    }

    void StartFirstPhase()
    {
        if (phaseManager != null)
        {
            phaseManager.StartDicePhase();
        }
    }

    void Update()
    {
        // Возврат в меню по нажатию Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMenu();
        }
    }

    public void ReturnToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }


    public void RegenerateIsland()
    {
        if (islandGenerator != null)
        {
            islandGenerator.GenerateIsland();
        }
    }

    // Переход к следующей фазе
    public void NextPhase()
    {
        if (phaseManager != null)
        {
            switch (phaseManager.currentPhase)
            {
                case GamePhaseManager.GamePhase.DiceRolling:
                    // Завершаем фазу кубиков и переходим к строительству
                    if (diceManager != null)
                    {
                        diceManager.ContinueToNextPhase();
                    }
                    break;

                case GamePhaseManager.GamePhase.Building:
                    phaseManager.StartCombatPhase();
                    break;

                case GamePhaseManager.GamePhase.Combat:
                    Debug.Log("Игра завершена!");
                    // Можно вернуться в меню или начать заново
                    break;
            }
        }
    }
}