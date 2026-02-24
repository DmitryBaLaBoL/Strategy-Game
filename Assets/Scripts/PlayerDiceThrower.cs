using UnityEngine;

public class PlayerDiceThrower : MonoBehaviour
{
    [Header("Префаб кубика")]
    public GameObject dicePrefab;
    public Transform throwPoint;

    [Header("Настройки броска")]
    public float throwCooldown = 1f;
    public KeyCode throwKey = KeyCode.Space;

    private float lastThrowTime;

    void Update()
    {
        if (Input.GetKeyDown(throwKey) && Time.time > lastThrowTime + throwCooldown)
        {
            ThrowDice();
            lastThrowTime = Time.time;
        }
    }

    void ThrowDice()
    {
        if (dicePrefab == null || throwPoint == null) return;

        // Создаем кубик
        GameObject diceObject = Instantiate(dicePrefab, throwPoint.position, Quaternion.identity);
        Dice dice = diceObject.GetComponent<Dice>();

        if (dice != null)
        {
            // Бросаем кубик вперед от игрока
            Vector3 throwDirection = transform.forward * 5f + transform.up * 2f;
            dice.ThrowDice(throwDirection);
        }
    }
}