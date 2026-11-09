
using UnityEngine;
using UnityEngine.UI;

public class MemoryDifficultyMenuUI : MonoBehaviour
{
    [Header("Controlador del minijuego")]
    [SerializeField] private MemoryGameController gameController;

    [Header("Dificultad Normal")]
    [SerializeField] private Button normalPlayButton;
    [SerializeField] private GameObject normalLockedGroup;

    [Header("Dificultad Dificil")]
    [SerializeField] private Button hardPlayButton;
    [SerializeField] private GameObject hardLockedGroup;

    private void OnEnable()
    {
        RefreshDifficultyStates();
    }

    public void RefreshDifficultyStates()
    {
        if (gameController == null)
        {
            Debug.LogWarning(
                "Sushi Memory: falta asignar el controlador."
            );
            return;
        }

        // Comprobar si Normal esta desbloqueado.
        bool normalUnlocked = gameController.IsDifficultyUnlocked(
            MemoryGameController.MemoryDifficulty.Normal
        );

        if (normalPlayButton != null)
        {
            normalPlayButton.interactable = normalUnlocked;
        }

        if (normalLockedGroup != null)
        {
            normalLockedGroup.SetActive(!normalUnlocked);
        }

        // Comprobar si Dificil esta desbloqueado.
        bool hardUnlocked = gameController.IsDifficultyUnlocked(
            MemoryGameController.MemoryDifficulty.Hard
        );

        if (hardPlayButton != null)
        {
            hardPlayButton.interactable = hardUnlocked;
        }

        if (hardLockedGroup != null)
        {
            hardLockedGroup.SetActive(!hardUnlocked);
        }
    }
}
