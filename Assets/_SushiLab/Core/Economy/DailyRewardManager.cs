using System;
using UnityEngine;

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager Instance { get; private set; }

    [SerializeField] private int maxDailyRewards = 3;

    private int partidasUsadas = 0;
    private string lastResetDate;

    // Evento para que la UI se actualice sola
    public event Action<int, int> OnRewardsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        CargarDatos(); // Cargamos el progreso local
    }

    private void CargarDatos()
    {
        // Obtenemos la fecha actual del dispositivo
        string hoy = DateTime.Now.ToString("yyyy-MM-dd");
        lastResetDate = PlayerPrefs.GetString("DailyRewards_Date", "");
        partidasUsadas = PlayerPrefs.GetInt("DailyRewards_Used", 0);

        // Si es un día nuevo, reiniciamos el contador
        if (lastResetDate != hoy)
        {
            lastResetDate = hoy;
            partidasUsadas = 0;
            GuardarDatos();
        }
    }

    public bool CanPlayForReward()
    {
        // Verificamos si el día cambió mientras el juego estaba abierto
        string hoy = DateTime.Now.ToString("yyyy-MM-dd");
        if (lastResetDate != hoy)
        {
            lastResetDate = hoy;
            partidasUsadas = 0;
            GuardarDatos();
            OnRewardsChanged?.Invoke(partidasUsadas, maxDailyRewards);
        }

        return partidasUsadas < maxDailyRewards;
    }

    public void RegisterMatchPlayed()
    {
        if (CanPlayForReward())
        {
            partidasUsadas++;
            GuardarDatos();
            OnRewardsChanged?.Invoke(partidasUsadas, maxDailyRewards);
        }
    }

    private void GuardarDatos()
    {
        PlayerPrefs.SetString("DailyRewards_Date", lastResetDate);
        PlayerPrefs.SetInt("DailyRewards_Used", partidasUsadas);
        PlayerPrefs.Save();
    }

    public int GetUsedRewards() => partidasUsadas;
    public int GetRemainingRewards() => maxDailyRewards - partidasUsadas;
    public int GetMaxDailyRewards() => maxDailyRewards;
}