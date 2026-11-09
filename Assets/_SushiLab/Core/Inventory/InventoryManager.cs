using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    // id de IngredientData -> cantidad
    private readonly Dictionary<string, int> stock = new Dictionary<string, int>();

    public event Action<string, int> OnStockChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int GetQuantity(string ingredientId)
    {
        return stock.TryGetValue(ingredientId, out int qty) ? qty : 0;
    }

    public bool HasEnough(string ingredientId, int amount)
    {
        return amount > 0 && GetQuantity(ingredientId) >= amount;
    }

    public void Add(string ingredientId, int amount)
    {
        if (string.IsNullOrEmpty(ingredientId) || amount <= 0) return;

        stock[ingredientId] = GetQuantity(ingredientId) + amount;
        OnStockChanged?.Invoke(ingredientId, stock[ingredientId]);
    }

    public bool TryConsume(string ingredientId, int amount)
    {
        if (!HasEnough(ingredientId, amount)) return false;

        stock[ingredientId] -= amount;
        OnStockChanged?.Invoke(ingredientId, stock[ingredientId]);
        return true;
    }
}