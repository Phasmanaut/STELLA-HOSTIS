using UnityEngine;

// Standalone dev-only testing harness: jump straight to a level's enemy layout,
// or spawn a single enemy type in isolation, without playing through the normal
// start-cube flow. Detach by deleting this component/GameObject - it touches
// nothing else and is stripped from release builds automatically.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
public class DebugTester : MonoBehaviour
{
    [Header("Enemy Test Spawn")]
    public Transform testSpawnPoint; // defaults to this object's position if left empty

    private const string Controls =
        "DEV MODE ACTIVATED\n" +
        "1-5: Jump to level\n" +
        "F1-F5: Spawn Enemy A/Ab/B/C/D";

    private GameStats gameStats;
    private bool isActive = false;

    void Start()
    {
        gameStats = GameObject.FindWithTag("GameStats").GetComponent<GameStats>();
    }

    void Update()
    {
        if (!isActive)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0)) Activate();
            return;
        }

        // Number keys: jump straight to that level's enemy layout
        if (Input.GetKeyDown(KeyCode.Alpha1)) StartLevel(1);
        if (Input.GetKeyDown(KeyCode.Alpha2)) StartLevel(2);
        if (Input.GetKeyDown(KeyCode.Alpha3)) StartLevel(3);
        if (Input.GetKeyDown(KeyCode.Alpha4)) StartLevel(4);
        if (Input.GetKeyDown(KeyCode.Alpha5)) StartLevel(5);

        // F keys: spawn a single enemy type in isolation
        if (Input.GetKeyDown(KeyCode.F1)) SpawnEnemy(gameStats.enemy_A, "Enemy A");
        if (Input.GetKeyDown(KeyCode.F2)) SpawnEnemy(gameStats.enemy_Ab, "Enemy Ab");
        if (Input.GetKeyDown(KeyCode.F3)) SpawnEnemy(gameStats.enemy_B, "Enemy B");
        if (Input.GetKeyDown(KeyCode.F4)) SpawnEnemy(gameStats.enemy_C, "Enemy C");
        if (Input.GetKeyDown(KeyCode.F5)) SpawnEnemy(gameStats.enemy_D, "Enemy D");
    }

    void Activate()
    {
        isActive = true;

        StartButton startButton = FindFirstObjectByType<StartButton>();
        if (startButton != null) startButton.gameObject.SetActive(false);

        gameStats.screenText.text = Controls;

        Debug.Log("[DebugTester] Dev mode activated");
    }

    void StartLevel(int level)
    {
        gameStats.playerAlive = true;
        gameStats.level = level;
        gameStats.LevelStart(level);
        Debug.Log($"[DebugTester] Jumped to level {level}");
    }

    void SpawnEnemy(GameObject enemyPrefab, string label)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning($"[DebugTester] {label} has no prefab assigned on GameStats.");
            return;
        }

        Vector3 pos = testSpawnPoint != null ? testSpawnPoint.position : transform.position;
        Instantiate(enemyPrefab, pos, Quaternion.identity);
        Debug.Log($"[DebugTester] Spawned {label}");
    }
}
#endif
