using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class DiceManager : MonoBehaviour
{
    [Header("Ссылки")]
    public SimpleIslandGenerator islandGenerator;
    public GamePhaseManager phaseManager;

    [Header("Настройки кубиков")]
    public int numberOfDice = 5; // Количество кубиков
    public List<DiceFaceData> diceFaces = new List<DiceFaceData>(); // Данные граней

    [Header("UI элементы")]
    public GameObject dicePanel; // Панель с кубиками
    public GameObject dicePrefab; // Префаб кубика
    public Transform diceContainer; // Контейнер для кубиков
    public Button rollButton; // Кнопка броска
    public Button continueButton; // Кнопка продолжения

    [Header("Тексты результатов")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI totalWoodText;
    public TextMeshProUGUI totalStoneText;
    public TextMeshProUGUI rollCountText; // Текст для отображения попыток

    [Header("Анимация")]
    public float rollAnimationDuration = 2f;
    public float diceSpacing = 1.5f;

    [Header("Настройки фаз")]
    public int maxRolls = 3; // Максимальное количество бросков в фазе
    public int currentRollCount = 0;

    // Текущие результаты
    private Dictionary<SimpleIslandGenerator.ResourceType, int> currentResources =
        new Dictionary<SimpleIslandGenerator.ResourceType, int>();

    private List<GameObject> spawnedDice = new List<GameObject>();
    private List<int> currentDiceResults = new List<int>();
    private bool isRolling = false;

    [System.Serializable]
    public class DiceFaceData
    {
        public SimpleIslandGenerator.ResourceType resourceType;
        public Sprite faceIcon;
        public Color faceColor = Color.white;
        public int minAmount = 1;
        public int maxAmount = 3;
    }

    void Start()
    {
        // Инициализируем словарь ресурсов
        ResetResources();

        // Подписываемся на события кнопок
        if (rollButton != null)
            rollButton.onClick.AddListener(StartRollingDice);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueToNextPhase);

        // Скрываем панель сначала
        if (dicePanel != null)
            dicePanel.SetActive(false);

        UpdateRollCountText();
    }

    public void StartDicePhase()
    {
        currentRollCount = 0;
        ResetResources();

        if (dicePanel != null)
            dicePanel.SetActive(true);

        // Создаем кубики
        SpawnDice();

        // Активируем кнопку броска
        if (rollButton != null)
            rollButton.interactable = true;

        UpdateRollCountText();
    }

    void SpawnDice()
    {
        // Очищаем старые кубики
        foreach (var dice in spawnedDice)
        {
            Destroy(dice);
        }
        spawnedDice.Clear();

        // Создаем новые кубики
        for (int i = 0; i < numberOfDice; i++)
        {
            if (dicePrefab != null && diceContainer != null)
            {
                Vector3 position = diceContainer.position + new Vector3(i * diceSpacing, 0, 0);
                GameObject dice = Instantiate(dicePrefab, position, Quaternion.identity, diceContainer);

                // Настраиваем визуал кубика
                DiceVisual diceVisual = dice.GetComponent<DiceVisual>();
                if (diceVisual != null)
                {
                    diceVisual.SetDiceFaces(diceFaces);
                }

                spawnedDice.Add(dice);
            }
        }
    }

    void StartRollingDice()
    {
        if (isRolling) return;

        if (currentRollCount >= maxRolls)
        {
            // Если достигли лимита бросков, показываем сообщение
            if (resultText != null)
                resultText.text = "Достигнут лимит бросков!";
            return;
        }

        currentRollCount++;
        StartCoroutine(RollDiceAnimation());
        UpdateRollCountText();
    }

    IEnumerator RollDiceAnimation()
    {
        isRolling = true;
        rollButton.interactable = false;

        float elapsedTime = 0f;
        currentDiceResults.Clear();

        // Анимация вращения кубиков
        while (elapsedTime < rollAnimationDuration)
        {
            foreach (var dice in spawnedDice)
            {
                dice.transform.Rotate(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360), Space.Self);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Получаем результаты
        GetDiceResults();

        isRolling = false;

        // Если не достигли лимита, активируем кнопку снова
        if (currentRollCount < maxRolls)
        {
            rollButton.interactable = true;
        }
    }

    void GetDiceResults()
    {
        // Сбрасываем ресурсы текущего броска
        Dictionary<SimpleIslandGenerator.ResourceType, int> rollResources =
            new Dictionary<SimpleIslandGenerator.ResourceType, int>();
        rollResources[SimpleIslandGenerator.ResourceType.Tree] = 0;
        rollResources[SimpleIslandGenerator.ResourceType.Stone] = 0;

        // Бросаем каждый кубик
        for (int i = 0; i < numberOfDice; i++)
        {
            // Выбираем случайную грань
            int faceIndex = Random.Range(0, diceFaces.Count);
            DiceFaceData rolledFace = diceFaces[faceIndex];

            // Получаем случайное количество ресурса
            int amount = Random.Range(rolledFace.minAmount, rolledFace.maxAmount + 1);

            // Добавляем к результатам броска
            rollResources[rolledFace.resourceType] += amount;

            // Сохраняем индекс грани для визуала
            currentDiceResults.Add(faceIndex);

            Debug.Log($"Кубик {i + 1}: {rolledFace.resourceType} +{amount}");
        }

        // Добавляем ресурсы от этого броска к общему счету
        currentResources[SimpleIslandGenerator.ResourceType.Tree] += rollResources[SimpleIslandGenerator.ResourceType.Tree];
        currentResources[SimpleIslandGenerator.ResourceType.Stone] += rollResources[SimpleIslandGenerator.ResourceType.Stone];

        // Обновляем UI
        UpdateResourceTexts();
        UpdateDiceVisuals();

        // Показываем результаты броска
        if (resultText != null)
        {
            resultText.text = $"Бросок {currentRollCount} из {maxRolls}\n" +
                             $"Получено:\n" +
                             $"Дерево: +{rollResources[SimpleIslandGenerator.ResourceType.Tree]}\n" +
                             $"Камень: +{rollResources[SimpleIslandGenerator.ResourceType.Stone]}";
        }
    }

    void UpdateDiceVisuals()
    {
        for (int i = 0; i < spawnedDice.Count && i < currentDiceResults.Count; i++)
        {
            DiceVisual diceVisual = spawnedDice[i].GetComponent<DiceVisual>();
            if (diceVisual != null)
            {
                diceVisual.ShowFace(currentDiceResults[i]);
            }
        }
    }

    void UpdateResourceTexts()
    {
        if (totalWoodText != null)
        {
            totalWoodText.text = $"Всего дерева: {currentResources[SimpleIslandGenerator.ResourceType.Tree]}";
        }

        if (totalStoneText != null)
        {
            totalStoneText.text = $"Всего камня: {currentResources[SimpleIslandGenerator.ResourceType.Stone]}";
        }
    }

    void UpdateRollCountText()
    {
        if (rollCountText != null)
        {
            rollCountText.text = $"Бросков: {currentRollCount}/{maxRolls}";
        }
    }

    void ResetResources()
    {
        currentResources.Clear();
        currentResources[SimpleIslandGenerator.ResourceType.Tree] = 0;
        currentResources[SimpleIslandGenerator.ResourceType.Stone] = 0;
        UpdateResourceTexts();
    }

    public void ContinueToNextPhase()
    {
        // Добавляем ресурсы в генератор острова
        if (islandGenerator != null)
        {
            // Получаем текущие ресурсы
            int currentTrees = islandGenerator.treeCount;
            int currentStones = islandGenerator.stoneCount;

            // Добавляем полученные ресурсы
            islandGenerator.SetResourceCounts(
                currentTrees + currentResources[SimpleIslandGenerator.ResourceType.Tree],
                currentStones + currentResources[SimpleIslandGenerator.ResourceType.Stone]
            );

            Debug.Log($"Добавлены ресурсы: Дерево +{currentResources[SimpleIslandGenerator.ResourceType.Tree]}, " +
                     $"Камень +{currentResources[SimpleIslandGenerator.ResourceType.Stone]}");
        }

        // Скрываем панель кубиков
        if (dicePanel != null)
            dicePanel.SetActive(false);

        // Переходим к фазе строительства
        if (phaseManager != null)
        {
            phaseManager.StartBuildPhase();
        }
    }
}