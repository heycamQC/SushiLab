using System;

public interface IBalanceBackend
{
    void Load(Action<bool, int> onDone);              // saldo actual
    void Add(int amount, Action<bool, int> onDone);   // devuelve saldo nuevo
    void Spend(int amount, Action<bool, int> onDone); // devuelve saldo nuevo
}