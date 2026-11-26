using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class BootstrapLoader : MonoBehaviour
{
    [Header("Referencias UI")]
    public Slider barraProgreso;
    public TextMeshProUGUI textoPorcentaje;

    [Header("Configuración")]
    public string nombreEscenaDestino = "RestaurantePrincipal";
    public float tiempoMinimoCarga = 2.0f; // Para que se aprecie el arte si carga muy rápido

    void Start()
    {
        StartCoroutine(CargarEscenaAsync());
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }
 private void OnLoginSuccess(LoginResult result)
{
    Debug.Log("PlayFab Login OK");
    
    // Aquí conectamos la economía a PlayFab
    if (EconomyManager.Instance != null)
    {
        // startingBalance es 50 por defecto
        EconomyManager.Instance.Initialize(new PlayFabUserDataBackend(50)); 
    }
    EconomyManager.Instance.Initialize(new PlayFabUserDataBackend(50), () =>
{
    Debug.Log("Economía lista y sincronizada con PlayFab.");
});
}

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("PlayFab Login FAIL: " + error.GenerateErrorReport());
    }

    IEnumerator CargarEscenaAsync()
    {
        AsyncOperation operacion = SceneManager.LoadSceneAsync("SC_MainMenu");
        
        operacion.allowSceneActivation = false;

        float tiempoTranscurrido = 0f;

        while (!operacion.isDone)
        {
            tiempoTranscurrido += Time.deltaTime;

            float progresoReal = Mathf.Clamp01(operacion.progress / 0.9f);
            
            float progresoVisual = Mathf.Min(progresoReal, tiempoTranscurrido / tiempoMinimoCarga);

            barraProgreso.value = progresoVisual;
            if (textoPorcentaje != null)
            {
                textoPorcentaje.text = Mathf.RoundToInt(progresoVisual * 100f) + "%";
            }

            if (progresoVisual >= 1f && operacion.progress >= 0.9f)
            {
                operacion.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}