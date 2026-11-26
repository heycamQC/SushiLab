
using UnityEngine;
using UnityEngine.UI;

public class PreparedDishView : MonoBehaviour
{
    [Header("Imagenes del Nigiri")]
    [SerializeField] private Image imageRiceMound;
    [SerializeField] private Image imageNigiriFinal;

    [Header("Imagenes del Maki")]
    [SerializeField] private Image imageRollNori;
    [SerializeField] private Image imageRollRice;
    [SerializeField] private Image imageRollCucumber;
    [SerializeField] private Image imageRollUncut;
    [SerializeField] private Image imageRollCut;

    // Oculta todas las imagenes de preparacion.
    public void ClearView()
    {
        if (imageRiceMound != null)
            imageRiceMound.enabled = false;

        if (imageNigiriFinal != null)
            imageNigiriFinal.enabled = false;

        if (imageRollNori != null)
            imageRollNori.enabled = false;

        if (imageRollRice != null)
            imageRollRice.enabled = false;

        if (imageRollCucumber != null)
            imageRollCucumber.enabled = false;

        if (imageRollUncut != null)
            imageRollUncut.enabled = false;

        if (imageRollCut != null)
            imageRollCut.enabled = false;
    }

    // NIGIRI: mostrar el arroz moldeado.
    public void ShowRice()
    {
        ClearView();

        if (imageRiceMound != null)
            imageRiceMound.enabled = true;
    }

    // NIGIRI: mostrar la porcion terminada.
    public void ShowFinal(RecipeData recipe)
    {
        if (recipe == null || recipe.displaySprite == null
            || imageNigiriFinal == null)
            return;

        ClearView();

        imageNigiriFinal.sprite = recipe.displaySprite;
        imageNigiriFinal.color = Color.white;
        imageNigiriFinal.enabled = true;
    }

    // MAKI: colocar el nori.
    public void ShowRollNori()
    {
        ClearView();

        if (imageRollNori != null)
            imageRollNori.enabled = true;
    }

    // MAKI: extender el arroz sobre el nori.
    public void ShowRollRice()
    {
        if (imageRollNori != null)
            imageRollNori.enabled = true;

        if (imageRollRice != null)
            imageRollRice.enabled = true;
    }

    // MAKI: colocar el relleno de pepino.
    public void ShowRollCucumber()
    {
        if (imageRollNori != null)
            imageRollNori.enabled = true;

        if (imageRollRice != null)
            imageRollRice.enabled = true;

        if (imageRollCucumber != null)
            imageRollCucumber.enabled = true;
    }

    // MAKI: reemplazar las capas por el roll entero.
    public void ShowRollUncut()
    {
        ClearView();

        if (imageRollUncut != null)
            imageRollUncut.enabled = true;
    }

    // MAKI: reemplazar el roll entero por las piezas cortadas.
    public void ShowRollCut()
    {
        ClearView();

        if (imageRollCut != null)
            imageRollCut.enabled = true;
    }
}
