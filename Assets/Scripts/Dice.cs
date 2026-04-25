using System.Collections;
using UnityEngine;

public class Dice : MonoBehaviour
{
    [System.Serializable]
    public class DiceFace
    {
        public string faceName;
        // Тип ресурса
        public ResourceType resourceType;
        // Количество ресурса
        public int amount;                  
        public Color faceColor = Color.white;
    }

    [Header("Настройки граней")]
    public DiceFace[] faces = new DiceFace[6];

    public System.Action<ResourceType, int, GameObject> OnDiceLanded;

    private Rigidbody rb;
    private bool hasReported = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        rb.mass = 1f;
        rb.linearDamping = 0.3f;
        rb.angularDamping = 0.3f;
    }

    void Start()
    {
        StartCoroutine(WaitAndDetermineResult());
    }

    IEnumerator WaitAndDetermineResult()
    {
        float waitTime = 0f;
        float maxWaitTime = 5f;

        while (waitTime < maxWaitTime)
        {
            waitTime += Time.deltaTime;

            if (rb.linearVelocity.magnitude < 0.5f && rb.angularVelocity.magnitude < 0.5f)
            {
                yield return new WaitForSeconds(0.3f);
                DetermineAndReportResult();
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
        }

        Debug.LogWarning($"Ожидание остановки кубика {gameObject.name}");
        DetermineAndReportResult();
    }

    void DetermineAndReportResult()
    {
        if (hasReported) return;

        int upperFace = GetUpperFace();

        if (upperFace >= 0 && upperFace < faces.Length)
        {
            DiceFace result = faces[upperFace];
            Debug.Log($"Кубик: {result.resourceType} +{result.amount} ===");

            hasReported = true;
            OnDiceLanded?.Invoke(result.resourceType, result.amount, gameObject);
        }
        else
        {
            Debug.LogError($"Кубик {gameObject.name}: не удалось определить грань!");
            hasReported = true;
            OnDiceLanded?.Invoke(ResourceType.Wood, 0, gameObject);
        }

        Destroy(gameObject, 2f);
    }

    int GetUpperFace()
    {
        float maxDot = -Mathf.Infinity;
        int upperFace = 0;

        Vector3[] directions = {
            transform.up,      // Грань 0
            -transform.up,     // Грань 1
            transform.right,   // Грань 2
            -transform.right,  // Грань 3
            transform.forward, // Грань 4
            -transform.forward // Грань 5
        };

        for (int i = 0; i < directions.Length; i++)
        {
            float dot = Vector3.Dot(directions[i], Vector3.up);
            if (dot > maxDot)
            {
                maxDot = dot;
                upperFace = i;
            }
        }

        return upperFace;
    }

    public void ThrowDice(Vector3 force)
    {
        hasReported = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(new Vector3(
            Random.Range(-10f, 10f),
            Random.Range(-10f, 10f),
            Random.Range(-10f, 10f)
        ), ForceMode.Impulse);
    }
}