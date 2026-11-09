using UnityEngine;

[CreateAssetMenu(menuName = "SushiLab/Ingredient Data", fileName = "SO_Ingredient_")]
public class IngredientData : ScriptableObject
{
    [Header("Identidad")]
    [Tooltip("ID estable usado en guardados. No cambiar tras publicar.")]
    public string id;
    public string displayName;

    [Header("Visual")]
    public Sprite sprite;

    [Header("Economía")]
    [Min(0)] public int price;
    public IngredientCategory category;

    [Header("Inventario")]
    [Tooltip("Unidades por compra o por defecto en inventario inicial.")]
    [Min(1)] public int defaultUnits = 1;
}

public enum IngredientCategory
{
    Rice,
    Fish,
    Seaweed,
    Topping,
    Condiment,
    Other
}