using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }

    [Header("Настройки")]
    public float warriorSpeed = 5f;

    [Header("Префабы")]
    public GameObject warriorPrefab;

    private Ship attackingShip;
    private int attackingWarriors;
    private Building selectedTarget;
    private Vector3 landingPoint;
    private List<GameObject> spawnedWarriors = new List<GameObject>();
    private bool isSelectingTarget = false;
    private bool isSelectingLandingPoint = false;   
    private Ship pendingShip;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // Режим выбора здания
        if (isSelectingTarget && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                Building building = hit.collider.GetComponent<Building>();
                if (building != null && !building.isMainBuilding)
                {
                    selectedTarget = building;
                    isSelectingTarget = false;
                    // Точку высадки выбираем автоматически рядом с зданием
                    landingPoint = selectedTarget.transform.position + new Vector3(2f, 0, 2f);
                    landingPoint.y = 0.1f;

                    Debug.Log($"Выбрано здание: {building.name}. Точка высадки автоматически: {landingPoint}");

                    // Запускаем атаку сразу
                    StartCoroutine(ExecuteAttack());
                }
                else if (building != null && building.isMainBuilding)
                {
                    Debug.Log("Нельзя атаковать главное здание!");
                    CombatUI combatUI = FindObjectOfType<CombatUI>();
                    if (combatUI != null && combatUI.statusText != null)
                    {
                        combatUI.statusText.text = "Нельзя атаковать главное здание! Выберите другое.";
                    }
                }
                else
                {
                    Debug.Log("Нужно выбрать здание!");
                }
            }
        }
    }

    public void SetSelectedShip(Ship ship)
    {
        pendingShip = ship;
        Debug.Log($"SetSelectedShip: Корабль {ship?.shipName} сохранен");
    }

    public Ship GetSelectedShip()
    {
        return pendingShip;
    }

    public void ClearSelectedShip()
    {
        pendingShip = null;
        Debug.Log("ClearSelectedShip: Очищен");
    }

    // Добавь эти методы:
    public void SetPendingShip(Ship ship)
    {
        pendingShip = ship;
        Debug.Log($"SetPendingShip: Корабль {ship?.shipName} сохранен в CombatManager");
    }

    public Ship GetPendingShip()
    {
        return pendingShip;
    }

    public void ClearPendingShip()
    {
        pendingShip = null;
        Debug.Log("ClearPendingShip: Корабль очищен");
    }

    public void StartAttack(Ship ship, int warriorCount)
    {
        if (ship == null)
        {
            Debug.LogError("StartAttack: Корабль = null!");
            return;
        }

        if (ship.gameObject == null)
        {
            Debug.LogError("StartAttack: Корабль уничтожен!");
            return;
        }

        attackingShip = ship;
        attackingWarriors = warriorCount;
        isSelectingTarget = true;

        Debug.Log($"StartAttack: УСПЕШНО! Корабль {ship.shipName}, воинов: {warriorCount}");

        CombatUI combatUI = FindObjectOfType<CombatUI>();
        if (combatUI != null && combatUI.statusText != null)
        {
            combatUI.statusText.text = $"Кликните на здание для атаки ({warriorCount} воинов)";
        }
    }

    IEnumerator ExecuteAttack()
    {
        // Проверяем, что корабль еще существует
        if (attackingShip == null)
        {
            Debug.LogError("Корабль атаки уничтожен или не существует!");
            EndCombat();
            yield break;
        }

        // Проверяем, что цель существует
        if (selectedTarget == null)
        {
            Debug.LogError("Цель атаки не существует!");
            EndCombat();
            yield break;
        }

        Debug.Log($"Начинаем атаку на {selectedTarget.name} с {attackingWarriors} воинами с корабля {attackingShip.shipName}");

        CombatUI combatUI = FindObjectOfType<CombatUI>();
        if (combatUI != null && combatUI.statusText != null)
        {
            combatUI.statusText.text = "Войска высаживаются...";
        }

        // Снимаем воинов с корабля
        attackingShip.RemoveWarriors(attackingWarriors);

        // Обновляем UI после снятия воинов
        if (combatUI != null)
        {
            combatUI.UpdateCombatInfo();
        }

        // Создаем воинов
        SpawnWarriors();

        // Двигаем воинов к зданию
        yield return StartCoroutine(MoveWarriorsToBuilding());

        // Атакуем здание
        AttackBuilding();

        // Завершаем
        EndCombat();
    }

    void SpawnWarriors()
    {
        // Очищаем старых
        foreach (var warrior in spawnedWarriors)
        {
            if (warrior != null) Destroy(warrior);
        }
        spawnedWarriors.Clear();

        // Проверяем, что префаб воина назначен
        if (warriorPrefab == null)
        {
            Debug.LogError("Warrior Prefab не назначен в CombatManager!");
            return;
        }

        // Создаем новых
        for (int i = 0; i < attackingWarriors; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
            GameObject warrior = Instantiate(warriorPrefab, landingPoint + offset, Quaternion.identity);
            spawnedWarriors.Add(warrior);
        }
        Debug.Log($"Создано {attackingWarriors} воинов");
    }

    IEnumerator MoveWarriorsToBuilding()
    {
        if (selectedTarget == null) yield break;

        Vector3 targetPos = selectedTarget.transform.position;
        float stopDistance = 1.2f;
        float timeout = 15f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            bool allReached = true;

            foreach (var warrior in spawnedWarriors)
            {
                if (warrior == null) continue;

                if (Vector3.Distance(warrior.transform.position, targetPos) > stopDistance)
                {
                    allReached = false;
                    warrior.transform.position = Vector3.MoveTowards(
                        warrior.transform.position,
                        targetPos,
                        warriorSpeed * Time.deltaTime
                    );
                }
            }

            if (allReached) break;
            yield return null;
        }

        Debug.Log("Воины достигли здания");
    }

    void AttackBuilding()
    {
        if (selectedTarget == null) return;

        Debug.Log($"Атакуем здание {selectedTarget.name}");

        // Уничтожаем здание
        Destroy(selectedTarget.gameObject);

        // Уничтожаем воинов
        foreach (var warrior in spawnedWarriors)
        {
            if (warrior != null) Destroy(warrior);
        }
        spawnedWarriors.Clear();

        Debug.Log("Здание уничтожено!");
    }

    void EndCombat()
    {
        isSelectingTarget = false;
        isSelectingLandingPoint = false;

        // Очищаем воинов на всякий случай
        foreach (var warrior in spawnedWarriors)
        {
            if (warrior != null) Destroy(warrior);
        }
        spawnedWarriors.Clear();

        CombatUI combatUI = FindObjectOfType<CombatUI>();
        if (combatUI != null)
        {
            if (combatUI.statusText != null)
                combatUI.statusText.text = "Атака завершена!";

            combatUI.Hide();
        }

        // Очищаем ссылки
        attackingShip = null;
        selectedTarget = null;

        Debug.Log("Сражение завершено");
    }

    public void CancelCombat()
    {
        isSelectingTarget = false;
        isSelectingLandingPoint = false;

        foreach (var warrior in spawnedWarriors)
        {
            if (warrior != null) Destroy(warrior);
        }
        spawnedWarriors.Clear();

        Debug.Log("Атака отменена");

        CombatUI combatUI = FindObjectOfType<CombatUI>();
        if (combatUI != null)
        {
            combatUI.Hide();
        }

        attackingShip = null;
        selectedTarget = null;
    }
}