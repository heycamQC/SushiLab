
using System;
using System.Collections.Generic;
using UnityEngine;

// Familias de sushi que tendremos en el juego.
public enum SushiFamily
{
    Nigiri,
    Roll,
    Temaki,
    Gunkan
}

// Acciones posibles durante la preparación.
public enum RecipeStepType
{
    AddIngredient,
    Roll,
    Cut,
    FormCone
}

// Representa un paso de preparación de la receta.
[Serializable]
public class RecipeStep
{
    public RecipeStepType stepType = RecipeStepType.AddIngredient;

    // Se utiliza cuando el paso consiste en agregar un ingrediente.
    public IngredientData ingredient;

    // Cantidad requerida de ese ingrediente.
    [Min(1)]
    public int amount = 1;
}

// Datos originales de una receta de Sushi Lab.
[CreateAssetMenu(
    fileName = "SO_Recipe_",
    menuName = "SushiLab/Recipe Data"
)]
public class RecipeData : ScriptableObject
{
    [Header("Identidad")]
    public string id;
    public string displayName;
    public Sprite displaySprite;

    [Header("Clasificacion")]
    public SushiFamily recipeFamily;

    [Header("Progresion y Economia")]
    [Min(1)] public int unlockDay = 1;
    [Min(0)] public int basePrice;

    [Header("Porciones")]
    [Min(1)] public int piecesPerPortion = 1;

    [Header("Secuencia de preparacion")]
    public List<RecipeStep> orderedSteps = new List<RecipeStep>();
}
