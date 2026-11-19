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
    [SerializeField] private TMP_Text textoSushiGems; // Si tienes EconomyManager para gemas, si no, lo dejamos estático o local
    [SerializeField] private TMP_Text textoPartidasRecompensa;

    // Variable local temporal para las gemas hasta que hagas el sistema de gemas en el Core
    [SerializeField] private int sushiGemsActuales = 100; 

    private void Start()
    {
        if (botonIrMainMenu != null) {
            Debug.Log("botonIrMainMenu is not null, adding listener.");
            botonIrMainMenu.onClick.AddListener(IrAlMainMenu);
        }
        if (botonIrTicTacToe != null) {
            botonIrTicTacToe.onClick.AddListener(IrATictacToe);
        }
        if (botonIrMemory != null) {
            botonIrMemory.onClick.AddListener(IrAMemory);
        }
        if (botonIrSushiFlappy != null) {
            botonIrSushiFlappy.onClick.AddListener(IrASushiFlappy);
        }
        if (botonIrSushiTower != null) {
            botonIrSushiTower.onClick.AddListener(IrASushiTower);
        }

        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.OnBalanceChanged += ActualizarTextoSushiDolares;
        }
        
        if (DailyRewardManager.Instance != null)
        {
            DailyRewardManager.Instance.OnRewardsChanged += ActualizarTextoRecompensas;
        }
    }

    private void OnEnable()
    {
        ActualizarTodosLosTextos();
    }

    private void OnDestroy()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnBalanceChanged -= ActualizarTextoSushiDolares;
            
        if (DailyRewardManager.Instance != null)
            DailyRewardManager.Instance.OnRewardsChanged -= ActualizarTextoRecompensas;
    }

    private void ActualizarTodosLosTextos()
    {
        if (EconomyManager.Instance != null)
            textoSushiDolares.text = EconomyManager.Instance.Balance.ToString();
        else
            textoSushiDolares.text = "0"; 

        if (textoSushiGems != null)
            textoSushiGems.text = sushiGemsActuales.ToString();

        if (DailyRewardManager.Instance != null)
        {
            int usadas = DailyRewardManager.Instance.GetUsedRewards();
            int maximo = DailyRewardManager.Instance.GetMaxDailyRewards();

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

    
    public void IrAlMainMenu(){ SceneManager.LoadScene("SC_MainMenu");
    Debug.Log("Cambiando a la escena SC_MainMenu"); 
    } 
    public void IrATictacToe() { SceneManager.LoadScene("MG_TicTacToe"); }
    public void IrAMemory() { SceneManager.LoadScene("MG_Memory"); }
    public void IrASushiFlappy() { SceneManager.LoadScene("MG_SushiFlappy"); }
    public void IrASushiTower() { SceneManager.LoadScene("MG_SushiTower"); }
}