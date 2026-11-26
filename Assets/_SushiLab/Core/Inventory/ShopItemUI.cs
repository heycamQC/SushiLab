using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [Header("Referencias visuales")]
    [SerializeField] private Image iconoIngrediente;
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private TMP_Text textoPrecio;
    [SerializeField] private Button botonComprar;

    [Header("Estado Bloqueado")]
    [SerializeField] private GameObject iconoCandado;
    [SerializeField] private TMP_Text textoDiaDesbloqueo;

    [Header("Feedback de Compra (Opcional)")]
    [SerializeField] private TMP_Text textoStock; // Para mostrar "Tienes: X" 
    [SerializeField] private Color colorSinDinero = Color.red;
    private Color colorOriginalBoton;

    private IngredientData datosIngrediente;
    private bool estaDesbloqueado;

    private void Awake()
    {
        if (botonComprar != null)
        {
            colorOriginalBoton = botonComprar.image.color;
        }
    }

    public void Configurar(IngredientData datos, int diaActualJuego)
    {
        datosIngrediente = datos;
        
        iconoIngrediente.sprite = datos.sprite;
        textoNombre.text = datos.displayName;
        textoPrecio.text = $"${datos.price}";

        estaDesbloqueado = diaActualJuego >= datos.unlockDay;

        if (iconoCandado != null)
            iconoCandado.SetActive(!estaDesbloqueado);

        if (textoDiaDesbloqueo != null)
        {
            textoDiaDesbloqueo.gameObject.SetActive(!estaDesbloqueado);
            if (!estaDesbloqueado) textoDiaDesbloqueo.text = $"Día {datos.unlockDay}";
        }

        botonComprar.interactable = estaDesbloqueado;

        // Escuchamos cuando el inventario cambie para actualizar el stock
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnStockChanged += AlCambiarInventario;
        }

        ActualizarStockUI();

        botonComprar.onClick.RemoveAllListeners();
        botonComprar.onClick.AddListener(IntentarComprar);
    }

    private void OnDestroy()
    {
        // Limpiamos el evento del inventario
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnStockChanged -= AlCambiarInventario;
        }
    }

    private void ActualizarStockUI()
    {
        if (textoStock != null && InventoryManager.Instance != null)
        {
            int cantidad = InventoryManager.Instance.GetQuantity(datosIngrediente.id);
            textoStock.text = $"Tienes: {cantidad}";
        }
    }

    private void AlCambiarInventario(string id, int nuevaCantidad)
    {
        // Si el ingrediente que cambió es el de esta tarjeta, actualizamos el texto
        if (id == datosIngrediente.id)
        {
            ActualizarStockUI();
        }
    }

    private void IntentarComprar()
    {
        if (EconomyManager.Instance == null || InventoryManager.Instance == null) return;

        if (!EconomyManager.Instance.CanAfford(datosIngrediente.price))
        {
            Debug.Log("No tienes suficiente dinero.");
            StartCoroutine(ParpadearRojo()); // Efecto visual si no tiene dinero
            return;
        }

        string txId = "buy_" + datosIngrediente.id + "_" + System.Guid.NewGuid().ToString();

        if (EconomyManager.Instance.TrySpend(datosIngrediente.price, txId))
        {
            InventoryManager.Instance.Add(datosIngrediente.id, datosIngrediente.defaultUnits);
            Debug.Log($"Compraste {datosIngrediente.displayName}");
            
            // El texto de stock se actualiza solo gracias al evento OnStockChanged
        }
    }

    private System.Collections.IEnumerator ParpadearRojo()
    {
        botonComprar.image.color = colorSinDinero;
        yield return new WaitForSeconds(0.3f);
        botonComprar.image.color = colorOriginalBoton;
    }
}