using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ResourceUI : MonoBehaviour
{
    [System.Serializable]
    public class ResourceDisplay
    {
        public string resourceName;
        public Text amountText;
        public Image iconImage;
    }

    public List<ResourceDisplay> resourceDisplays = new List<ResourceDisplay>();

    void Update()
    {
        UpdateResourceDisplay();
    }

    void UpdateResourceDisplay()
    {
        foreach (var display in resourceDisplays)
        {
            if (display.amountText != null)
            {
                int amount = ResourceManager.Instance.GetResourceAmount(display.resourceName);
                display.amountText.text = amount.ToString();
            }
        }
    }
}