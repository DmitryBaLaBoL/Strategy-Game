using UnityEngine;
using System.Collections.Generic;

public class DiceVisual : MonoBehaviour
{
    [Header("Грани кубика")]
    public MeshRenderer[] faceRenderers; // 6 граней кубика
    public Material defaultMaterial;

    private List<DiceManager.DiceFaceData> diceFaces;
    private Material[] faceMaterials;

    void Start()
    {
        // Создаем массив материалов для граней
        faceMaterials = new Material[faceRenderers.Length];
        for (int i = 0; i < faceRenderers.Length; i++)
        {
            if (faceRenderers[i] != null)
            {
                faceMaterials[i] = faceRenderers[i].material;
            }
        }
    }

    public void SetDiceFaces(List<DiceManager.DiceFaceData> faces)
    {
        diceFaces = faces;

        // Здесь можно настроить внешний вид граней в зависимости от данных
        // Например, установить иконки или цвета
    }

    public void ShowFace(int faceIndex)
    {
        // Поворачиваем кубик нужной гранью вверх
        // Это пример - нужно настроить под твою модель кубика
        switch (faceIndex)
        {
            case 0: // Грань 1
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case 1: // Грань 2
                transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            case 2: // Грань 3
                transform.rotation = Quaternion.Euler(90, 0, 0);
                break;
            case 3: // Грань 4
                transform.rotation = Quaternion.Euler(-90, 0, 0);
                break;
            case 4: // Грань 5
                transform.rotation = Quaternion.Euler(0, -90, 0);
                break;
            case 5: // Грань 6
                transform.rotation = Quaternion.Euler(180, 0, 0);
                break;
        }

        // Если есть данные о грани, меняем материал
        if (diceFaces != null && faceIndex < diceFaces.Count && faceIndex < faceRenderers.Length)
        {
            // Здесь можно изменить цвет или текстуру грани
            // Например: faceRenderers[faceIndex].material.color = diceFaces[faceIndex].faceColor;
        }
    }

    public void RandomizeRotation()
    {
        transform.rotation = Random.rotation;
    }
}