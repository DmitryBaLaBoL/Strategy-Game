using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Генератор острова")]
    public SimpleIslandGenerator islandGenerator;

    void Start()
    {
        // Генерируем остров при старте
        if (islandGenerator != null)
        {
            islandGenerator.GenerateIsland();
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

    // Опционально: регенерация острова по кнопке
    public void RegenerateIsland()
    {
        if (islandGenerator != null)
        {
            islandGenerator.GenerateIsland();
        }
    }
}