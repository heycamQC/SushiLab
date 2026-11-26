
using UnityEngine;
using UnityEngine.EventSystems;

public class IngredientDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private PrepStation prepStation;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null || prepStation == null)
            return;

        // Identificamos qué ingrediente se arrastró.
        IngredientDragItem dragItem =
            eventData.pointerDrag.GetComponent<IngredientDragItem>();

        if (dragItem == null)
            return;

        // Entregamos el ingrediente a la estación.
        prepStation.TryAddIngredient(dragItem.Ingredient);
    }
}
