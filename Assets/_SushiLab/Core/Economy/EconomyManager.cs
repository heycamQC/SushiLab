using System;
using System.Collections.Generic;
using UnityEngine;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [SerializeField] private int startingBalance = 50;
    public int Balance { get; private set; }
    public event Action<int> OnBalanceChanged;

    private readonly HashSet<string> processedTransactions = new HashSet<string>();
    private IBalanceBackend backend;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Balance = startingBalance;
    }

    // NUEVO: Se llama después de que PlayFab te da el Login OK
    public void Initialize(IBalanceBackend newBackend, Action onReady = null)
    {
        backend = newBackend;
        backend.Load((ok, bal) =>
        {
            if (ok) SetBalanceFromServer(bal);
            onReady?.Invoke();
        });
    }

    public bool CanAfford(int amount) => amount >= 0 && Balance >= amount;

    public bool TrySpend(int amount, string transactionId)
    {
        if (amount < 0 || !CanAfford(amount) || !RegisterTransaction(transactionId)) return false;

        Balance -= amount;
        OnBalanceChanged?.Invoke(Balance);

        // NUEVO: Avisar a PlayFab
        backend?.Spend(amount, (ok, bal) => { if (ok) SetBalanceFromServer(bal); else Resync(); });
        return true;
    }

    public bool AddSushiDollars(int amount, string transactionId)
    {
        if (amount <= 0 || !RegisterTransaction(transactionId)) return false;

        Balance += amount;
        OnBalanceChanged?.Invoke(Balance);

        // NUEVO: Avisar a PlayFab
        backend?.Add(amount, (ok, bal) => { if (ok) SetBalanceFromServer(bal); else Resync(); });
        return true;
    }

    public void SetBalanceFromServer(int balance)
    {
        if (balance < 0) return;
        Balance = balance;
        OnBalanceChanged?.Invoke(Balance);
    }

    private void Resync()
    {
        backend?.Load((ok, bal) => { if (ok) SetBalanceFromServer(bal); });
    }

    private bool RegisterTransaction(string transactionId)
    {
        if (string.IsNullOrEmpty(transactionId)) return false;
        return processedTransactions.Add(transactionId);
    }
}