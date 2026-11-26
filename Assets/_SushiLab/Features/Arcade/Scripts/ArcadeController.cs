using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; 

public class ArcadeController : MonoBehaviour
{
    [Header("Botones")]
    [SerializeField] private Button botonIrMainMenu;
    [SerializeField] private Button botonIrTicTacToe;
    [SerializeField] private Button botonIrMemory;
    [SerializeField] private Button botonIrSushiFlappy;
    [SerializeField] private Button botonIrSushiTower;

    [Header("Textos HUD (Datos Reales)")]
    [SerializeField] private TMP_Text textoSushiDolares;
    [SerializeField] private TMP_Text textoSushiGems; 
    [SerializeField] private TMP_Text textoPartidasRecompensa;

    // Variable local temporal para las gemas hasta que hagas el sistema de gemas en el Core
    [SerializeField] private int sushiGemsActuales = 100; 

    private void Start()
    {
        // Configurar botones
        if (botonIrMainMenu != null) botonIrMainMenu.onClick.AddListener(IrAlMainMenu);
        if (botonIrTicTacToe != null) botonIrTicTacToe.onClick.AddListener(IrATictacToe);
        if (botonIrMemory != null) botonIrMemory.onClick.AddListener(IrAMemory);
        if (botonIrSushiFlappy != null) botonIrSushiFlappy.onClick.AddListener(IrASushiFlappy);
        if (botonIrSushiTower != null) botonIrSushiTower.onClick.AddListener(IrASushiTower);

        // Suscribirse a los eventos
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBalanceChanged += ActualizarTextoSushiDolares;
        }
        else
        {
            Debug.LogWarning("ArcadeController: No se encontró EconomyManager.");
        }
        
        if (DailyRewardManager.Instance != null)
        {
            DailyRewardManager.Instance.OnRewardsChanged += ActualizarTextoRecompensas;
        }
        else
        {
            Debug.LogWarning("ArcadeController: No se encontró DailyRewardManager.");
        }
        Invoke(nameof(ActualizarTodosLosTextos), 0.5f); 
    }

    private void OnEnable()
    {
        // Actualizar textos cada vez que se abre la escena
        ActualizarTodosLosTextos();
    }

    private void OnDestroy()
    {
        // Desuscribirse para evitar errores de memoria
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnBalanceChanged -= ActualizarTextoSushiDolares;
            
        if (DailyRewardManager.Instance != null)
            DailyRewardManager.Instance.OnRewardsChanged -= ActualizarTextoRecompensas;
    }

       public void ActualizarTodosLosTextos()
    {
        // 1. Sushi Dólares
        if (textoSushiDolares != null)
        {
            if (EconomyManager.Instance != null)
                textoSushiDolares.text = EconomyManager.Instance.Balance.ToString();
            else
                textoSushiDolares.text = "0";
        }

        // 2. Sushi Gems
        if (textoSushiGems != null)
            textoSushiGems.text = sushiGemsActuales.ToString();

        // 3. Partidas con Recompensa (Límite diario)
        if (textoPartidasRecompensa != null && DailyRewardManager.Instance != null)
        {
            int maximo = DailyRewardManager.Instance.GetMaxDailyRewards();
            int usadas = DailyRewardManager.Instance.GetUsedRewards();
            textoPartidasRecompensa.text = $"{usadas}/{maximo}";
        }
    }

    private void ActualizarTextoSushiDolares(int nuevoBalance)
    {
        if (textoSushiDolares != null)
            textoSushiDolares.text = nuevoBalance.ToString();
    }

    private void ActualizarTextoRecompensas(int usadas, int maximo)
    {
        if (textoPartidasRecompensa != null)
            textoPartidasRecompensa.text = $"{usadas}/{maximo}";
    }

    // ==========================================
    // NAVEGACIÓN
    // ==========================================
    public void IrAlMainMenu() 
    { 
        SceneManager.LoadScene("SC_MainMenu"); 
    } 
    public void IrATictacToe() { SceneManager.LoadScene("MG_TicTacToe"); }
    public void IrAMemory() { SceneManager.LoadScene("MG_Memory"); }
    public void IrASushiFlappy() { SceneManager.LoadScene("MG_SushiFlappy"); }
    public void IrASushiTower() { SceneManager.LoadScene("MG_SushiTower"); }
}