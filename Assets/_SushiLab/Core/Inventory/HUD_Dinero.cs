using TMPro;
using UnityEngine;

public class MarketHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text textoSaldo;

    private void OnEnable()
    {
        ActualizarSaldo();

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBalanceChanged += ActualizarSaldoUI;
        }
    }

    private void OnDisable()
    {
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBalanceChanged -= ActualizarSaldoUI;
        }
    }

    private void ActualizarSaldo()
    {
        if (EconomyManager.Instance != null && textoSaldo != null)
        {
            textoSaldo.text = $"$ {EconomyManager.Instance.Balance}";
        }
    }

    private void ActualizarSaldoUI(int nuevoSaldo)
    {
        if (textoSaldo != null)
        {
            textoSaldo.text = $"$ {nuevoSaldo}";
        }
    }
}