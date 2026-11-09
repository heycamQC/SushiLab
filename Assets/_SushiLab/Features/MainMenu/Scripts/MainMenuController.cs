using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // <-- ¡ESTO FALTABA! Es obligatorio para usar Button

public class MainMenuController : MonoBehaviour
{
    [Header("Paneles UI")]
    [SerializeField] private GameObject panelAjustes;
    [SerializeField] private GameObject panelRecetario;
    [Header("Botones")]
    [SerializeField] private Button botonAjustes;
    [SerializeField] private Button botonRecetario;
    [SerializeField] private Button botonIrRestaurante;
    [SerializeField] private Button botonIrArcade;
    [SerializeField] private Button botonSalirAjustes;
    [SerializeField] private Button botonSalirRecetario;



    private void Start()
    {
        // Cerramos los paneles al iniciar
        if (panelAjustes != null) panelAjustes.SetActive(false);
        if (panelRecetario != null) panelRecetario.SetActive(false);
        
        if (botonAjustes != null) botonAjustes.onClick.AddListener(AbrirAjustes);
        if (botonRecetario != null) botonRecetario.onClick.AddListener(AbrirRecetario);       
        if (botonSalirAjustes != null) botonSalirAjustes.onClick.AddListener(CerrarAjustes);
        if (botonSalirRecetario != null) botonSalirRecetario.onClick.AddListener(CerrarRecetario);
        if (botonIrRestaurante != null) botonIrRestaurante.onClick.AddListener(IrAlRestaurante);
        if (botonIrArcade != null) botonIrArcade.onClick.AddListener(IrAlArcade);
        
    }

    // ==========================================
    // BOTONES DE PANELES
    // ==========================================
    
    public void AbrirAjustes()
    {
        if (panelAjustes != null) panelAjustes.SetActive(true);
    }

    public void CerrarAjustes()
    {
        if (panelAjustes != null) panelAjustes.SetActive(false);
    }

    public void AbrirRecetario()
    {
        if (panelRecetario != null) panelRecetario.SetActive(true);
    }

    public void CerrarRecetario()
    {
        if (panelRecetario != null) panelRecetario.SetActive(false);
    }

    

    public void IrAlRestaurante()
    {
        SceneManager.LoadScene("SC_Restaurant");
    }

    public void IrAlArcade()
    {
        SceneManager.LoadScene("SC_Arcade");
    }
}