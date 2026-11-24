
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaitingTray : MonoBehaviour
{
    [Header("Contenido de la bandeja")]
    [SerializeField] private Transform contentPortions;
    [SerializeField] private TrayPortionView portionPrefab;

    [Header("Textos de la bandeja")]
    [SerializeField] private TMP_Text emptyTrayText;
    [SerializeField] private TMP_Text portionCountText;

    // Cada porcion se conserva individualmente.
    private class PlatedPortion
    {
        public RecipeData recipe;
        public bool hasError;

        public PlatedPortion(RecipeData recipe, bool hasError)
        {
            this.recipe = recipe;
            this.hasError = hasError;
        }
    }

    private readonly List<PlatedPortion> portions =
        new List<PlatedPortion>();

    // Una tarjeta visual por receta.
    private readonly Dictionary<RecipeData, TrayPortionView> portionViews =
        new Dictionary<RecipeData, TrayPortionView>();

    private bool isClosed = false;
    // Indica si el pedido ya fue entregado.
    public bool IsClosed
    {
        get { return isClosed; }
    }

    // Avisa al ticket cuando cambia la bandeja.
    public event Action OnTrayChanged;

    public int PortionCount
    {
        get { return portions.Count; }
    }

    private void Start()
    {
        UpdateTrayUI();
    }

    // Agrega una porcion terminada.
    public bool TryAddPortion(RecipeData recipe, bool hasError)
    {
        if (isClosed)
        {
            Debug.LogWarning(
                "La bandeja esta cerrada. El pedido ya fue entregado."
            );

            return false;
        }

        if (recipe == null)
            return false;

        if (contentPortions == null || portionPrefab == null)
        {
            Debug.LogError(
                "WaitingTray: faltan referencias en el Inspector."
            );

            return false;
        }

        TrayPortionView portionView;

        // Si la receta no tiene tarjeta, creamos una.
        if (!portionViews.TryGetValue(recipe, out portionView)
            || portionView == null)
        {
            portionView = Instantiate(
                portionPrefab,
                contentPortions
            );

            portionViews[recipe] = portionView;
        }

        // Registramos la porcion individual.
        portions.Add(new PlatedPortion(recipe, hasError));

        // Actualizamos la cantidad de esa receta.
        int quantity = CountPortionsOf(recipe);
        bool recipeHasError = HasErrorsForRecipe(recipe);

        portionView.Setup(
            recipe,
            quantity,
            recipeHasError
        );

        UpdateTrayUI();

        // Informar al ticket.
        OnTrayChanged?.Invoke();

        Debug.Log(
            "Porcion emplatada: " + recipe.displayName
            + " | Total bandeja: " + portions.Count
        );

        return true;
    }

    // Cuenta las porciones de una receta.
    public int CountPortionsOf(RecipeData recipe)
    {
        int count = 0;

        foreach (PlatedPortion portion in portions)
        {
            if (portion.recipe == recipe)
                count++;
        }

        return count;
    }

    // Detecta cualquier preparacion marcada con errores.
    public bool HasPortionsWithErrors()
    {
        foreach (PlatedPortion portion in portions)
        {
            if (portion.hasError)
                return true;
        }

        return false;
    }

    // Detecta errores en una receta especifica.
    private bool HasErrorsForRecipe(RecipeData recipe)
    {
        foreach (PlatedPortion portion in portions)
        {
            if (portion.recipe == recipe && portion.hasError)
                return true;
        }

        return false;
    }

    // Se llama SOLO cuando se confirma la entrega.
    public void CompleteDelivery()
    {
        if (isClosed)
            return;

        // Cerrar la bandeja para evitar nuevas porciones.
        isClosed = true;

        // Eliminar las tarjetas visuales.
        foreach (TrayPortionView view in portionViews.Values)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        // Vaciar los registros del pedido entregado.
        portionViews.Clear();
        portions.Clear();

        UpdateTrayUI();

        // Avisar que la bandeja cambio.
        OnTrayChanged?.Invoke();

        Debug.Log("Bandeja vaciada despues de la entrega.");
    }

    // Actualiza el contador general.
    private void UpdateTrayUI()
    {
        int count = portions.Count;

        if (emptyTrayText != null)
        {
            emptyTrayText.gameObject.SetActive(count == 0);
        }

        if (portionCountText != null)
        {
            portionCountText.text = count == 1
                ? "1 porcion"
                : count + " porciones";
        }
    }
}
