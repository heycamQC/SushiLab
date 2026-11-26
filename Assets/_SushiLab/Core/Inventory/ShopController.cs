using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    [Header("Configuración de la Tienda")]
    [SerializeField] private Transform contenedorItems;
    [SerializeField] private ShopItemUI prefabShopItem;

    [Header("Catálogo")]
    [SerializeField] private List<IngredientData> catalogoIngredientes;

    [Header("Progreso (Pruebas)")]
    [Tooltip("Cambia este número para probar el bloqueo de los candados.")]
    [SerializeField] private int diaActualDePrueba = 1;

    private void OnEnable()
    {
        PoblarTienda();
    }

    private void PoblarTienda()
    {
        if (contenedorItems == null)
        {
            Debug.LogError("ShopController: falta asignar el contenedor de items.", this);
            return;
        }

        if (prefabShopItem == null)
        {
            Debug.LogError("ShopController: falta asignar el prefab ShopItem.", this);
            return;
        }

        if (catalogoIngredientes == null)
        {
            Debug.LogError("ShopController: falta asignar el catálogo de ingredientes.", this);
            return;
        }

        foreach (Transform child in contenedorItems)
        {
            Destroy(child.gameObject);
        }

        foreach (var ingrediente in catalogoIngredientes)
        {
            if (ingrediente != null)
            {
                ShopItemUI nuevoItem = Instantiate(prefabShopItem, contenedorItems);
                nuevoItem.Configurar(ingrediente, diaActualDePrueba);
            }
        }
    }
}