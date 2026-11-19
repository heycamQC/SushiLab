# Interfaces de Core (Sushi Lab)

Mantenido por: Valele
Última actualización: 2026-11-9

## EconomyManager

- `EconomyManager.Instance`
- `int Balance { get; }`
- `bool CanAfford(int amount)`
- `bool TrySpend(int amount, string transactionId)`
- `bool AddSushiDollars(int amount, string transactionId)`
- `event Action<int> OnBalanceChanged`

**Regla:** ningún módulo modifica `Balance` directamente.

## InventoryManager

- `InventoryManager.Instance`
- `int GetQuantity(string ingredientId)`
- `bool HasEnough(string ingredientId, int amount)`
- `bool TryConsume(string ingredientId, int amount)`
- `void Add(string ingredientId, int amount)`
- `event Action<string,int> OnStockChanged`

**Regla:** la cocina solo usa `TryConsume`. Market usa `Add`.

## IngredientData (ScriptableObject)

Campos: `id, displayName, sprite, price, category, defaultUnits`
Ubicación: `Assets/_SushiLab/Core/SharedData/Ingredients/`
