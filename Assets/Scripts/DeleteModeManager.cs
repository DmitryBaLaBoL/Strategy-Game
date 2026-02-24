using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeleteModeManager : MonoBehaviour
{
    [Header("UI элементы")]
    public Button deleteModeButton;
    public TextMeshProUGUI buttonText;
    public GameObject deleteModePanel; // Панель с подтверждением или информацией

    [Header("Настройки")]
    public string activateText = "Включить режим удаления";
    public string deactivateText = "Выключить режим удаления";
    public Color normalModeColor = Color.white;
    public Color deleteModeColor = Color.red;

    [Header("Визуальные эффекты")]
    public GameObject deleteModeIndicator; // Например, красная рамка на весь экран или иконка

    private bool isDeleteModeActive = false;

    // Событие для оповещения других скриптов
    public System.Action<bool> OnDeleteModeChanged;

    void Start()
    {
        deleteModeButton.onClick.AddListener(ToggleDeleteMode);
        UpdateButtonUI();

        if (deleteModeIndicator != null)
            deleteModeIndicator.SetActive(false);

        if (deleteModePanel != null)
            deleteModePanel.SetActive(false);
    }

    public void ToggleDeleteMode()
    {
        isDeleteModeActive = !isDeleteModeActive;

        // Визуальные изменения
        UpdateButtonUI();

        if (deleteModeIndicator != null)
            deleteModeIndicator.SetActive(isDeleteModeActive);

        if (deleteModePanel != null)
            deleteModePanel.SetActive(isDeleteModeActive);

        // Оповещаем другие скрипты
        OnDeleteModeChanged?.Invoke(isDeleteModeActive);

        Debug.Log(isDeleteModeActive ? "Режим удаления ВКЛЮЧЕН" : "Режим удаления ВЫКЛЮЧЕН");
    }

    void UpdateButtonUI()
    {
        if (buttonText != null)
        {
            buttonText.text = isDeleteModeActive ? deactivateText : activateText;
        }

        // Меняем цвет кнопки
        ColorBlock colors = deleteModeButton.colors;
        colors.normalColor = isDeleteModeActive ? deleteModeColor : normalModeColor;
        deleteModeButton.colors = colors;
    }

    public bool IsDeleteModeActive()
    {
        return isDeleteModeActive;
    }
}