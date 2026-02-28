using System;
using UnityEngine;

public class PlaceObjects : MonoBehaviour
{
    public LayerMask layer;
    public float rotateSpeed = 60f;

    // Новые поля для проверки коллизий
    public LayerMask obstacleLayer; // Слой для препятствий (объекты, которые уже размещены)
    public Vector3 objectSize = Vector3.one; // Размер объекта для проверки коллизий
    private bool canPlace = true;
    private Material originalMaterial;
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isPlaced = false; // Флаг, указывающий, размещен ли объект

    private void Start()
    {
        // Сохраняем оригинальный материал и получаем рендерер
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
            originalColor = objectRenderer.material.color;
        }

        PositionObject();
    }

    private void Update()
    {
        // Если объект уже размещен - не обновляем позицию и цвет
        if (isPlaced)
        {
            return;
        }

        PositionObject();

        // Визуальная индикация - можно ли ставить объект
        if (objectRenderer != null)
        {
            if (canPlace)
            {
                objectRenderer.material.color = Color.green;
            }
            else
            {
                objectRenderer.material.color = Color.red;
            }
        }

        if (Input.GetMouseButtonDown(1) && canPlace)
        {
            PlaceBuilding();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            transform.Rotate(Vector3.up * 90);
        }
    }

    private void PlaceBuilding()
    {
        // Помечаем объект как размещенный
        isPlaced = true;

        // Возвращаем оригинальный материал
        if (objectRenderer != null)
        {
            objectRenderer.material = originalMaterial;
            objectRenderer.material.color = originalColor;
            Debug.LogWarning("Вернули материал");
        }

        // Удаляем скрипт размещения
        Destroy(gameObject.GetComponent<PlaceObjects>());

        Debug.LogWarning("Построено");
    }

    private void PositionObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000f, layer))
        {
            Vector3 a = hit.point;
            int b = 0, c = 0;

            // Округление до ближайшего целого с учетом смещения 0.5
            b = Mathf.RoundToInt(a.x);
            c = Mathf.RoundToInt(a.z);

            a.x = b;
            a.z = c;

            // Временно перемещаем объект для проверки коллизий
            transform.position = a;

            // Проверяем, можно ли разместить объект здесь
            canPlace = CheckPlacement();
        }
    }

    private bool CheckPlacement()
    {
        // Проверяем коллизии с другими объектами
        Collider[] colliders = Physics.OverlapBox(
            transform.position + Vector3.up * (objectSize.y / 2f), // Центр бокса проверки
            objectSize / 2f, // Половина размера объекта
            transform.rotation,
            obstacleLayer
        );

        // Исключаем собственный коллайдер из проверки
        Collider ownCollider = GetComponent<Collider>();
        foreach (Collider col in colliders)
        {
            if (col != ownCollider)
            {
                return false; // Есть перекрытие с другим объектом
            }
        }

        return true; // Место свободно
    }

    // Визуализация зоны проверки в редакторе (для отладки)
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = canPlace ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.up * (objectSize.y / 2f), objectSize);
    }
}