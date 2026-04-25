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
    public int numberOfDice = 5;
    public GameObject dicePrefab;
    public Transform diceSpawnPoint;
    public Transform diceTable;
    public GameObject diceArena;
    public float throwForce = 8f;
    public float throwUpwardForce = 6f;
    public float spreadForce = 2f;

    [Header("UI элементы")]
    public GameObject dicePanel;
    public Button rollButton;
    public Button continueButton;
    public GameObject resourcePanel;

    [Header("Тексты ресурсов")]
    public TextMeshProUGUI woodCurrentText;
    public TextMeshProUGUI stoneCurrentText;

    // Текущие ресурсы
    private int currentWood = 0;
    private int currentStone = 0;

    private List<GameObject> currentDiceList = new List<GameObject>();
    private int diceResultsReceived = 0;
    private bool isRolling = false;
    // Флаг на то, что бросок уже был сделан
    private bool hasThrown = false; 

    void Start()
    {
        if (rollButton != null)
            rollButton.onClick.AddListener(OnRollButtonClicked);

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueToNextPhase);

        if (dicePanel != null)
            dicePanel.SetActive(false);

        if (diceArena != null)
            diceArena.SetActive(false);

        // Изначально кнопка Continue неактивна
        if (continueButton != null)
            continueButton.interactable = false;
    }

    public void StartDicePhase()
    {
        Debug.Log("Фаза броска кубиков");
        currentWood = 0;
        currentStone = 0;
        diceResultsReceived = 0;
        hasThrown = false;

        if (dicePanel != null)
            dicePanel.SetActive(true);

        if (resourcePanel != null)
            resourcePanel.SetActive(true);

        ShowArena(true);
        UpdateResourceDisplay();
        CreateNewDiceSet();

        if (rollButton != null)
            rollButton.interactable = true;

        if (continueButton != null)
            continueButton.interactable = false;
    }

    void ShowArena(bool show)
    {
        if (diceArena != null)
            diceArena.SetActive(show);
    }

    void CreateNewDiceSet()
    {
        // Удаляем старые кубики
        DestroyAllDice();

        // Создаем новые кубики
        for (int i = 0; i < numberOfDice; i++)
        {
            if (dicePrefab != null)
            {
                Vector3 spawnPos = diceSpawnPoint != null ?
                    diceSpawnPoint.position + new Vector3(i * 0.5f, 0, 0) :
                    new Vector3(i * 0.5f, 3f, 0);

                GameObject dice = Instantiate(dicePrefab, spawnPos, Quaternion.identity);

                Dice diceScript = dice.GetComponent<Dice>();
                if (diceScript != null)
                {
                    diceScript.OnDiceLanded += OnDiceResult;
                }
                else
                {
                    Debug.LogError($"У префаба {dicePrefab.name} нет компонента Dice!");
                }

                currentDiceList.Add(dice);
            }
        }

        Debug.Log($"Создано {currentDiceList.Count} кубиков");
    }

    void DestroyAllDice()
    {
        foreach (var dice in currentDiceList)
        {
            if (dice != null)
                Destroy(dice);
        }
        currentDiceList.Clear();
    }

    void OnDiceResult(ResourceType resourceType, int amount, GameObject diceObject)
    {
        Debug.Log($"Выпал кубик: {resourceType} +{amount}");

        // Добавляем ресурсы через ResourcePanelManager
        if (ResourcePanelManager.Instance != null)
        {
            ResourcePanelManager.Instance.AddResource(resourceType, amount);
        }

        diceResultsReceived++;

        if (diceResultsReceived >= numberOfDice)
        {
            Debug.Log("Все кубики дали результат!");
            EnableContinueButton();
        }
    }

    void UpdateResourceDisplay()
    {
        if (woodCurrentText != null)
            woodCurrentText.text = $"{currentWood}";

        if (stoneCurrentText != null)
            stoneCurrentText.text = $"{currentStone}";
    }

    public void OnRollButtonClicked()
    {
        if (isRolling)
        {
            Debug.Log("Бросок уже выполняется!");
            return;
        }

        if (hasThrown)
        {
            Debug.Log("Бросок уже был сделан! Нажмите Продолжить.");
            return;
        }

        StartCoroutine(PerformThrow());
    }

    IEnumerator PerformThrow()
    {
        isRolling = true;
        hasThrown = true;

        if (rollButton != null)
            rollButton.interactable = false;

        // Сбрасываем счетчик результатов
        diceResultsReceived = 0;

        yield return new WaitForSeconds(0.2f);

        // Бросаем кубики
        foreach (var diceObj in currentDiceList)
        {
            if (diceObj == null) continue;

            Rigidbody rb = diceObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 randomDir = new Vector3(
                    Random.Range(-spreadForce, spreadForce),
                    Random.Range(2f, throwUpwardForce),
                    Random.Range(1f, throwForce)
                );

                Dice diceScript = diceObj.GetComponent<Dice>();
                if (diceScript != null)
                {
                    diceScript.ThrowDice(randomDir);
                }
            }
        }

        Debug.Log($"Бросок выполнен, ожидаем результаты...");
        isRolling = false;

        // Таймаут на случай проблем
        StartCoroutine(TimeoutCheck());
    }

    IEnumerator TimeoutCheck()
    {
        float timeout = 8f;
        float elapsed = 0f;

        while (diceResultsReceived < numberOfDice && elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return new WaitForSeconds(0.5f);
        }

        if (diceResultsReceived < numberOfDice)
        {
            Debug.LogWarning($"Получено только {diceResultsReceived}/{numberOfDice} результатов. Принудительная активация Continue.");
            EnableContinueButton();
        }
    }

    void EnableContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.interactable = true;
        }
    }

    public void ContinueToNextPhase()
    {
        Debug.Log($"Переход к стройке с ресурсами: Дерево={ResourcePanelManager.Instance.GetWood()}, Камень={ResourcePanelManager.Instance.GetStone()}");

        if (islandGenerator != null)
        {
            int currentWood = ResourcePanelManager.Instance.GetWood();
            int currentStones = ResourcePanelManager.Instance.GetStone();

            islandGenerator.SetResourceCounts(currentWood, currentStones);
        }

        DestroyAllDice();
        ShowArena(false);

        if (dicePanel != null)
            dicePanel.SetActive(false);

        if (phaseManager != null)
        {
            phaseManager.StartBuildPhase();
        }
    }
}