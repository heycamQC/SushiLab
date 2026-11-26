
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PrepStation : MonoBehaviour
{
    [Header("Recetas disponibles")]
    [SerializeField]
    private List<RecipeData> availableRecipes =
        new List<RecipeData>();

    [Header("Modo de prueba manual (opcional)")]
    [SerializeField] private bool manualTestMode = false;
    [SerializeField] private RecipeData testRecipe;

    [Header("Elementos visuales")]
    [SerializeField] private PreparedDishView dishView;
    [SerializeField] private TMP_Text preparationStatus;

    [Header("Bandeja de espera")]
    [SerializeField] private WaitingTray waitingTray;

    private int currentStep;
    private bool hasError;

    // Recetas que siguen siendo posibles con los pasos realizados.
    private List<RecipeData> possibleRecipes =
        new List<RecipeData>();

    // Receta reconocida al terminar la preparacion.
    private RecipeData completedRecipe;

    private void Start()
    {
        ResetPreparation();
    }

    // Recibe el ingrediente que soltamos sobre la mesa.
    public void TryAddIngredient(IngredientData ingredient)
    {
        if (ingredient == null)
            return;

        TryAdvance(RecipeStepType.AddIngredient, ingredient);
    }

    // Boton Enrollar.
    public void TryRoll()
    {
        TryAdvance(RecipeStepType.Roll, null);
    }

    // Boton Cortar.
    public void TryCut()
    {
        TryAdvance(RecipeStepType.Cut, null);
    }

    // Comprueba si el ingrediente o accion coincide
    // con alguna de las recetas que siguen siendo posibles.
    private void TryAdvance(
        RecipeStepType action,
        IngredientData ingredient)
    {
        // No permitir cocinar despues de entregar el pedido.
        if (waitingTray != null && waitingTray.IsClosed)
        {
            SetStatus("Pedido entregado. Espera el siguiente pedido.");
            return;
        }
        if (completedRecipe != null)
        {
            SetStatus("Primero debes emplatar o descartar.");
            return;
        }

        List<RecipeData> matchingRecipes =
            new List<RecipeData>();

        foreach (RecipeData recipe in possibleRecipes)
        {
            if (recipe == null || recipe.orderedSteps == null)
                continue;

            if (currentStep >= recipe.orderedSteps.Count)
                continue;

            RecipeStep nextStep =
                recipe.orderedSteps[currentStep];

            // El tipo de accion debe coincidir.
            if (nextStep.stepType != action)
                continue;

            // Si se agrega un ingrediente, debe ser el correcto.
            if (action == RecipeStepType.AddIngredient &&
                nextStep.ingredient != ingredient)
                continue;

            matchingRecipes.Add(recipe);
        }

        // Ninguna receta coincide con esta accion.
        if (matchingRecipes.Count == 0)
        {
            if (action == RecipeStepType.AddIngredient)
            {
                hasError = true;

                SetStatus(
                    "Ingrediente fuera de orden. Puedes descartar."
                );

                Debug.Log(
                    "Ingrediente incorrecto: "
                    + ingredient.displayName
                );
            }
            else
            {
                SetStatus("Esta accion no corresponde todavia.");
            }

            return;
        }

        // Conservar solamente las recetas compatibles.
        possibleRecipes = matchingRecipes;

        // Registrar que completamos un paso.
        currentStep++;

        // Comprobar si alguna receta ya se completo.
        foreach (RecipeData recipe in possibleRecipes)
        {
            if (currentStep == recipe.orderedSteps.Count)
            {
                completedRecipe = recipe;
                break;
            }
        }

        // Actualizar la representacion visual.
        UpdatePreparationView();

        // Mostrar el estado de la preparacion.
        if (completedRecipe != null)
        {
            FinishPreparation();
        }
        else
        {
            UpdateProgressMessage();
        }
    }

    // Decide que imagen mostrar segun lo preparado.
    private void UpdatePreparationView()
    {
        if (dishView == null || possibleRecipes.Count == 0)
            return;

        // Utilizamos la familia de las recetas compatibles.
        SushiFamily family = possibleRecipes[0].recipeFamily;

        if (family == SushiFamily.Nigiri)
        {
            if (completedRecipe != null)
            {
                dishView.ShowFinal(completedRecipe);
            }
            else
            {
                dishView.ShowRice();
            }
        }
        else if (family == SushiFamily.Roll)
        {
            // Estados visuales del Maki Classic.
            if (currentStep == 1)
                dishView.ShowRollNori();

            else if (currentStep == 2)
                dishView.ShowRollRice();

            else if (currentStep == 3)
                dishView.ShowRollCucumber();

            else if (currentStep == 4)
                dishView.ShowRollUncut();

            else if (currentStep == 5)
                dishView.ShowRollCut();
        }
    }

    // Mensajes segun el progreso de la receta.
    private void UpdateProgressMessage()
    {
        if (possibleRecipes.Count == 0)
            return;

        SushiFamily family = possibleRecipes[0].recipeFamily;

        if (family == SushiFamily.Nigiri)
        {
            SetStatus("Arroz colocado. Agrega el pescado.");
        }
        else if (family == SushiFamily.Roll)
        {
            if (currentStep == 1)
                SetStatus("Nori colocado. Agrega arroz.");

            else if (currentStep == 2)
                SetStatus("Arroz extendido. Agrega pepino.");

            else if (currentStep == 3)
                SetStatus("Relleno listo. Presiona Enrollar.");

            else if (currentStep == 4)
                SetStatus("Roll listo. Presiona Cortar.");
        }
    }

    // Mostrar el resultado cuando se reconoce una receta.
    private void FinishPreparation()
    {
        if (completedRecipe == null)
            return;

        if (hasError)
        {
            SetStatus(
                completedRecipe.displayName
                + " terminado con errores."
            );
        }
        else
        {
            SetStatus(
                "¡" + completedRecipe.displayName
                + " listo! (" + completedRecipe.piecesPerPortion
                + " piezas)"
            );
        }

        Debug.Log(
            "Receta reconocida: " + completedRecipe.displayName
        );
    }

    // Boton Emplatar.
    public void TryPlate()
    {
        if (completedRecipe == null)
        {
            SetStatus("Primero debes terminar un sushi.");
            return;
        }

        if (waitingTray == null)
        {
            SetStatus("La bandeja no esta configurada.");
            return;
        }

        // Emplatar exactamente la receta reconocida.
        RecipeData platedRecipe = completedRecipe;

        bool plated = waitingTray.TryAddPortion(
            platedRecipe,
            hasError
        );

        if (!plated)
        {
            SetStatus("No se pudo emplatar.");
            return;
        }

        // La mesa queda libre para otra preparacion.
        ResetPreparation();

        SetStatus(
            platedRecipe.displayName + " emplatado."
        );
    }

    // Reiniciar solamente la mesa de preparacion.
    public void ResetPreparation()
    {
        currentStep = 0;
        hasError = false;
        completedRecipe = null;

        // Recuperar las recetas posibles.
        possibleRecipes = new List<RecipeData>();

        if (manualTestMode && testRecipe != null)
        {
            possibleRecipes.Add(testRecipe);
        }
        else
        {
            foreach (RecipeData recipe in availableRecipes)
            {
                if (recipe != null)
                    possibleRecipes.Add(recipe);
            }
        }

        if (dishView != null)
            dishView.ClearView();

        if (possibleRecipes.Count == 0)
            SetStatus("No hay recetas disponibles.");
        else
            SetStatus("Arrastra un ingrediente para comenzar.");
    }

    // Conservamos este metodo para los botones
    // del selector antiguo, utilizados en pruebas.
    public void SelectRecipe(RecipeData recipe)
    {
        if (!manualTestMode)
        {
            SetStatus("Modo automatico: cocina directamente.");
            return;
        }

        if (recipe == null)
            return;

        if (currentStep > 0)
        {
            SetStatus("Primero debes emplatar o descartar.");
            return;
        }

        testRecipe = recipe;
        ResetPreparation();

        SetStatus("Receta de prueba: " + recipe.displayName);
    }

    private void SetStatus(string message)
    {
        if (preparationStatus != null)
            preparationStatus.text = message;
    }
}
