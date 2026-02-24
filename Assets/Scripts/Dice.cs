using System.Collections;
using System.Resources;
using UnityEngine;

public class Dice : MonoBehaviour
{
    [System.Serializable]
    public class DiceFace
    {
        public string resourceName;  // Название ресурса
        public Sprite icon;          // Иконка ресурса
        public int amount;            // Количество ресурса
        public Color faceColor = Color.white; // Цвет грани
    }

    [Header("Настройки кубика")]
    public DiceFace[] faces = new DiceFace[6]; // 6 граней кубика
    public float throwForce = 10f;     // Сила броска
    public float torqueForce = 10f;     // Сила вращения
    public float destroyTime = 5f;      // Время до уничтожения кубика

    [Header("Компоненты")]
    public Rigidbody rb;
    public MeshRenderer meshRenderer;

    private bool hasLanded = false;
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    // Метод для броска кубика
    public void ThrowDice(Vector3 throwDirection)
    {
        hasLanded = false;
        transform.position = startPosition;
        transform.rotation = startRotation;

        // Добавляем силу броска
        rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);

        // Добавляем случайное вращение
        Vector3 randomTorque = new Vector3(
            Random.Range(-torqueForce, torqueForce),
            Random.Range(-torqueForce, torqueForce),
            Random.Range(-torqueForce, torqueForce)
        );
        rb.AddTorque(randomTorque, ForceMode.Impulse);

        // Запускаем проверку результата через некоторое время
        StartCoroutine(CheckDiceResult());
    }

    IEnumerator CheckDiceResult()
    {
        // Ждем пока кубик успокоится
        yield return new WaitForSeconds(1.5f);

        // Ждем пока кубик перестанет двигаться
        while (rb.linearVelocity.magnitude > 0.1f || rb.angularVelocity.magnitude > 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // Определяем верхнюю грань
        int upperFaceIndex = GetUpperFace();

        // Получаем ресурс с верхней грани
        DiceFace result = faces[upperFaceIndex];

        // Добавляем ресурс игроку
        ResourceManager.Instance.AddResource(result.resourceName, result.amount);

        // Показываем результат (опционально)
        Debug.Log($"Выпало: {result.resourceName} +{result.amount}");

        hasLanded = true;

        // Уничтожаем кубик через некоторое время
        Destroy(gameObject, destroyTime);
    }

    int GetUpperFace()
    {
        float maxDot = -Mathf.Infinity;
        int upperFace = 0;

        // Направления граней кубика (в локальных координатах)
        Vector3[] faceDirections = {
            transform.up,      // Грань 0 (верх)
            -transform.up,     // Грань 1 (низ)
            transform.right,   // Грань 2 (право)
            -transform.right,  // Грань 3 (лево)
            transform.forward, // Грань 4 (перед)
            -transform.forward // Грань 5 (зад)
        };

        for (int i = 0; i < faceDirections.Length; i++)
        {
            float dot = Vector3.Dot(faceDirections[i], Vector3.up);
            if (dot > maxDot)
            {
                maxDot = dot;
                upperFace = i;
            }
        }

        return upperFace;
    }
}