using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopController : MonoBehaviour
{
    [SerializeField] private List<IngredientData> ingredientesDisponibles;
    [SerializeField] private Transform shopContentParent;

    private void Start()
    {
        // Aquí podrías inicializar la UI de la tienda con los ingredientes disponibles
        foreach (var ingrediente in ingredientesDisponibles)
        {
            // Crear un nuevo elemento de UI para cada ingrediente
            GameObject itemUI = Instantiate(Resources.Load<GameObject>("ShopItem"), shopContentParent);
            ShopItemUI shopItemUI = itemUI.GetComponent<ShopItemUI>();
            shopItemUI.Configurar(ingrediente);
        }
    }
  
}
