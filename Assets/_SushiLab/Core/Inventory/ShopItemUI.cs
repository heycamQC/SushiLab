using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image icono;
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private TMP_Text textoPrecio;
    [SerializeField] private Button botonComprar;
    

    private IngredientData datosIngrediente;

    public void Configurar(IngredientData datos)
    {
        datosIngrediente = datos;
        icono.sprite = datos.sprite;
        textoNombre.text = datos.displayName;
        textoPrecio.text = $"${datos.price}";
        
        botonComprar.onClick.RemoveAllListeners();
        botonComprar.onClick.AddListener(IntentarComprar);
    }

    private void IntentarComprar()
    {
        if (EconomyManager.Instance == null || InventoryManager.Instance == null) return;

        // 1. Validar si tiene dinero
        if (EconomyManager.Instance.CanAfford(datosIngrediente.price))
        {
            // 2. Cobrar (usando el ID como transacción única)
            string txId = "buy_" + datosIngrediente.id + "_" + System.Guid.NewGuid().ToString();
            if (EconomyManager.Instance.TrySpend(datosIngrediente.price, txId))
            {
                // 3. Dar el ingrediente
                InventoryManager.Instance.Add(datosIngrediente.id, datosIngrediente.defaultUnits);
                Debug.Log($"Compraste {datosIngrediente.displayName}");
            }
        }
        else
        {
            Debug.Log("No tienes suficiente dinero.");
            // Aquí podrías activar un texto rojo de "Fondos insuficientes"
        }
    }
}