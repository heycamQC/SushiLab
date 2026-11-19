using UnityEngine;

public static class SaveSystem
{
    private const string KEY_DAY = "Save_Day";
    private const string KEY_BALANCE = "Save_Balance";
    private const string KEY_INVENTORY = "Save_Inventory_"; // Se concatena con el ID

    public static void SaveGame(int day, int balance)
    {
        PlayerPrefs.SetInt(KEY_DAY, day);
        PlayerPrefs.SetInt(KEY_BALANCE, balance);
        PlayerPrefs.Save();
        Debug.Log("Juego guardado.");
    }

    public static int LoadDay()
    {
        return PlayerPrefs.GetInt(KEY_DAY, 1); // Día 1 por defecto
    }

    public static int LoadBalance()
    {
        return PlayerPrefs.GetInt(KEY_BALANCE, 50); // Saldo inicial por defecto
    }

    public static void SaveIngredient(string id, int amount)
    {
        PlayerPrefs.SetInt(KEY_INVENTORY + id, amount);
    }

    public static int LoadIngredient(string id)
    {
        return PlayerPrefs.GetInt(KEY_INVENTORY + id, 0);
    }

    public static void DeleteSave()
    {
        PlayerPrefs.DeleteAll();
    }
}