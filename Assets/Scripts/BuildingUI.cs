using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : MonoBehaviour
{
    public BuildingSystem buildingSystem;
    public GameObject buttonPrefab;
    public Transform buttonsContainer;

    void Start()
    {
        CreateBuildingButtons();
    }

    void CreateBuildingButtons()
    {
        foreach (BuildingData building in buildingSystem.availableBuildings)
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonsContainer);
            Button button = buttonObj.GetComponent<Button>();

            // Настраиваем иконку
            Image icon = buttonObj.transform.Find("Icon").GetComponent<Image>();
            if (icon != null)
                icon.sprite = building.icon;

            // Настраиваем текст
            Text text = buttonObj.GetComponentInChildren<Text>();
            if (text != null)
                text.text = building.buildingName;

            // Добавляем обработчик
            button.onClick.AddListener(() => buildingSystem.SelectBuilding(building));
        }
    }
}