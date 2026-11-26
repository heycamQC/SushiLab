using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TowerSpawner : MonoBehaviour
{
    public GameObject[] sushiPrefabs;
    public TowerGameController gameController;
    public TowerCamera towerCamera;
    public TowerUIController towerUI;

    public float moveSpeed = 2f;
    public float moveRange = 1.6f;
    public float spawnOffset = 3.2f;
    public float speedIncrease = 0.2f;
    public int piecesPerIncrease = 15;

    private GameObject currentSushi;
    private bool canDrop = false;
    private bool stopped = false;
    private float currentSpawnHeight;

    void Start()
    {
        canDrop = false;
    }

    public void BeginGame()
    {
        if (stopped || gameController.gameOver || currentSushi != null)
            return;

        SpawnSushi();
    }

    void Update()
    {
        if (towerUI != null && (!towerUI.gameStarted || towerUI.isPaused))
        {
            return;
        }

        if (!canDrop || currentSushi == null || stopped)
            return;

        float currentSpeed = moveSpeed +
            (gameController.score / piecesPerIncrease) * speedIncrease;

        float x = Mathf.Sin(Time.time * currentSpeed) * moveRange;

        currentSushi.transform.position = new Vector3(
            x, currentSpawnHeight, 0
        );

        bool mouseClick = Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        bool spaceKey = Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame;

        bool screenTouch = Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        bool pointerOverUI = EventSystem.current != null &&
                            EventSystem.current.IsPointerOverGameObject();

        if ((mouseClick || spaceKey || screenTouch) && !pointerOverUI)
        {
            DropSushi();
        }
    }

    void SpawnSushi()
    {
        if (stopped || gameController.gameOver)
            return;

        currentSpawnHeight = Mathf.Max(
            3.5f,
            gameController.highestSupportY + spawnOffset
        );

        int randomIndex = Random.Range(0, sushiPrefabs.Length);

        currentSushi = Instantiate(
            sushiPrefabs[randomIndex],
            new Vector3(0, currentSpawnHeight, 0),
            Quaternion.identity
        );

        SushiPiece piece = currentSushi.GetComponent<SushiPiece>();

        piece.Initialize(
            gameController,
            this,
            gameController.highestSupportY
        );

        Rigidbody2D rb = currentSushi.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        canDrop = true;
    }

    void DropSushi()
    {
        canDrop = false;

        SushiPiece piece = currentSushi.GetComponent<SushiPiece>();
        piece.SetDropped();

        Rigidbody2D rb = currentSushi.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public void PieceSettled()
    {
        if (!stopped && !gameController.gameOver)
            SpawnSushi();
    }

    public void StopSpawning()
    {
        stopped = true;
        canDrop = false;
        CancelInvoke();
    }
}
