
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Prefab del par de vigas")]
    [SerializeField] private GameObject obstaclePrefab;

    [Header("Tus cuatro vigas originales")]
    [SerializeField] private Sprite[] vigaSprites = new Sprite[4];

    [Header("Espacio para pasar")]
    [SerializeField] private float minGap = 2.8f;
    [SerializeField] private float maxGap = 3.6f;

    [Header("Ajuste de bordes")]
    [SerializeField] private float overlap = 0.15f;

    [Header("Generacion continua")]
    [SerializeField] private float spawnX = 10.2f;
    [SerializeField] private float firstDelay = 1.35f;
    [SerializeField] private float interval = 2.2f;

    [Header("Diagnostico")]
    [SerializeField] private bool showDebug = true;

    private Camera gameCamera;
    private float timer;

    private int obstacleCount;
    private int nextTopIndex;

    private FlappyGameState lastState;
    private bool hasLoggedState;

    private void Start()
    {
        gameCamera = Camera.main;
        timer = firstDelay;

        nextTopIndex = vigaSprites != null &&
                       vigaSprites.Length > 0
            ? Random.Range(0, vigaSprites.Length)
            : 0;

        if (obstaclePrefab == null)
        {
            Debug.LogError(
                "Flappy Sushi: falta asignar PF_FlappyObstacle."
            );
        }

        if (gameCamera == null)
        {
            Debug.LogError(
                "Flappy Sushi: no se encontro Main Camera."
            );
        }
    }

    private void Update()
    {
        FlappyGameController game =
            FlappyGameController.Instance;

        if (game == null)
        {
            return;
        }

        // Registrar cuando cambia el estado del juego.
        // Esto ayuda a detectar por que se detendria.
        if (!hasLoggedState || lastState != game.State)
        {
            if (showDebug)
            {
                Debug.Log(
                    "[Flappy Spawner] Estado: " + game.State
                );
            }

            lastState = game.State;
            hasLoggedState = true;
        }

        // Mientras este jugando, generar obstaculos.
        // NO HAY LIMITE DE CANTIDAD.
        if (game.State != FlappyGameState.Playing)
        {
            return;
        }

        if (obstaclePrefab == null || gameCamera == null)
        {
            return;
        }

        timer -= Time.deltaTime;

        if (timer > 0f)
        {
            return;
        }

        Spawn();

        timer = Mathf.Max(0.5f, interval);
    }

    private void Spawn()
    {
        if (vigaSprites == null || vigaSprites.Length == 0)
        {
            Debug.LogError(
                "Flappy Sushi: asigna las cuatro vigas."
            );
            return;
        }

        // La viga superior va recorriendo los
        // diferentes sprites para asegurar variedad.
        Sprite topSprite = GetNextTopSprite(
            out int topIndex
        );

        Sprite bottomSprite = GetRandomBottomSprite(
            topIndex
        );

        if (topSprite == null || bottomSprite == null)
        {
            Debug.LogError(
                "Flappy Sushi: hay sprites sin asignar."
            );
            return;
        }

        GameObject pair = Instantiate(
            obstaclePrefab,
            new Vector3(spawnX, 0f, 0f),
            Quaternion.identity
        );

        Transform top = pair.transform.Find("Top");
        Transform bottom = pair.transform.Find("Bottom");
        Transform scoreZone = pair.transform.Find("ScoreZone");

        if (top == null || bottom == null)
        {
            Debug.LogError(
                "El prefab necesita Top y Bottom."
            );

            Destroy(pair);
            return;
        }

        SpriteRenderer topRenderer =
            top.GetComponent<SpriteRenderer>();

        SpriteRenderer bottomRenderer =
            bottom.GetComponent<SpriteRenderer>();

        if (topRenderer == null || bottomRenderer == null)
        {
            Debug.LogError(
                "Top y Bottom necesitan Sprite Renderer."
            );

            Destroy(pair);
            return;
        }

        float cameraHeight =
            gameCamera.orthographicSize * 2f;

        // Abertura diferente en cada pareja.
        float gap = Random.Range(
            Mathf.Min(minGap, maxGap),
            Mathf.Max(minGap, maxGap)
        );

        gap = Mathf.Clamp(
            gap,
            1f,
            cameraHeight - 0.5f
        );

        // Altura total que ocuparan las vigas.
        float totalHeight =
            cameraHeight - gap + (overlap * 2f);

        // Misma escala para ambas vigas:
        // se conservan las proporciones de los dibujos.
        float originalHeight =
            topSprite.bounds.size.y +
            bottomSprite.bounds.size.y;

        float commonScale =
            totalHeight / Mathf.Max(0.1f, originalHeight);

        ConfigureViga(
            top,
            topSprite,
            true,
            commonScale
        );

        ConfigureViga(
            bottom,
            bottomSprite,
            false,
            commonScale
        );

        // Pegarlas al borde superior e inferior.
        float cameraTop =
            gameCamera.transform.position.y +
            gameCamera.orthographicSize;

        float cameraBottom =
            gameCamera.transform.position.y -
            gameCamera.orthographicSize;

        float moveTop =
            cameraTop + overlap -
            topRenderer.bounds.max.y;

        top.position += Vector3.up * moveTop;

        float moveBottom =
            cameraBottom - overlap -
            bottomRenderer.bounds.min.y;

        bottom.position += Vector3.up * moveBottom;

        // Actualizar la zona de puntuacion.
        if (scoreZone != null)
        {
            float upperEdge =
                topRenderer.bounds.min.y;

            float lowerEdge =
                bottomRenderer.bounds.max.y;

            float actualGap =
                upperEdge - lowerEdge;

            Vector3 position = scoreZone.position;

            scoreZone.position = new Vector3(
                position.x,
                (upperEdge + lowerEdge) / 2f,
                position.z
            );

            BoxCollider2D scoreCollider =
                scoreZone.GetComponent<BoxCollider2D>();

            if (scoreCollider != null)
            {
                float scaleY = Mathf.Max(
                    0.01f,
                    Mathf.Abs(scoreZone.lossyScale.y)
                );

                scoreCollider.size = new Vector2(
                    scoreCollider.size.x,
                    Mathf.Max(
                        0.1f,
                        (actualGap - 0.2f) / scaleY
                    )
                );

                scoreCollider.offset = Vector2.zero;
                scoreCollider.isTrigger = true;
            }
        }

        obstacleCount++;

        if (showDebug)
        {
            Debug.Log(
                "[Flappy Spawner] Obstaculo #" +
                obstacleCount +
                " | Arriba: " + topSprite.name +
                " | Abajo: " + bottomSprite.name
            );
        }
    }

    // Recorre los sprites para que todos aparezcan.
    private Sprite GetNextTopSprite(out int selectedIndex)
    {
        selectedIndex = -1;

        for (int attempt = 0;
             attempt < vigaSprites.Length;
             attempt++)
        {
            int index =
                nextTopIndex % vigaSprites.Length;

            nextTopIndex++;

            if (vigaSprites[index] != null)
            {
                selectedIndex = index;
                return vigaSprites[index];
            }
        }

        return null;
    }

    // Escoge otra viga aleatoria para abajo.
    private Sprite GetRandomBottomSprite(int topIndex)
    {
        int chosenIndex = -1;
        int candidates = 0;

        for (int i = 0; i < vigaSprites.Length; i++)
        {
            if (vigaSprites[i] == null || i == topIndex)
            {
                continue;
            }

            candidates++;

            if (Random.Range(0, candidates) == 0)
            {
                chosenIndex = i;
            }
        }

        // Si solo existe una viga, tambien puede usarse.
        if (chosenIndex == -1)
        {
            chosenIndex = topIndex;
        }

        return chosenIndex >= 0
            ? vigaSprites[chosenIndex]
            : null;
    }

    private static void ConfigureViga(
        Transform viga,
        Sprite sprite,
        bool isTop,
        float scale
    )
    {
        SpriteRenderer renderer =
            viga.GetComponent<SpriteRenderer>();

        renderer.sprite = sprite;
        renderer.flipY = isTop;

        // Mantener la escala uniforme.
        // Se espera escala 1 en el padre del prefab.
        viga.localScale = new Vector3(
            scale,
            scale,
            1f
        );

        // Partir del centro antes de posicionar.
        Vector3 localPosition = viga.localPosition;

        viga.localPosition = new Vector3(
            localPosition.x,
            0f,
            localPosition.z
        );

        BoxCollider2D collider =
            viga.GetComponent<BoxCollider2D>();

        if (collider != null)
        {
            Bounds bounds = sprite.bounds;

            collider.size = new Vector2(
                bounds.size.x,
                bounds.size.y
            );

            collider.offset = new Vector2(
                bounds.center.x,
                isTop
                    ? -bounds.center.y
                    : bounds.center.y
            );

            collider.isTrigger = false;
        }
    }
}
