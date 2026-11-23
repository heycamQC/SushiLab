using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabUserDataBackend : IBalanceBackend
{
    private const string Key = "SD_Balance";
    private readonly int startingBalance;
    private int cached;

    public PlayFabUserDataBackend(int startingBalance) { this.startingBalance = startingBalance; }

    public void Load(Action<bool, int> onDone)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest { Keys = new List<string> { Key } },
            r =>
            {
                if (r.Data != null && r.Data.TryGetValue(Key, out var rec) && int.TryParse(rec.Value, out int b))
                {
                    cached = b;
                    onDone?.Invoke(true, cached);
                }
                else
                {
                    Write(startingBalance, onDone); // Primer inicio de sesión
                }
            },
            e => { Debug.LogError(e.GenerateErrorReport()); onDone?.Invoke(false, 0); });
    }

    public void Add(int amount, Action<bool, int> onDone) => Write(cached + amount, onDone);

    public void Spend(int amount, Action<bool, int> onDone)
    {
        if (cached < amount) { onDone?.Invoke(false, cached); return; }
        Write(cached - amount, onDone);
    }

    private void Write(int value, Action<bool, int> onDone)
    {
        PlayFabClientAPI.UpdateUserData(
            new UpdateUserDataRequest { Data = new Dictionary<string, string> { { Key, value.ToString() } } },
            r => { cached = value; onDone?.Invoke(true, cached); },
            e => { Debug.LogError(e.GenerateErrorReport()); onDone?.Invoke(false, cached); });
    }
}