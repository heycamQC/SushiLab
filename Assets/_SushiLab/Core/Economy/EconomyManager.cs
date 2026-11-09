using System;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [SerializeField] private int startingBalance = 50;

    public int Balance { get; private set; }

    // Evita recompensas duplicadas por transactionId (guía §4.3 y §4.6)
    private readonly HashSet<string> processedTransactions = new HashSet<string>();

    public event Action<int> OnBalanceChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Balance = startingBalance;
    }

    public bool CanAfford(int amount) => amount >= 0 && Balance >= amount;

    public bool TrySpend(int amount, string transactionId)
    {
        if (amount < 0) return false;
        if (!CanAfford(amount)) return false;
        if (!RegisterTransaction(transactionId)) return false;

        Balance -= amount;
        OnBalanceChanged?.Invoke(Balance);
        return true;
    }

    public bool AddSushiDollars(int amount, string transactionId)
    {
        if (amount <= 0) return false;
        if (!RegisterTransaction(transactionId)) return false;

        Balance += amount;
        OnBalanceChanged?.Invoke(Balance);
        return true;
    }

    private bool RegisterTransaction(string transactionId)
    {
        if (string.IsNullOrEmpty(transactionId))
        {
            Debug.LogWarning("EconomyManager: transactionId vacío. Usa un ID único.");
            return false;
        }
        return processedTransactions.Add(transactionId); // false si ya existe
    }
}