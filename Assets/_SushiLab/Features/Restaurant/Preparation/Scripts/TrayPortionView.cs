
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TrayPortionView : MonoBehaviour
{
    [Header("Elementos visuales")]
    [SerializeField] private Image portionImage;
    [SerializeField] private TMP_Text portionNameText;
    [SerializeField] private TMP_Text quantityText;

    // Muestra una receta y cuantas porciones tenemos.
    public void Setup(RecipeData recipe, int quantity, bool hasError)
    {
        if (recipe == null)
            return;

        // Imagen final de la receta.
        if (portionImage != null)
        {
            portionImage.sprite = recipe.displaySprite;
            portionImage.preserveAspect = true;
            portionImage.enabled = recipe.displaySprite != null;
        }

        // Nombre del sushi.
        if (portionNameText != null)
        {
            portionNameText.text = recipe.displayName;

            // Advertir si alguna porcion del grupo tiene errores.
            if (hasError)
            {
                portionNameText.text += "\n(Con error)";
            }
        }

        // Cantidad acumulada de porciones.
        if (quantityText != null)
        {
            quantityText.text = "×" + quantity;
        }
    }
}
