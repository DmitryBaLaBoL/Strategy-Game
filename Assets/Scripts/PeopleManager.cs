using UnityEngine;
using TMPro;

public class PeopleManager : MonoBehaviour
{
    public static PeopleManager Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI peopleText;

    private int totalPeople = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddPeople(int amount)
    {
        totalPeople += amount;
        UpdateUI();
        Debug.Log($"Добавлено людей: +{amount}. Всего: {totalPeople}");
    }

    public int GetPeopleCount()
    {
        return totalPeople;
    }

    void UpdateUI()
    {
        if (peopleText != null)
            peopleText.text = $"{totalPeople}";
    }
}