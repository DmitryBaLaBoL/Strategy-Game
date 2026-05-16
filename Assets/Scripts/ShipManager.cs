using UnityEngine;
using System.Collections.Generic;

public class ShipManager : MonoBehaviour
{
    public static ShipManager Instance { get; private set; }

    private List<Ship> ships = new List<Ship>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void RegisterShip(Ship ship)
    {
        if (!ships.Contains(ship))
        {
            ships.Add(ship);
            Debug.Log($"Корабль {ship.shipName} зарегистрирован");
        }
    }

    public void UnregisterShip(Ship ship)
    {
        ships.Remove(ship);
    }

    public int GetShipsCount()
    {
        return ships.Count;
    }

    public List<Ship> GetShips()
    {
        return ships;
    }

    public int GetTotalWarriors()
    {
        int total = 0;
        foreach (var ship in ships)
        {
            total += ship.currentWarriors;
        }
        return total;
    }
}