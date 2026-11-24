
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestOrderValidator : MonoBehaviour
{
    [Serializable]
    public class TestOrderLine
    {
        public RecipeData recipe;

        [Min(1)]
        public int quantity = 1;
    }

    [Header("Bandeja de espera")]
    [SerializeField] private WaitingTray waitingTray;

    [Header("Pedido de prueba")]
    [SerializeField]
    private List<TestOrderLine> requestedPortions =
        new List<TestOrderLine>();

    [Header("Interfaz")]
    [SerializeField] private TMP_Text orderText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button deliverButton;

    // Impide entregar el mismo pedido dos veces.
    private bool orderDelivered = false;

    private void OnEnable()
    {
        if (waitingTray != null)
            waitingTray.OnTrayChanged += RefreshOrderProgress;
    }

    private void OnDisable()
    {
        if (waitingTray != null)
            waitingTray.OnTrayChanged -= RefreshOrderProgress;
    }

    private void Start()
    {
        RefreshOrderProgress();
    }

    // Actualizar el ticket al emplatar.
    private void RefreshOrderProgress()
    {
        if (deliverButton != null)
            deliverButton.interactable = !orderDelivered;

        if (orderText != null)
        {
            string message = orderDelivered
                ? "PEDIDO ENTREGADO\n\n"
                : "PEDIDO DE PRUEBA\n\n";

            foreach (TestOrderLine line in requestedPortions)
            {
                if (line == null || line.recipe == null)
                    continue;

                // Despues de entregar, conservar el resumen.
                int prepared = orderDelivered
                    ? line.quantity
                    : (waitingTray != null
                        ? waitingTray.CountPortionsOf(line.recipe)
                        : 0);

                message += line.recipe.displayName
                    + " " + prepared + "/" + line.quantity;

                if (!orderDelivered && prepared > line.quantity)
                    message += " (Exceso)";

                message += "\n";
            }

            orderText.text = message;
        }

        if (orderDelivered)
        {
            SetResult("¡Pedido entregado!");
            return;
        }

        // Indicar si el pedido ya esta listo.
        string validationMessage;

        if (CheckOrder(out validationMessage))
            SetResult("Pedido completo. Puedes entregar.");
        else
            SetResult("Pedido en preparacion.");
    }

    // Boton antiguo: solo comprueba el pedido.
    public void ValidateOrder()
    {
        if (orderDelivered)
        {
            SetResult("Este pedido ya fue entregado.");
            return;
        }

        string message;
        CheckOrder(out message);

        SetResult(message);
    }

    // NUEVO: boton Entregar pedido.
    public void TryDeliverOrder()
    {
        // Bloquear cualquier segunda entrega.
        if (orderDelivered)
        {
            SetResult("Este pedido ya fue entregado.");
            return;
        }

        string message;

        // Validar antes de entregar.
        if (!CheckOrder(out message))
        {
            SetResult(message);
            return;
        }

        // Confirmar el resultado una sola vez.
        orderDelivered = true;

        // Retirar las porciones del pedido entregado.
        waitingTray.CompleteDelivery();

        // Actualizar el ticket y bloquear el boton.
        RefreshOrderProgress();

        Debug.Log("Pedido entregado correctamente (simulacion).");
    }

    // Valida recetas, cantidades y errores.
    private bool CheckOrder(out string message)
    {
        if (waitingTray == null)
        {
            message = "La bandeja no esta configurada.";
            return false;
        }

        if (requestedPortions == null ||
            requestedPortions.Count == 0)
        {
            message = "El pedido esta vacio.";
            return false;
        }

        Dictionary<RecipeData, int> required =
            new Dictionary<RecipeData, int>();

        int totalRequired = 0;

        // Agrupar las cantidades del pedido.
        foreach (TestOrderLine line in requestedPortions)
        {
            if (line == null ||
                line.recipe == null ||
                line.quantity < 1)
            {
                message = "El pedido tiene datos incompletos.";
                return false;
            }

            if (!required.ContainsKey(line.recipe))
                required.Add(line.recipe, 0);

            required[line.recipe] += line.quantity;
            totalRequired += line.quantity;
        }

        // Comprobar la cantidad de cada receta.
        foreach (KeyValuePair<RecipeData, int> item in required)
        {
            int prepared =
                waitingTray.CountPortionsOf(item.Key);

            if (prepared < item.Value)
            {
                message = item.Key.displayName
                    + ": faltan " + (item.Value - prepared)
                    + " porciones.";

                return false;
            }

            if (prepared > item.Value)
            {
                message = item.Key.displayName
                    + ": sobran " + (prepared - item.Value)
                    + " porciones.";

                return false;
            }
        }

        // Detectar recetas adicionales.
        if (waitingTray.PortionCount != totalRequired)
        {
            message = "Hay sushis adicionales en la bandeja.";
            return false;
        }

        // Detectar preparaciones incorrectas.
        if (waitingTray.HasPortionsWithErrors())
        {
            message = "Hay preparaciones con errores.";
            return false;
        }

        message = "¡Pedido correcto!";
        return true;
    }

    private void SetResult(string message)
    {
        if (resultText != null)
            resultText.text = message;
    }
}
